using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using BCrypt.Net;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User(
                Guid.NewGuid(),
                dto.Email,
                passwordHash,
                dto.Role
            );

            if (!string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.LastName))
            {
                user.UpdateProfile(dto.FirstName, dto.LastName);
            }

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            // Check if email is being changed and if new email already exists
            if (user.Email != dto.Email && await _userRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already exists");

            // Update email if changed
            if (!string.IsNullOrWhiteSpace(dto.Email) && user.Email != dto.Email)
            {
                user.UpdateEmail(dto.Email);
            }

            // Update profile (first name, last name)
            user.UpdateProfile(dto.FirstName, dto.LastName);

            // Update role if provided
            if (dto.Role.HasValue)
            {
                user.UpdateRole(dto.Role.Value);
            }

            // Update active status if provided
            if (dto.IsActive.HasValue)
            {
                if (dto.IsActive.Value)
                    user.Activate();
                else
                    user.Deactivate();
            }

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _userRepository.ExistsAsync(id);
            if (!exists) return false;

            await _userRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(Guid id, UpdatePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.UpdatePassword(passwordHash);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
