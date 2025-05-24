using AutoMapper;
using LinkShorter.Business.Models;
using LinkShorter.Data.Models;
using LinkShorter.Presentation.Models;

namespace LinkShorter.Business.Profiles
{
    public class ModelProfile : Profile
    {
        public ModelProfile()
        {
            CreateMap<UrlPl, UrlBl>().ReverseMap();
            CreateMap<UrlBl, UrlDl>().ReverseMap();
        }
    }
}
