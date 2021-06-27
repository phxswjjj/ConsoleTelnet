using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTelnet
{
    class TelnetClient : IDisposable
    {
        public string HostName { get; private set; }
        public int Port { get; private set; }
        public bool CommandMode { get; private set; }
        private TcpClient Client;

        public TelnetClient(string hostName, int port)
        {
            this.HostName = hostName;
            this.Port = port;
        }

        internal void Command(string cmd)
        {
            WriteLine(cmd);
            this.CommandMode = false;
        }

        internal void WaitCommandMode(int times = 10)
        {
            var retryTimes = 0;
            while (retryTimes < times)
            {
                Thread.Sleep(100);
                var s = Read();
                Console.Write(s);
                if (s.TrimEnd().EndsWith("%"))
                {
                    this.CommandMode = true;
                    return;
                }
                retryTimes++;
            }
        }

        internal bool Login(string acc, string pwd)
        {
            var s = Read();
            if (!s.TrimEnd().EndsWith(":"))
                return false;

            Console.Write(s);
            Console.WriteLine($"{acc}\n");
            WriteLine(acc);

            var accountResult = Read();
            if (!accountResult.TrimEnd().EndsWith(":"))
                return false;

            Console.Write(accountResult);
            Console.WriteLine($"{pwd}\n");
            WriteLine(pwd);

            WaitCommandMode(30);
            return this.CommandMode;
        }

        internal bool Connect()
        {
            var client = new TcpClient(this.HostName, this.Port);
            this.Client = client;
            return client.Connected;
        }

        internal string Read()
        {
            var client = this.Client;
            if (!client.Connected) return null;

            var stream = client.GetStream();
            var sb = new StringBuilder();
            do
            {
                int ic = stream.ReadByte();
                if (ic == -1) break;
                else if ((Verbs)ic == Verbs.IAC)
                {
                    ic = stream.ReadByte();
                    if (ic == -1) break;

                    switch ((Verbs)ic)
                    {
                        case Verbs.DO:
                        case Verbs.DONT:
                        case Verbs.WILL:
                        case Verbs.WONT:
                            var icOption = stream.ReadByte();
                            if (icOption == -1) break;
                            stream.WriteByte((byte)Verbs.IAC);
                            if ((Verbs)icOption == Verbs.SGA)
                                stream.WriteByte((Verbs)ic == Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                            else
                                stream.WriteByte((Verbs)ic == Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                            stream.WriteByte((byte)icOption);
                            Thread.Sleep(100);
                            break;
                        default:
                            sb.Append((char)ic);
                            break;
                    }
                }
                else
                    sb.Append((char)ic);
            }
            while (client.Available > 0);
            return sb.ToString();
        }

        internal void WriteLine(string s)
        {
            Write($"{s}\n");
        }
        internal void Write(string s)
        {
            var client = this.Client;
            if (!client.Connected) return;

            var stream = client.GetStream();
            byte[] data = Encoding.ASCII.GetBytes(s.Replace("\0xFF", "\0xFF\0xFF"));
            stream.Write(data, 0, data.Length);
        }

        public void Dispose()
        {
            var client = this.Client;
            if (client.Connected)
            {
                client.Close();
                client.Dispose();
            }
        }

        private enum Verbs
        {
            SGA = 3,
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            IAC = 255,
        }
    }
}
