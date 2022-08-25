using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EventBus.Messages.Events;
using Loader.API.Entities;
using MassTransit;

namespace Loader.API.EventBusConsumer
{
    //public class ParserFileConsumer : IConsumer<ParserFileEvent>
    //{
    //    private readonly IMapper _mapper;
    //    private readonly ILogger _logger;

    //    public ParserFileConsumer(ILogger<ParserFileConsumer> logger, IMapper mapper)
    //    {
    //        _logger = logger;
    //        _mapper = mapper;
    //    }

    //    public Task Consume(ConsumeContext<ParserFileEvent> context)
    //    {
    //        var command = _mapper.Map<LoaderFileCommand>(context.Message);
    //        var result = 
    //    }
    //}
}
