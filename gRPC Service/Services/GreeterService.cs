using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace gRPC_Service
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<PongReply> Ping(PingRequest request, ServerCallContext context)
        {
            bool isOk = request.Msg == "ping";
            
            return Task.FromResult(new PongReply
            {
                Msg = isOk ? "pong" : $"wrong message: {request.Msg} (size: {request.CalculateSize()})"
            });
        }
    }
}
