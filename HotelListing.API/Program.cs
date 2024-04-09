using HotelListing.API.Configurations;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options =>
{
    if (connectionString != null)
        options.UseSqlServer(connectionString);
});

// Add the Identity Core
/*
 When we're adding the Identity Core, we need it to be added relative to our user type.
"IdentityUser" comes out of the box with everything we need by default as a user type,
includling things like username, email, password, etc. even encryption.
If we want to add more properties to the user, we can create a new class that inherits from "IdentityUser".

In this case, we're creating a new class called "ApiUser" that inherits from "IdentityUser".
You SHOULD ALWAYS create a new class that inherits from "IdentityUser" to add more properties to the user.
Otherwise AddIdentityCore won't work as expected.
 */
builder.Services.AddIdentityCore<ApiUser>()
    .AddRoles<IdentityRole>() // Here you can add roles to the user
    .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingAPI") // Here we add a token provider to the user
    // Here we say use "HotelListingDbContext" as the data store for the user.
    // You can also use an external DB to put a user context related to Auth
    .AddEntityFrameworkStores<HotelListingDbContext>()
    .AddDefaultTokenProviders(); // Here we add the default token providers

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// First add the CORS service to the builder.Services
builder.Services.AddCors(options =>
{
    // Here you can specify what to allow or not
    options.AddPolicy("AllowAll", allow => allow.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});

// Add the Serilog:
// 1. Add the Serilog package to the project
// 2. Add the Serilog configuration to the builder.Host
// 3. "ctx" is the instance of the builder
// 4. "loggerConfig" is the instance of the Serilog configuration
// 5. Write to the console and also read from the configuration file (appsettings.json)
builder.Host.UseSerilog((ctx, loggerConfig) => loggerConfig.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

// Add the AutoMapper service to the builder.Services
builder.Services.AddAutoMapper(typeof(AutoMapperConfig));

// Add the Repository services to the builder.Services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(ICountriesRepository), typeof(CountriesRepository));
builder.Services.AddScoped(typeof(IHotelsRepository), typeof(HotelsRepository));
builder.Services.AddScoped(typeof(IAuthManager), typeof(AuthManager));

// Configure authentication services in the application.
builder.Services.AddAuthentication(options =>
{
    // Set the default scheme for authentication to JWT Bearer. This means the app expects the authentication
    // to be done through a JWT token.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // This line is a duplicate and should be removed or replaced with options.DefaultChallengeScheme if the intent
    // was to set the challenge scheme.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // Configure the options for JWT Bearer authentication.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Ensure the token's issuer signing key is valid.
        ValidateIssuerSigningKey = true,

        // Ensure the token's issuer is valid.
        ValidateIssuer = true,

        // Ensure the token's audience is valid.
        ValidateAudience = true,

        // Ensure the token has not expired.
        ValidateLifetime = true,

        // Set the clock skew (the allowed time difference between the server and client times) to zero,
        // to reduce the chance of expired token errors due to time discrepancies.
        ClockSkew = TimeSpan.Zero,

        // Set the valid issuer of the token to what's configured in the app's settings.
        // This is typically a URL or an identifier for the auth server.
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

        // Set the valid audience of the token to what's configured in the app's settings.
        // The audience refers to the intended recipient of the token (usually the API).
        ValidAudience = builder.Configuration["JwtSettings:Audience"],

        // Set the issuer signing key to a symmetric security key based on a secret stored in the app's settings.
        // This key is used to sign and verify JWT tokens.
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// This logs info like the request method, path, response status code, how long the request took, etc.
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Specify the CORS policy to use, in this case "AllowAll"
app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
