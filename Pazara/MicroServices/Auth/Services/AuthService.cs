using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Pazara.MicroServices.Auth.Data;
using Pazara.MicroServices.Auth.DTOs;
using Pazara.MicroServices.Auth.Models;
using Pazara.MicroServices.Auth.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<UserResponseDto> RegisterAsync(UserRegisterDto userRegisterDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == userRegisterDto.Username))
            throw new Exception("Username already exists.");

        var user = new User
        {
            Username = userRegisterDto.Username,
            Email = userRegisterDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<UserResponseDto> LoginAsync(UserLoginDto userLoginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == userLoginDto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.PasswordHash))
            throw new Exception("Invalid username or password.");

        var token = GenerateJwtToken(user);

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<TokenDto> RefreshTokenAsync(string refreshToken)
    {
        //Todo: Implement refresh token logic
        throw new NotImplementedException();
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("id", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
