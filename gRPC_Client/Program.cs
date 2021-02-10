using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;

namespace gRPC_Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            HelloReply reply = await client.SayHelloAsync(new HelloRequest{Name = "GreeterClient"});

            Console.WriteLine("Greeting: " + reply.Message);

            PongReply ping1 = await client.PingAsync(new PingRequest
            {
                Msg = "foo"
            });
            
            Console.WriteLine("ping1 (foo):" + ping1.Msg);

            PongReply ping2 = await client.PingAsync(new PingRequest
            {
                Msg = "ping"
            });
            
            Console.WriteLine("ping2 (ping):" + ping2.Msg);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            PongReply test = await client.TestAsync(new ComplexType
            {
                En = ComplexType.Types.EnumExample.First,
                Arr = { "1", "2", "3" },
                //Remove = "remove",
                St = new ComplexType.Types.SubType
                {
                    Bar = 314,
                    Foo = "foooo"
                }
            }, new Metadata{ {"header", "some header data"} },
                DateTime.Now.ToUniversalTime().AddMinutes(5));

            Console.WriteLine("test:" + test.Msg);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            AsyncDuplexStreamingCall<PingRequest, PongReply> stream = client.StreamTest();
            await stream.RequestStream.WriteAsync(new PingRequest {Msg = "foo"});
            await stream.RequestStream.WriteAsync(new PingRequest {Msg = "fooooo"});
            await stream.RequestStream.WriteAsync(new PingRequest {Msg = "foooooooo"});

            PongReply cur = stream.ResponseStream.Current;
            Console.WriteLine($"pong: {cur?.Msg}");
            await stream.ResponseStream.MoveNext();
            cur = stream.ResponseStream.Current;
            Console.WriteLine($"pong: {cur?.Msg}");
            await stream.ResponseStream.MoveNext();
            cur = stream.ResponseStream.Current;
            Console.WriteLine($"pong: {cur?.Msg}");

            await stream.RequestStream.CompleteAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
