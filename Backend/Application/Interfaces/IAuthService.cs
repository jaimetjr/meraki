using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(Guid userId, string email, UserRole role);
    }
}

