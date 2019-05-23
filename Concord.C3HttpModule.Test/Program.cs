using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Concord.C3HttpModule;



namespace Concord.C3HttpModule.Test
{
    
    class Program
    {

        private static IWebServer _ws; 
        private static void Main(string[] args)
        {

            _ws = new WebServer();
            _ws.PortNumber = 8086;
            _ws.ServerAddress = "127.0.0.1";
            _ws.AllowBrowsing = true;

            Console.WriteLine(string.Format("Browsing started on {0}:{1}", _ws.ServerAddress, _ws.PortNumber));
            Console.WriteLine("Root directory : " + _ws.RootDirectory);

            Thread thread1 = new Thread(Program.ThreadFunc);
            thread1.Start();

            _ws.Start();
            Console.WriteLine(" Press a key to quit.");
            Console.ReadKey();
            _ws.Stop();
        }

        private static void ThreadFunc()
        {
            

            _ws.AddURLWithExpiry("new.html", @"C:\status.html", 0);
            Console.WriteLine(string.Format("Added a URL  {0} as {1}", @"C:\status.html", "new.html"));

            Console.WriteLine(string.Format("Adding a URL  {0} as {1}", @"/MI/7", "Buffer-of-string for 30 seconds"));
            _ws.AddURLBufferWithExpiry("/////////MI////////////////7", "This message will self destruct in 30 seconds. And you are not Ethan Hunt.", 30);
            System.Threading.Thread.Sleep(30000);
            Console.WriteLine("Access /MI/7 Again, Updating the buffer, Should retire in 30 seconds");
            _ws.AddURLBufferWithExpiry("/MI/7", "This is a new message for Ethan Hunt.", 30);


            
            _ws.AddURLBuffer("/BenHur", "<html><head></head><body> <H1>  This one stays here </H1></body></html>");
            Console.WriteLine("Access /BenHur No Expiry set for this one");


            _ws.AddURLBufferWithExpiry("//MI/8", "<html><head></head><body> <H1>  MI 8 : THIS MESSAGE WILL SELF DESTRUCT IN 30 seconds </H1></body></html>", 30);
            Console.WriteLine("Access /MI/8 with in 30 seconds from " + DateTime.Now.ToLongTimeString());

            System.Threading.Thread.Sleep(60000);
            _ws.PokeUrlWithExpiry("/MI/7", 60);
            Console.WriteLine("/MI/7 Is live again for 60 seconds from" + DateTime.Now.ToLongTimeString());

            Console.WriteLine("Making Thread Sleep for 60 seconds");
            System.Threading.Thread.Sleep(60000);


            Console.WriteLine("Browsing off for 30 seconds - Everything shall be 400");
            _ws.AllowBrowsing = false;
            System.Threading.Thread.Sleep(60000);

            Console.WriteLine("Browsing on! Back to Normal!");
            _ws.AllowBrowsing = true;
            System.Threading.Thread.Sleep(30000);

        }
    }
}
