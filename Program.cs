using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace LabaClient
{
    public static class Extensions
    {
        public static IEnumerable<string> Split(this string str, int n)
        {
            if (String.IsNullOrEmpty(str) || n < 1)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < str.Length; i += n)
            {
                yield return str.Substring(i, Math.Min(n, str.Length - i));
            }
        }
    }

    class Program
    {
        // адрес і порт сервера, до якого будемо підключатись 
        static int port = 1029; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера
        static void Main(string[] args)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Console.WriteLine("Enter <exit> if you whant to go out \nEnter <Who> to get some information");
                while (true)
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(ipPoint);//  підключаємося до віддаленого сервера

                    byte[] data = new byte[256];

                    Console.Write("Enter the message: ");
                    string message = Console.ReadLine();
                    if (message == "exit")
                        break;
                    if (message == "Who")
                    {
                        Console.WriteLine("Ordak Olexandra, К-25, variant 4");
                        Console.Write("Enter the message: ");
                        message = Console.ReadLine();
                    }
                    IEnumerable<string> s = message.Split(20);
                    foreach (var str in s)
                    {
                        data = Encoding.Unicode.GetBytes(str);
                        socket.Send(data);
                    }

                    // отримуємо відповідь
                    data = new byte[256]; // буфер для відповіді
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // кількість отриманих байт

                    do
                    {
                        builder.Clear();
                        bytes = socket.Receive(data, data.Length, 0);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        Console.WriteLine("Server in " + DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                    } while (socket.Available > 0);


                    // закриваємо сокет
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }
    }
}
