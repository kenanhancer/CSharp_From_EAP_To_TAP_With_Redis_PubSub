using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace CSharp_From_EAP_To_TAP_With_Redis_PubSub
{
    class Program
    {
        static SimphonyService _simphonyService;
        static BroccoliService _broccoliService;

        const string _requestChannel = "SimphonyRequestChannel";
        const string _responseChannel = "SimphonyResponseChannel";

        static void Main(string[] args)
        {
            IConnectionMultiplexer _connection1 = ConnectionMultiplexer.Connect("localhost:6379");
            IConnectionMultiplexer _connection2 = ConnectionMultiplexer.Connect("localhost:6379");

            _simphonyService = new SimphonyService(_connection1, _requestChannel);

            _broccoliService = new BroccoliService(_connection2, _requestChannel, _responseChannel);

            _simphonyService.ListenRequests();

            _broccoliService.ListenResponses();

            MakeParallelRequest();

            Console.WriteLine("Hello world!");

            Console.ReadKey();
        }

        public static void MakeParallelRequest()
        {
            Action<int> act1 = async (int id) =>
            {
                string productName1 = await _broccoliService.GetMenu(id);
                Console.WriteLine($"Product Name : {productName1} from invocation {id}");
            };

            Parallel.For(0, 10, i => act1(i));
        }
    }
}