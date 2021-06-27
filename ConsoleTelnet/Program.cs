using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Host Name: ");
            var hostName = Console.ReadLine();
            var port = 23;
            Console.Write($"Port(default {port}): ");
            var s = Console.ReadLine();
            if (!string.IsNullOrEmpty(s))
                port = int.Parse(s);

            Console.Write("Account: ");
            var acc = Console.ReadLine();
            Console.Write("Password: ");
            var pwd = Console.ReadLine();

            var client = new TelnetClient(hostName, port);
            if (client.Connect() && client.Login(acc, pwd))
            {
                Console.WriteLine("ls -al");
                client.Command("ls -al");
                client.WaitCommandMode();
                string cmd;
                do
                {
                    cmd = Console.ReadLine();
                    client.Command(cmd);
                    client.WaitCommandMode();
                }
                while (!string.IsNullOrEmpty(cmd) && cmd != "exit");
            }
            else
                Console.WriteLine("Connect Fail..");

            AskContinue();
        }

        static void AskContinue()
        {
            Console.WriteLine();
            Console.WriteLine("press [ENTER] to continue..");
            Console.ReadLine();
        }
    }
}
