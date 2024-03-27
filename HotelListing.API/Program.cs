using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.UseAuthorization();

app.MapControllers();

app.Run();
