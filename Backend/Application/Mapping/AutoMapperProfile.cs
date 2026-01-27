using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Service ↔ DTO
            CreateMap<Service, ServiceDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency))
                .ReverseMap()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new Money(src.Price, src.Currency)));

            // Therapist ↔ DTO
            CreateMap<Therapist, TherapistDto>().ReverseMap();

            // Course ↔ DTO
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.GetDisplayName()))
                .ForMember(dest => dest.Modality, opt => opt.MapFrom(src => src.Modality.HasValue ? src.Modality.Value.GetDisplayName() : null))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.GetDisplayName()))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price != null ? src.Price.Amount : 0))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price != null ? src.Price.Currency : "BRL"));
            
            // Reverse mapping - ignore enum fields since service layer handles parsing
            CreateMap<CourseDto, Course>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => new Money(src.Price ?? 0, src.Currency)))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Modality, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            // Category ↔ DTO
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Benefit ↔ DTO
            CreateMap<Benefit, BenefitDto>().ReverseMap();

            // Specialty ↔ DTO
            CreateMap<Specialty, SpecialtyDto>().ReverseMap();

            // Testimonial ↔ DTO
            CreateMap<Testimonial, TestimonialDto>().ReverseMap();

            // User ↔ DTO
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore());
        }
    }
}
