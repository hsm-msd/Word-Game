/*
project: A05Client
File: MainWindow.xaml.cs
OVERVIEW: This file contains the implementation of the MainWindow class for the A05Client application.
AUTHOR: Houssemeddine Msadok
DATE: 11-18-2023
*/
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;

namespace A05Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string userName;
        private string IP;
        private int    portnumber;
        private List<string> wordschecked = new List<string>();
        private DispatcherTimer timer;
        private int secondsRemaining;
        /*
* Constructor: MainWindow
* Description: Initializes a new instance of the MainWindow class.
* Parameters: None
* Return Value: None
*/
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        /*
* Method: MainWindow_Loaded
* Description: Event handler for the Loaded event of the MainWindow.
* Parameters:
*   - object sender: The source of the event.
*   - RoutedEventArgs e: The event data.
* Return Value: None
*/
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Closing += MainWindow_Closing;
        }
        /*
* Method: MainWindow_Closing
* Description: Event handler for the Closing event of the MainWindow.
* Parameters:
*   - object sender: The source of the event.
*   - System.ComponentModel.CancelEventArgs e: The event data.
* Return Value: None
*/
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(secondsRemaining != 0)
            {
                MessageBoxResult result = MessageBox.Show("you still have time! Are you sure you want to end it ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true; // Cancel the closing event
                }
                else
                {
                    SendData(IP, portnumber, $"&{userName}&@exit@&");
                }
            }
            else
            {
                SendData(IP, portnumber, $"&{userName}&@exit@&");
            }
            
        }
        /*
* Method: StartTimer
* Description: Parses and validates the entered time format (H:M:S), then starts a timer.
* Parameters: None
* Return Value: None
*/
        private void StartTimer()
        {
            // Parse and validate the entered time format (H:M:S)
            if (TimeSpan.TryParse(timeInput.Text, out TimeSpan inputTime))
            {
                // Calculate total seconds from the parsed time
                secondsRemaining = (int)inputTime.TotalSeconds;

                // Initialize the timer
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;

                // Start the timer
                timer.Start();
            }
            else
            {
                MessageBox.Show("Invalid time format. Please enter time in the format H:M:S.");
            }
        }
        /*
* Method: Timer_Tick
* Description: Event handler for the Tick event of the timer. Updates the remaining time and handles timer expiration.
* Parameters:
*   - object sender: The source of the event.
*   - EventArgs e: The event data.
* Return Value: None
*/
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the text block with the remaining time
            timerText.Text = $"{TimeSpan.FromSeconds(secondsRemaining).ToString(@"hh\:mm\:ss")}";

            // Check if the timer has reached 0
            if (secondsRemaining == 0)
            {
                timer.Stop(); // Stop the timer when it reaches 0
                MessageBox.Show("Time's up!");
                greeting.Text = $"Ohh, Sorry for that {userName}, You can Try again now";
                checkword.IsEnabled = false;
                tryagain.Visibility = Visibility.Visible;
            }
            else
            {
                secondsRemaining--; // Decrement the remaining time
            }
        }
        /*
* Method: Button_Click
* Description: Event handler for the Click event of the Button. Validates user input and sends data to the server.
* Parameters:
*   - object sender: The source of the event.
*   - RoutedEventArgs e: The event data.
* Return Value: None
*/
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            error.Text = "";
            bool alright = true;
            if(ip.Text == "")
            {
                error.Text += "||Enter an IP address please||";alright = false;
            }
            if(port.Text == "")
            {
                error.Text += "||Enter a port please||"; alright = false;
            }
            if (username.Text == "")
            {
                error.Text += "||Enter a username please||"; alright = false;
            }
            if (!TimeSpan.TryParse(timeInput.Text, out TimeSpan inputTime))
            {
                error.Text += "||Invalid time format. Please enter time in the format H:M:S.||";
                alright = false;
            }
            if (alright)
            {
                this.IP = ip.Text;
                int.TryParse(port.Text, out this.portnumber);
                this.userName = username.Text;
                string message = $"&{userName}&&";
                SendData(IP, portnumber, message);
            }
        }
        /*
* Method: SendData
* Description: Sends data to the server using TCP connection and processes the response.
* Parameters:
*   - string server: The server IP address or hostname.
*   - int portnumber: The port number to establish the connection.
*   - string message: The message to be sent to the server.
* Return Value: None
*/
        private void SendData(String server, int portnumber, String message)
        {
            try
            {
                error.Text = "";
                Int32 port = portnumber;
                TcpClient client = new TcpClient(server, port);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                data = new Byte[256];
                String responseData = String.Empty;
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                worker(responseData);

                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                error.Text = $"ArgumentNullException: {e}";
            }
            catch (SocketException e)
            {
                error.Text = $"SocketException: {e}";
            }

        }
        /*
* Method: worker
* Description: Processes the response data received from the server.
* Parameters:
*   - string responseData: The data received from the server.
* Return Value: None
*/

        private void worker(string responseData)
        {

            if (responseData[0] == '@')
            {
                error.Text = responseData;
            }
            else
            {
                string characters = "";
                string wordsnb = "";
                ParseData(responseData, ref characters, ref wordsnb);
                if (characters != "" && wordsnb != "")
                {
                    chars.Text = characters;
                    remainwords.Text = wordsnb;
                    connect.IsEnabled = false;
                    connect.Content = "Connected";
                    checkword.IsEnabled = true;
                    tryagain.Visibility = Visibility.Collapsed;
                    greeting.Text = $"{userName}! Welcome to GuessWords Game.\nthe Characters are here below";
                    StartTimer();
                }
                else if (characters == "" && wordsnb != "")
                {
                    
                    if(remainwords.Text == wordsnb)
                    {
                        greeting.Text = "Wrong Word, Keep trying";
                    }
                    else
                    {
                        greeting.Text = "Good Job! You Get One";
                        remainwords.Text = wordsnb;
                        wordschecked.Add(wordtocheck.Text);
                        if (wordsnb == "0")
                        {
                            greeting.Text = $"Congratulation {userName}!\nYou found all the words.";
                            tryagain.Visibility = Visibility.Visible;

                        }
                    }
                }

            }
        }
        /*
* Method: ParseData
* Description: Parses the incoming message to extract CHAR and WORDNB values.
* Parameters:
*   - string message: The message to be parsed.
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
* Method: Checkword
* Description: Handles the event when the user checks a word.
* Parameters:
*   - object sender: The object that triggered the event.
*   - RoutedEventArgs e: Event arguments.
* Return Value: None
*/
        private void Checkword(object sender, RoutedEventArgs e)
        {
            error.Text = "";
            if(wordtocheck.Text == "")
            {
                error.Text = "Please enter a word to check, it can't be blank! ";
            }
            else if (wordschecked.Contains(wordtocheck.Text))
            {
                error.Text = "this word is already checked, please try another one";
            }
            else
            {
                SendData(IP, portnumber, $"&{userName}&{wordtocheck.Text}&");
            }
        }
        /*
* Method: tryAgain
* Description: Handles the event when the user chooses to try again.
* Parameters:
*   - object sender: The object that triggered the event.
*   - RoutedEventArgs e: Event arguments.
* Return Value: None
*/
        private void tryAgain(object sender, RoutedEventArgs e)
        {
            error.Text = "";
            wordtocheck.Text = "";
            wordschecked.Clear();
            SendData(IP, portnumber, $"&{userName}&{userName}&");
        }
    }
}
