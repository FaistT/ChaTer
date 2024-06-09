using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ChatServer
{
    private static TcpListener listener;
    private static List<TcpClient> clients = new List<TcpClient>();
    private static readonly object lockObj = new object();

    static void Main(string[] args)
    {
        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Console.WriteLine("Server started, waiting for connections");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            lock (lockObj)
            {
                clients.Add(client);
            }
            Console.WriteLine("New client connected.");
            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + message);
            BroadcastMessage(message);
        }

        lock (lockObj)
        {
            clients.Remove(client);
        }
        client.Close();
        Console.WriteLine("Client disconnected.");
    }

    private static void BroadcastMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        lock (lockObj)
        {
            foreach (TcpClient client in clients)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
