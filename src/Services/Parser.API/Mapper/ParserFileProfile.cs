using AutoMapper;
using EventBus.Messages.Events;
using Parser.API.Entities;

namespace Parser.API.Mapper
{
    internal class ParserFileProfile : Profile
    {
        public ParserFileProfile()
        {
            CreateMap<ParserFile, ParserFileEvent>().ReverseMap();
        }
    }
}
