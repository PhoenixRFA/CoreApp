using System.Linq;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Google.Protobuf.Collections;

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

        public override Task<PongReply> Test(ComplexType request, ServerCallContext context)
        {
            string a = request.Remove;
            RepeatedField<string> b = request.Arr;
            string[] c = request.Arr.ToArray();
            ComplexType.Types.EnumExample d = request.En;
            ComplexType.Types.SubType e = request.St;

            return Task.FromResult(new PongReply { Msg = "pong" });
        }

        public override async Task StreamTest(IAsyncStreamReader<PingRequest> requestStream, IServerStreamWriter<PongReply> responseStream, ServerCallContext context)
        {
            PingRequest cur = requestStream.Current;
            bool next = await requestStream.MoveNext();
            cur = requestStream.Current;
            next = await requestStream.MoveNext();
            cur = requestStream.Current;

            await responseStream.WriteAsync(new PongReply {Msg = "123"});
            await responseStream.WriteAsync(new PongReply {Msg = "456"});
            await responseStream.WriteAsync(new PongReply {Msg = "789"});
        }
    }
}
