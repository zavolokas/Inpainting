using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Nancy;
using Nancy.Hosting.Self;

namespace InpaintHTTP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating new server..");
            NancyHost nancyHost = new NancyHost(new Uri("http://localhost:8069"));
            nancyHost.Start();
            Console.WriteLine("Initialized and started NancyHost");

            //inf sleep to avoid main thread from finishing/terminating
            Thread.Sleep(-1); 
        }
    }
}
