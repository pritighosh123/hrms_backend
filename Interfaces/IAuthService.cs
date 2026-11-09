using HRMS.API.DTOs;

namespace HRMS.API.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterUserDto request);
    Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordDto request);
    Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto request);
}
