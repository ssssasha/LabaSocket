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
        static int port = 1029; // порт для приема входящих запросов

        static void Main(string[] args)
        {
            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("The server is running. Wait for connections...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builderForFile = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        builder.Clear();
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        builderForFile.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        Console.WriteLine("User in " + DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                    } while (handler.Available > 0);


                    // записываем в журнал(файл) сообщения клиента.
                    using (FileStream fileStream =
                        new FileStream(@"/proga/LabaServer/журнал.txt", // путь к файлу, в который производится запись
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

                    // отправляем ответ
                    Console.Write("Enter the answer: ");
                    string message = Console.ReadLine();
                    IEnumerable<string> s = message.Split(20);
                    foreach (var str in s)
                    {
                        data = Encoding.Unicode.GetBytes(str);
                        handler.Send(data);
                    }

                    // закрываем сокет
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
