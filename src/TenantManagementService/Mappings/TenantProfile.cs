using AutoMapper;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.DTOs;

namespace DefaultProjects.Microservices.TenantManagementServices.Mappings;

public class TenantProfile : Profile
{
    public TenantProfile()
    {
        CreateMap<TenantCreationDTO, Tenant>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom((src, dest) => DateTime.UtcNow))
            .ReverseMap();
    }
}
