/*
project: A05 – TCP/IP
File: Listener.cs
OVERVIEW: This file contains the implementation of a TCP/IP server .
AUTHOR: Houssemeddine Msadok
DATE: 11-18-2023
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace A05Server
{
    internal class Listener
    {
        class UserInfo
        {
            public string TextName { get; set; }
            public int Words { get; set; }
        }

        // Dictionary to store user information
        private Dictionary<string, UserInfo> Users = new Dictionary<string, UserInfo>();
        private static readonly object fileLock = new object();
        /*
* Method: StartListener
* Description: Initiates the TCP listener to handle incoming client requests on port 13000.
* Parameter: None
* Return Value: None
*/
        internal void StartListener()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse(GetLocalIpAddress());

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(Worker);
                    Thread clientThread = new Thread(ts);
                    clientThread.Start(client);
                }
            }
            catch (SocketException e)
            {

            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
                Users.Clear();
            }
        }
        /*
* Method: Worker
* Description: Handles the communication with a connected client in a separate thread.
* Parameter: Object o - TcpClient object representing the connected client.
* Return Value: None
*/
        public void Worker(Object o)
        {
            TcpClient client = (TcpClient)o;
            /*try
            {
                // Get the client's IP address and port.
                IPAddress clientIpAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
                Console.WriteLine($"Client connected from {clientIpAddress}:{clientPort}");

                // ... (rest of the code)

            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }*/
            Byte[] bytes = new Byte[256];
            String data = null;
            NetworkStream stream = client.GetStream();
            int i;
           
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                string userName = "";
                string word = "";
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                ParseData(data,ref  userName,ref word);
                if(userName != "" && word == "")
                {
                    lock (Users) // Use lock to ensure thread safety
                    {
                        if (!Users.ContainsKey(userName))
                        {
                            Users[userName] = new UserInfo();
                            Users[userName].TextName = GetRandomTextFile("./");
                            (string chars, int words) = ReadFromFile(Users[userName].TextName);
                            Users[userName].Words = words;
                            data = $"&{chars}&{words.ToString()}&";
                        }
                        else
                        {
                            data = "@This Username is already used and in game now, please try another one.";
                        }

                       
                    }
                }
                else if (userName == word)
                {
                    lock (Users) // Use lock to ensure thread safety
                    {
                        if (Users.ContainsKey(userName))
                        {
                            
                            Users[userName].TextName = GetRandomTextFile("./");
                            (string chars, int words) = ReadFromFile(Users[userName].TextName);
                            Users[userName].Words = words;
                            data = $"&{chars}&{words.ToString()}&";
                        }
                    }
                }
                else if(userName !="" && word =="@exit@")
                {
                    lock (Users)
                    {
                        if (Users.ContainsKey(userName))
                        {
                            Users.Remove(userName);
                        }
                    }
                }
                else 
                {
                    lock (Users)
                    {
                        if (Users.ContainsKey(userName))
                        {
                            if (CheckWordInFile(word, Users[userName].TextName))
                            {
                                Users[userName].Words = Users[userName].Words - 1;
                                data = $"&&{Users[userName].Words}&";
                            }
                            else
                            {
                                data = $"&&{Users[userName].Words}&";
                            }
                        }
                    }
                }
                
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                stream.Write(msg, 0, msg.Length);
            }

            // Shutdown and end connection
            client.Close();
        }
        /*
* Method: GetLocalIpAddress
* Description: Retrieves the local machine's IP address.
* Parameter: None
* Return Value: string - The local machine's IP address.
*/
        static string GetLocalIpAddress()
        {
            try
            {
                // Get the local machine's host name
                string hostName = Dns.GetHostName();

                // Get the host entry for the local machine
                IPHostEntry ipHostEntry = Dns.GetHostEntry(hostName);

                // Find the first IPv4 address (skipping loopback addresses)
                IPAddress localIpAddress = ipHostEntry.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));

                if (localIpAddress != null)
                {
                    return localIpAddress.ToString();
                }
                else
                {
                    return "127.0.0.1"; // Default to loopback address if none found
                }
            }
            catch (Exception ex)
            {
                return "127.0.0.1"; // Default to loopback address in case of an exception
            }
        }
        /*
* Method: ParseData
* Description: Parses a message formatted with protocol delimiters to extract CHAR and WORDNB values.
* Parameter:
*   - string message: The input message to be parsed.
*   - ref string charValue: Reference to store the extracted CHAR value.
*   - ref string wordnbValue: Reference to store the extracted WORDNB value.
* Return Value: None
*/
        static void ParseData(string message, ref string charValue, ref string wordnbValue)
        {
            // Check if the message starts and ends with the protocol delimiters
            if (message.StartsWith("&") && message.EndsWith("&"))
            {
                // Find the index of the first and last occurrence of "&"
                int firstIndex = message.IndexOf("&") + 1;
                int lastIndex = message.LastIndexOf("&");

                // Extract CHAR and WORDNB from the message
                string charAndWordnb = message.Substring(firstIndex, lastIndex - firstIndex);

                // Split the extracted string into CHAR and WORDNB using "&" as a delimiter
                string[] values = charAndWordnb.Split('&');

                charValue = values.Length > 0 ? values[0] : null;
                wordnbValue = values.Length > 1 ? values[1] : null;
            }
            else
            {
                // Set values to null if the message doesn't match the protocol
                charValue = null;
                wordnbValue = null;
            }
        }
        /*
* Method: GetRandomTextFile
* Description: Retrieves a randomly selected text file from the specified directory.
* Parameter:
*   - string directoryPath: The path of the directory containing text files.
* Return Value: string - The path of the randomly selected text file, or null if no text files are found.
*/
        static string GetRandomTextFile(string directoryPath)
        {
            try
            {
                // Get all text files in the specified directory
                string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                // Check if there are any text files in the directory
                if (textFiles.Length == 0)
                {
                    return null;
                }

                // Generate a random index to select a text file
                Random random = new Random();
                int randomIndex = random.Next(0, textFiles.Length);

                // Return the randomly selected text file
                return textFiles[randomIndex];
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /*
* Method: ReadFromFile
* Description: Reads data from a text file specified by the file path.
* Parameter:
*   - string filePath: The path of the text file to be read.
* Return Value: (string, int) - A tuple containing the 80-character string from the first line and the integer from the second line.
*                             If an error occurs during reading, default values (string.Empty, 0) are returned.
*/
        static (string, int) ReadFromFile(string filePath)
        {
            try
            {
                lock (fileLock)
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        // Read the 80-character string from the first line
                        string text = reader.ReadLine();

                        // Read the integer from the second line
                        int number = int.Parse(reader.ReadLine());

                        // Return a tuple with the retrieved values
                        return (text, number);
                    }
                }
            }
            catch (Exception ex)
            {
                // Return default values or handle the error as needed
                return (string.Empty, 0);
            }
        }
        /*
* Method: CheckWordInFile
* Description: Checks if a specified word is present in a text file.
* Parameter:
*   - string wordToFind: The word to search for in the file.
*   - string filePath: The path of the text file to be checked.
* Return Value: bool - True if the word is found in the file; false otherwise.
*/
        static bool CheckWordInFile(string wordToFind, string filePath)
        {
            filePath = filePath.Substring(2);
            try
            {
                lock (fileLock)
                {
                    // Read all lines from the file starting from the third line
                    string[] lines = File.ReadAllLines(filePath);

                    // Start checking from the third line
                    for (int i = 2; i < lines.Length; i++)
                    {
                        // Compare each word in the lines (ignoring case)
                        if (string.Equals(lines[i], wordToFind, StringComparison.OrdinalIgnoreCase))
                        {
                            // Word found
                            return true;
                        }
                    }

                    // Word not found
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle the error as needed
                return false;
            }
        }
    }
}
