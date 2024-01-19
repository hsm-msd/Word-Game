/*
project: A05 – TCP/IP
File: Program.cs
OVERVIEW: This file contains main program of a TCP/IP server .
AUTHOR: Houssemeddine Msadok
DATE: 11-18-2023
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A05Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Listener listener = new Listener();
            listener.StartListener();

            Console.WriteLine("Press Enter to End");
            Console.ReadLine();
        }
    }
}
