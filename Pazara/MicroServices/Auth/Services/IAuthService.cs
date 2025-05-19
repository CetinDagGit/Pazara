using Pazara.MicroServices.Auth.DTOs;

namespace Pazara.MicroServices.Auth.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(UserRegisterDto userRegisterDto);
        Task<UserResponseDto> LoginAsync(UserLoginDto userLoginDto);
        Task<TokenDto> RefreshTokenAsync(string refreshToken);
    }
}
