using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRMS.API.Data;
using HRMS.API.DTOs;
using HRMS.API.Interfaces;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.API.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }
        string _Username = "";
        var employeeExists = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId);
        if (employeeExists!=null)
        {
            _Username = employeeExists.Name;
        }

        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful.",
            //Username = user.Username,
            Username = employeeExists?.Name ?? user.Username,
            Role = user.Role,
            EmployeeId = user.EmployeeId,
            MustChangePassword = !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) && user.MustChangePassword,
            Token = GenerateJwtToken(user)
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterUserDto request)
    {
        var existingUser = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower());

        if (existingUser)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Username already exists."
            };
        }

        if (request.EmployeeId.HasValue)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == request.EmployeeId.Value);
            if (!employeeExists)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Employee with ID {request.EmployeeId} does not exist."
                };
            }
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Employee" : request.Role,
            EmployeeId = request.EmployeeId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Success = true,
            Message = "User registered successfully.",
            Username = user.Username,
            Role = user.Role,
            EmployeeId = user.EmployeeId,
            MustChangePassword = !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) && user.MustChangePassword,
            Token = GenerateJwtToken(user)
        };
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower());

        if (user == null || !VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Current password is incorrect or user does not exist."
            };
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password changed successfully."
        };
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto request)
    {
        var identifier = request.UsernameOrEmail.Trim().ToLower();

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == identifier ||
                                     (u.Employee != null && u.Employee.Email.ToLower() == identifier));

        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "No account found matching this username or email address."
            };
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password has been reset successfully. You can now log in with your new password."
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "HRMS_DEFAULT_FALLBACK_SECRET_SECURITY_KEY_2026!";
        var issuer = jwtSettings["Issuer"] ?? "HRMS.API";
        var audience = jwtSettings["Audience"] ?? "HRMS.Client";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryInMinutes"], out var mins) ? mins : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        var hashOfEntered = HashPassword(enteredPassword);
        return hashOfEntered == storedHash;
    }
}
