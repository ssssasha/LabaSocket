using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace LabaServer
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
        static int port = 1029; // порт для прийому вхідних запитів

        static void Main(string[] args)
        {
            // отримуємо адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            // створюємо сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // зв'язуємо сокет з локальною точкою, по якій будемо приймати  данні
                listenSocket.Bind(ipPoint);

                // починаємо прослуховування
                listenSocket.Listen(10);

                Console.WriteLine("The server is running. Wait for connections...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    // отримуємо повідомлення
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builderForFile = new StringBuilder();
                    int bytes = 0; // кількість отриманих байтів
                    byte[] data = new byte[256]; // буфер для отримуваних данних

                    do
                    {
                        builder.Clear();
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        builderForFile.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        Console.WriteLine("User in " + DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                    } while (handler.Available > 0);


                    // записуємо  в журнал(файл) повідомлення клієнта.
                    using (FileStream fileStream =
                        new FileStream(@"/proga/LabaServer/журнал.txt", // шлях до файлу, в який проводиться запис
                            FileMode.OpenOrCreate))
                    {
                        using (StreamWriter sw = new StreamWriter(fileStream))
                        {
                            string log;
                            if (builder.ToString() == "")
                            {
                                log = $"{DateTime.Now.ToLocalTime()}: User " +
                                             $"[{IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString())}, " +
                                             $"{((IPEndPoint)handler.RemoteEndPoint).Port.ToString()}] exit from client";
                            }
                            else
                            {
                                log = $"{DateTime.Now.ToLocalTime()}: Request : \"{builderForFile}\" from " +
                                             $"[{IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString())}, " +
                                             $"{((IPEndPoint)handler.RemoteEndPoint).Port.ToString()}] client.";
                            }

                            data = Encoding.Unicode.GetBytes(log);
                            long fileEnd = fileStream.Length;

                            fileStream.Seek(fileEnd, SeekOrigin.Begin);
                            sw.WriteLine(log);
                            sw.Flush();
                        }
                    }

                    // відправляємо відповідь
                    Console.Write("Enter the answer: ");
                    string message = Console.ReadLine();
                    IEnumerable<string> s = message.Split(20);
                    foreach (var str in s)
                    {
                        data = Encoding.Unicode.GetBytes(str);
                        handler.Send(data);
                    }

                    // закриваємо сокет
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
