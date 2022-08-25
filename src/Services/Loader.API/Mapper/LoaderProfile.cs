using AutoMapper;
using EventBus.Messages.Events;
using Loader.API.Entities;

namespace Loader.API.Mapper
{
    public class LoaderProfile : Profile
    {
        public LoaderProfile()
        {
            CreateMap<LoaderFileCommand, ParserFileEvent>().ReverseMap();
        }
    }
}
