using AutoMapper;

using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.DTOs;

using System.Text;
using System.Security.Cryptography;

namespace DefaultProjects.Shared.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom((src, dest) => HashPassword(src.Password)))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                .ReverseMap();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
