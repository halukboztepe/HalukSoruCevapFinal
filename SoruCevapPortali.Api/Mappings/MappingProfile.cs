using AutoMapper;
using SoruCevapPortali.Api.DTOs;
using SoruCevapPortali.Api.Models;

namespace SoruCevapPortali.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<RegisterModel, ApplicationUser>();
            
            // Question mappings
            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers.Count));
            CreateMap<CreateQuestionDto, Question>();

            // Answer mappings
            CreateMap<Answer, AnswerDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));
            CreateMap<CreateAnswerDto, Answer>();
        }
    }
} 