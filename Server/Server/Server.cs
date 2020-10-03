using System;
using System.Collections;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        public static int MAX_PLAYER
        {
            get;
            private set;
        }
        public static int Port
        {
            get;
            private set;
        }

        public static Dictionary<int, Client> clients;
        public delegate void PacketHandler(int fromIdm, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        static TcpListener tcpListener;
        static UdpClient udpListener;

        public static void StartServer( int playerCount, int portNumber)
        {
            MAX_PLAYER = playerCount;
            Port = portNumber;
            InitializeServerData();

            Console.WriteLine("Starting server...");

            tcpListener = new TcpListener( IPAddress.Any, portNumber);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient( new AsyncCallback(TCPConnectcallBack), null);

            udpListener = new UdpClient( Port);
            udpListener.BeginReceive( UDPReceiveCallBack, null);

            Console.WriteLine( $"Server started on port: {portNumber}");
        }

        static void TCPConnectcallBack( IAsyncResult result)
        {
            // Get the connected client!
            TcpClient client = tcpListener.EndAcceptTcpClient( result);

            // Start to listen new connections again to accept new player connections!
            tcpListener.BeginAcceptTcpClient( new AsyncCallback(TCPConnectcallBack), null);
            Console.WriteLine($"Incoming connection from client: {client.Client.RemoteEndPoint}...");

            for ( int i = 1; i <= MAX_PLAYER; i++)
            {
                if ( clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect( client);
                    return;
                }
            }

            Console.WriteLine( $"{client.Client.RemoteEndPoint} failed to connect: Server is full!");
        }

        static void UDPReceiveCallBack( IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive( UDPReceiveCallBack, null);

                if ( data.Length < 4)
                {
                    return;
                }

                using ( Packet packet = new Packet( data))
                {
                    int clientId = packet.ReadInt();

                    if ( clientId <= 0)
                    {
                        Console.WriteLine("Serious problem encountered with player id: less or equal to zero!");
                        return;
                    }

                    // New connection, empty package received with end point!
                    if ( clients[ clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect( clientEndPoint);
                        return;
                    }

                    // Security
                    if ( clients[ clientId].udp.endPoint.ToString() == clientEndPoint.ToString() )
                    {
                        clients[clientId].udp.HandleData( packet);
                    }

                }
            }
            catch ( Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void SendUDPData( IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if ( clientEndPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch ( Exception e)
            {
                Console.WriteLine($"Error on sending data to client end point {clientEndPoint}, {e}");
            }
        }

        static void InitializeServerData()
        {
            clients = new Dictionary<int, Client>();
            for ( int i = 1; i <= MAX_PLAYER; i++)
            {
                clients.Add( i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.MovementReceived },



                { (int)ClientPackets.udpTestReceive, ServerHandle.HandleUDPTestReceive }

            };
        }
    }
}
