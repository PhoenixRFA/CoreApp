using System;
using System.Threading.Tasks;
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

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
