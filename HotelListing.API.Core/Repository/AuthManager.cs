using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Core.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private ApiUser _user;

        // refer to properties for string values to prevent typos:
        private const string _loginProvider = "HotelListingAPI";
        private const string _refreshToken = "RefreshToken";

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, ILogger<AuthManager> logger)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<AuthResponseDTO> Login(LoginDTO loginPayload)
        {
            _logger.LogInformation($"Looking for user with email {loginPayload.Email}.");

            // Find the user by email:
            _user = await _userManager.FindByEmailAsync(loginPayload.Email);
            // Validate password:
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginPayload.Password);

            if (_user == null || isValidUser == false)
            {
                _logger.LogWarning($"User with email {loginPayload.Email} was not found.");
                return null;
            }

            var token = await GenerateToken();
            _logger.LogInformation($"Token generated for user with email {loginPayload.Email} | Token: {token}");
            return new AuthResponseDTO
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDTO userPayload)
        {
            _user = _mapper.Map<ApiUser>(userPayload);

            _user.UserName = userPayload.Email;

            // CreateAsync creates the user, encrypts the password and saves it here
            var result = await _userManager.CreateAsync(_user, userPayload.Password);

            // if success, define the user role here
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }

            return result.Errors;
        }

        public async Task<string> CreateRefreshToken()
        {
            // This line asynchronously removes the existing refresh token for a user from the database.
            // _userManager is an instance that manages user data. _user identifies the specific user,
            // _loginProvider specifies the authentication scheme (e.g., "Facebook", "Google"),
            // and _refreshToken is a string that likely holds the key or identifier for the refresh token.
            await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshToken);

            // Generates a new refresh token for the user. This token is unique and can be used to
            // maintain the user's session without them needing to re-enter their credentials.
            // The new token is stored in the variable newRefreshToken.
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider, _refreshToken);

            // Sets (saves) the newly generated refresh token for the user in the database.
            // This involves associating the newRefreshToken with the user's account in the system for future authentication.
            // The result of this operation (success or failure) is stored in the variable result, though this variable is not used later in the method.
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshToken, newRefreshToken);

            // The method returns the new refresh token. This token will be sent back to the client application,
            // which can then use it to obtain a new access token once the current access token expires,
            // without requiring the user to log in again.
            return newRefreshToken;
        }

        public async Task<AuthResponseDTO> VerifyRefreshToken(AuthResponseDTO request)
        {
            // Creates an instance of JwtSecurityTokenHandler, a class used to read, validate, and write JWTs.
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            // Reads the JWT access token from the request and extracts its content.
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);

            // Extracts the username (email in this context) from the token claims.
            // Claims are pieces of information asserted about the user, and JwtRegisteredClaimNames.Email
            // represents the claim type for the user's email address.
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            // Finds the user in the database based on the username extracted from the token.
            _user = await _userManager.FindByNameAsync(username);

            // Checks if the user does not exist or the user ID from the request does not match the user's ID.
            // If either is true, it returns null, indicating that the verification failed.
            if (_user == null || _user.Id != request.UserId)
            {
                return null;
            }

            // Verifies the refresh token. This method checks if the provided refresh token is valid for the user.
            // _userManager is an instance of a class that manages user data; _loginProvider indicates the login method;
            // _refreshToken is a key or identifier for the refresh token being verified.
            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, request.RefreshToken);

            // If the refresh token is valid, it proceeds to generate a new access token.
            if (isValidRefreshToken)
            {
                // Generates a new access token for the user.
                var token = await GenerateToken();
                // Constructs a new AuthResponseDTO object with the new token, user's ID, and a new refresh token.
                // This object will be returned to the client.
                return new AuthResponseDTO
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }

            // If the refresh token is not valid, updates the user's security stamp.
            // Updating the security stamp can invalidate existing tokens, adding an extra layer of security.
            await _userManager.UpdateSecurityStampAsync(_user);
            // Returns null to indicate the refresh token verification failed.
            return null;
        }


        private async Task<string> GenerateToken()
        {
            // Initialize a new symmetric security key using the secret key from the application's configuration.
            // This key will be used to sign the JWT token, ensuring its integrity and authenticity.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            // Create signing credentials using the above security key and specifying the HMAC SHA256 algorithm as the method used for signing the token.
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Retrieve the roles associated with the user from the user manager.
            // Roles are used in claims to enforce authorization based on user roles.
            var roles = await _userManager.GetRolesAsync(_user);

            // Create claims for each role the user has.
            // A claim is a statement about the user (e.g., name, role) that can be used by the application to make authorization decisions.
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            // Retrieve any additional claims associated with the user.
            var userClaims = await _userManager.GetClaimsAsync(_user);

            // Create a list of claims about the user that will be included in the JWT.
            // These include standardized JWT claims like 'sub' (subject, the user's email in this case) and 'jti' (JWT ID, a unique identifier for the token).
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id), // A custom claim to include the user's ID.
            }
            // Combine the above claims with any user-specific and role-specific claims.
            .Union(userClaims).Union(roleClaims);

            // Create the JWT token using the specified issuer and audience from the application's configuration,
            // including all previously defined claims, setting the token's expiration time,
            // and signing it with the specified credentials.
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
            );

            // Serialize the JWT token into a string that can be sent to the client.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
