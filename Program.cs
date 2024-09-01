using System.Net.Sockets;
using System.Text;

namespace RedisDevMonitor
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Please specify your Redis Port");

            var port = Console.ReadLine();

            if (string.IsNullOrEmpty(port))
            {
                Console.WriteLine("No port specified. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            if (!int.TryParse(port, out int portNumber))
            {
                Console.WriteLine("Invalid port number. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            try
            {
                await SetRedisMonitorAsync("localhost", portNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private static async Task SetRedisMonitorAsync(string host, int port)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(host, port);
                Console.WriteLine(new string('-', 60));
                Console.WriteLine("Connected to Redis container success.");
   

                var monitorCommand = Encoding.UTF8.GetBytes("MONITOR\r\n"); // Send the MONITOR command
                socket.Send(monitorCommand);

                using (var networkStream = new NetworkStream(socket))
                using (var reader = new StreamReader(networkStream))
                {
                    Console.WriteLine("Listening for all SUBSCRIBE, UNSUBSCRIBE and PUBLISH events.");
                    Console.WriteLine(new string('-', 60));
                    while (true)
                    {
                        var line = await reader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(line))
                        {
                            if (line.Contains("SUBSCRIBE")
                                || line.Contains("UNSUBSCRIBE")
                                || line.Contains("PUBLISH"))
                            {
                                ShowEventLogs(line);
                            }
                        }

                        await Task.Delay(350);
                    }
                }
            }
        }

        private static void ShowEventLogs(string line)
        {
            if (line.Contains("UNSUBSCRIBE"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(line);
            }
            else if (line.Contains("SUBSCRIBE"))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(line);
            }
            else if (line.Contains("PUBLISH"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(line);
            }
            else
            {
                Console.ResetColor();
                Console.WriteLine(line);
            }

            Console.ResetColor();
            Console.WriteLine(new string('-', 90));
        }
    }
}