using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Users;

/// <summary>
/// User management. Users are never removed: deactivation is reversible, inactive users stay
/// listed (and cannot log in), and nobody can deactivate their own account.
/// </summary>
public class UserService(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<List<UserDto>> GetAllAsync() =>
        (await db.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync()).ConvertAll(u => u.ToDto());

    public async Task<UserDto?> GetByIdAsync(Guid id) =>
        (await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id))?.ToDto();

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        await EnsureEmailIsFreeAsync(dto.Email, exceptUserId: null);

        var user = new User(Guid.NewGuid(), dto.Email, BCrypt.Net.BCrypt.HashPassword(dto.Password), dto.Role);
        if (!string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.LastName))
            user.UpdateProfile(dto.FirstName, dto.LastName);

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureEmailIsFreeAsync(dto.Email, exceptUserId: id);
            user.UpdateEmail(dto.Email);
        }
        user.UpdateProfile(dto.FirstName, dto.LastName);
        if (dto.Role.HasValue) user.UpdateRole(dto.Role.Value);

        if (dto.IsActive == true) user.Activate();
        else if (dto.IsActive == false) Deactivate(user);

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>DELETE deactivates; the user and their audit history are kept.</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        Deactivate(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePasswordAsync(Guid id, UpdatePasswordDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(dto.Password));
        await db.SaveChangesAsync();
        return true;
    }

    private void Deactivate(User user)
    {
        if (user.Id == currentUser.UserId)
            throw new DomainException("You cannot deactivate your own account.");
        user.Deactivate();
    }

    /// <summary>E-mails are unique across all users, active or not (matches the DB unique index).</summary>
    private async Task EnsureEmailIsFreeAsync(string email, Guid? exceptUserId)
    {
        var normalized = email.ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalized && u.Id != exceptUserId))
            throw new ConflictException("Email already exists");
    }
}
