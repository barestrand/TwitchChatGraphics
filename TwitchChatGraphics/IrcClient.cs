// Copyright (c) 2016, Henrik Barestrand, All rights reserved.
using System.Net.Sockets;
using System.IO;

namespace TwitchBot
{
    class IrcClient
    {
        private string ip;
        private int port;
        private string userName;
        private string password;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string userName, string password)
        {
            this.ip = ip;
            this.port = port;
            this.userName = userName;
            this.password = password;

            tcpClient = new TcpClient(ip, port);
            tcpClient.NoDelay = true;
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER " + userName + " 8 * : " + userName);
            outputStream.Flush();

        }

        public void joinRoom(string channel)
        {
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void sendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void sendChatMessage(string message)
        {
            // TODO: implement event chatter (change ip)
            sendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public string readMessage()
        {

            if (!inputStream.EndOfStream)
            {
                return inputStream.ReadLine();
            }
            else
            {
                // reconnect
                tcpClient = new TcpClient(ip, port);
                tcpClient.NoDelay = true;
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                outputStream.WriteLine("PASS " + password);
                outputStream.WriteLine("NICK " + userName);
                outputStream.WriteLine("USER " + userName + " 8 * : " + userName);
                outputStream.Flush();

                joinRoom(channel);

                return inputStream.ReadLine();
            }
        }

    }
}
