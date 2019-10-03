using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.ErrorHandling;

namespace InpaintAPI
{
    class WebHost
    {
        public WebHost()
        {
            NancyHost host = new NancyHost(new Uri("http://localhost:8069"));
            host.Start();
            Console.WriteLine("Initialized and started WebHost");
        }
    }

    public class MainMod : NancyModule
    {
        private static DateTime LastUpdate { get; set; }

        public MainMod()
        {
            //Test URL to see if Nancy is working correctly
            Get("/api/test", x =>
            {
                Console.WriteLine(x.ToString()); //Debugging
                return "Hello World!";
            });
        }

        public string UserInfo(string Token)
        {
            return Token;
        }

    }
    public class MyStatusHandler : IStatusCodeHandler
    {
        //TODO: return json error message?
        public bool HandlesStatusCode(global::Nancy.HttpStatusCode statusCode, NancyContext context)
        {
            return true;
        }

        public void Handle(global::Nancy.HttpStatusCode statusCode, NancyContext context)
        {
            return;
        }
    }

}
