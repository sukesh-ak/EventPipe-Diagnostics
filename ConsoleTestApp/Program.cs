using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting the app, wait for keyinput ...");
            System.Console.ReadLine();

            Task.Factory.StartNew(() => {

                FunctionXyz();

            });

            Task.Factory.StartNew(() => {

                DoSomeLongPendingActivity();

            });
            Console.WriteLine("Starting the app, wait for keyinput ...");
            System.Console.ReadLine();
            Console.WriteLine("Done");
        }

        private static void DoSomeLongPendingActivity()
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(IPAddress.Parse("192.168.50.1"), 5656);
            }
            catch (System.Exception)
            {

                System.Console.WriteLine("Exception while connecting");
            }

        }

        private static void FunctionXyz()
        {
            System.Console.WriteLine("Sleeping");
            Thread.Sleep((int)TimeSpan.FromMinutes(10).TotalMilliseconds);
        }
    }
}
