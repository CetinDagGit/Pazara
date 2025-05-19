using Microsoft.EntityFrameworkCore;
using Pazara.MicroServices.Auth.Data;
using Pazara.MicroServices.Auth.DTOs;
using Pazara.MicroServices.Auth.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API V1");
    c.RoutePrefix = string.Empty;
});

//app.MapGet("/", () => "Hello from Pazara.Auth!");

app.MapPost("/register", async (UserRegisterDto dto, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(dto);
    return Results.Ok(result);
});

app.MapPost("/login", async (UserLoginDto dto, IAuthService authService) =>
{
    var result = await authService.LoginAsync(dto);
    return Results.Ok(result);
});

app.Run();
