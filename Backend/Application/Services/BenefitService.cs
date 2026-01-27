using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BenefitService : IBenefitService
    {
        private readonly IBenefitRepository _benefitRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BenefitService(IBenefitRepository benefitRepo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _benefitRepo = benefitRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<BenefitDto>> GetAllAsync()
        {
            var benefits = await _benefitRepo.GetAllAsync();
            return _mapper.Map<List<BenefitDto>>(benefits);
        }

        public async Task<BenefitDto?> GetByIdAsync(Guid id)
        {
            var benefit = await _benefitRepo.GetByIdAsync(id);
            return benefit == null ? null : _mapper.Map<BenefitDto>(benefit);
        }

        public async Task<List<BenefitDto>> SearchAsync(string keyword)
        {
            var results = await _benefitRepo.SearchByKeywordAsync(keyword);
            return _mapper.Map<List<BenefitDto>>(results);
        }

        public async Task<BenefitDto> CreateAsync(BenefitDto dto)
        {
            var benefit = new Benefit(dto.Title, dto.Description);
            await _benefitRepo.AddAsync(benefit);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<BenefitDto>(benefit);
        }

        public async Task<bool> UpdateAsync(Guid id, BenefitDto dto)
        {
            var existing = await _benefitRepo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Update(dto.Title, dto.Description);
            await _benefitRepo.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _benefitRepo.ExistsAsync(id);
            if (!exists) return false;

            await _benefitRepo.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

    }
}
