using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookingBot.Application
{
    public class Listener
    {
        public Listener()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
        }

        private bool isListening;
        private readonly HttpListener listener;

        public void Run()
        {
            if (isListening)
                return;

            isListening = true;
            listener.Start();
            Console.WriteLine($"Now listening on http://localhost:8888/");

            Listen();
        }


        public void Listen()
        {
            Server server = new Server();
            try
            {
                while (true)
                {
                    var context = listener.GetContext();
                    server.HandleRequest(context);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.UtcNow}: {e}");
            }
        }
        
    }
}
