using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Contracts
{
    public interface IAuthManager
    {
        // This response logic is based on if Identity error list is empty, it means we have some success
        Task<IEnumerable<IdentityError>> Register(ApiUserDTO userDto);

        Task<AuthResponseDTO> Login(LoginDTO loginDto);

        Task<string> CreateRefreshToken();

        Task<AuthResponseDTO> VerifyRefreshToken(AuthResponseDTO request);
    }
}
