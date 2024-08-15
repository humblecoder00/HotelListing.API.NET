using Asp.Versioning;
using HotelListing.API.Core.Configurations;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.Middleware;
using HotelListing.API.Core.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Microsoft.OpenApi.Models;

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

// NOTE: For complete versioning, you need to do more specific setup
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Listing API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the next input below.
                        Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme // same as "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = "Oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// First add the CORS service to the builder.Services
builder.Services.AddCors(options =>
{
    // Here you can specify what to allow or not
    options.AddPolicy("AllowAll", allow => allow.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});


// reference: https://github.com/dotnet/aspnet-api-versioning/wiki/Migration#configuration
// Begin configuration for the API versioning service, which allows you to specify and enforce API versioning in your application.
builder.Services.AddApiVersioning(options =>
{
    // If an API version isn't specified in the client request, the service will assume the default version for the API.
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Set the default API version to 1.0. This version will be used when no specific version is requested by the client.
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);

    // Enable the API to report the supported versions via response headers. This is helpful for clients to understand which versions are available.
    options.ReportApiVersions = true;

    // Combine multiple API version readers, which means the API version can be specified by the client in several ways:
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"), // Through a query string parameter named "api-version".
        new HeaderApiVersionReader("X-Version"),        // Through a request header named "X-Version".
        new MediaTypeApiVersionReader("ver")            // Through a version parameter in the accept header media type named "ver".
    );
}) // Continue chaining the configuration.
.AddApiExplorer( // Configure the API Explorer which is used by Swagger and other tools to generate API documentation.
    options =>
    {
        // Define the format of the group name for each API version in the documentation.
        // It uses the version number and pads it with zeroes if necessary.
        options.GroupNameFormat = "'v'VVV";

        // When generating API paths in the documentation, replace the version placeholder with the actual version number.
        options.SubstituteApiVersionInUrl = true;
    }
);


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

// Configure response caching services
builder.Services.AddResponseCaching(options =>
    {
        // Software level: Set maximum response body size that can be cached (in bytes).
        options.MaximumBodySize = 1024;
        // Software level: Enable case-sensitive paths for caching,
        // meaning 'api/v1/Values' is treated differently from 'api/v1/values'.
        options.UseCaseSensitivePaths = true;
    }
);

// Add the OData service to the builder.Services
builder.Services.AddControllers().AddOData(options =>
{
    options.Select().Filter().OrderBy();
});


var app = builder.Build();

// These below are middlewares

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// This middleware is used to handle exceptions globally (custom)
app.UseMiddleware<ExceptionMiddleware>();

// This logs info like the request method, path, response status code, how long the request took, etc.
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Specify the CORS policy to use, in this case "AllowAll"
app.UseCors("AllowAll");

// Enable response caching middleware which attempts to serve requests directly from the cache if applicable.
app.UseResponseCaching();

// Directly adding a custom middleware. This example sets specific cache control headers on the response.
// (does same thing like adding ExceptionMiddleware above)
// ---
// Enable response caching middleware which attempts to serve requests directly from the cache if applicable.
// Software level: This middleware handles storing and retrieving cached responses internally within the app.
app.Use(async(context, next) =>
{
    // Network level: Configure cache control headers for the response.
    // This influences external caches (like browser caches and proxy servers).
    context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
    {
        Public = true, // Indicates the response can be cached by any cache (browser, proxy, etc.)
        MaxAge = TimeSpan.FromSeconds(10) // get new data every 10 seconds - this is just an example. find a balance between performance and freshness
    };

    // Network level: Configure the 'Vary' header to ensure different versions of response are cached based on 'Accept-Encoding'.
    // This is critical for correctly caching responses that may vary based on content encoding.
    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
        new string[] { "Accept Encoding" };

    // Continue processing the request in the middleware pipeline.
    await next();
}
);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
