using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    public static Client instance = null;
    static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();

        InitializeClientData();
        tcp.Connect();
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>
        {
            { (int) ServerPackets.welcome, ClientHandle.Welcome },
            { (int) ServerPackets.playerSpawn, ClientHandle.SpawnPlayer },
            { (int) ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int) ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int) ServerPackets.PlayerDisconnect, ClientHandle.PlayerDisconnect},
            { (int) ServerPackets.PlayerHealth, ClientHandle.PlayerHealth},
            { (int) ServerPackets.PlayerRespawned, ClientHandle.RespawnPlayer},
            { (int) ServerPackets.CreateItemSpawner, ClientHandle.CreateItemSpawner },
            { (int) ServerPackets.ItemSpawned, ClientHandle.ItemSpawned},
            { (int) ServerPackets.ItemPickedUp, ClientHandle.ItemPickedUp},
            { (int) ServerPackets.ProjectileSpawned, ClientHandle.HandleProjectileSpawn},
            { (int) ServerPackets.ProjectilePosition, ClientHandle.HandleProjectilePosition},
            { (int) ServerPackets.ProjectileExploded, ClientHandle.HandleProjectileExplosion },
            { (int) ServerPackets.SpawnEnemy, ClientHandle.HandleEnemySpawn},
            { (int) ServerPackets.EnemyPosition, ClientHandle.HandleEnemyPosition},
            { (int) ServerPackets.EnemyHealth, ClientHandle.HandleEnemyHealth },


            { (int) ServerPackets.udpTest, ClientHandle.UDPTestReceived }
        };

        Debug.Log("Initialized packet data!");
    }

    /**
     * 
     * 
     */
    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedData;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[ dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, null);
        }

        public void SendData( Packet packet)
        {
            try
            {
                if ( socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch ( Exception e)
            {
                Debug.Log("Error on sending data to host => " + e.ToString() );
            }
        }

        private void ConnectCallback( IAsyncResult result)
        {
            socket.EndConnect(result);

            if ( !socket.Connected)
            {
                return;
            }

            instance.isConnected = true;
            stream = socket.GetStream();
            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReadCompleteCallback, null);
        }

        private void ReadCompleteCallback( IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    // TODO: Disconnect
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[dataBufferSize];
                Array.Copy(receiveBuffer, data, byteLength);

                // TODO: handle data
                receivedData.Reset( HandleData( data) );

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReadCompleteCallback, null);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error on client id: {0}, error is -> ", instance.myId, e.ToString());
                Disconnect();
            }
        }

        private bool HandleData( byte[] data)
        {
            int packetLength = 0;
            receivedData.SetBytes( data);

            if ( receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if ( packetLength <= 0)
                {
                    return true;
                }
            }

            while ( packetLength > 0 && packetLength <= receivedData.UnreadLength() )
            {
                byte[] packetBytes = receivedData.ReadBytes( packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using ( Packet packet = new Packet( packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        packetHandlers[ packetId]( packet);
                    }
                });

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if ( packetLength <= 1)
            {
                return true;
            }
            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            socket = null;
            stream = null;

            receiveBuffer = null;
            receivedData = null;
        }
    }

    /**
     * 
     * 
     */
    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect( int localPort)
        {
            socket = new UdpClient( localPort);
            socket.Connect( endPoint);

            socket.BeginReceive(ReceiveCallBack, null);

            using ( Packet packet = new Packet())
            {
                SendData( packet);
            }
        }

        public void SendData( Packet packet)
        {
            try
            {
                packet.InsertInt( instance.myId);
                if ( socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch ( Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        void ReceiveCallBack( IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive( result, ref endPoint);
                socket.BeginReceive(ReceiveCallBack, null);

                if ( data.Length < 4)
                {
                    //TODO: disconnect!
                    instance.Disconnect();
                }

                HandleData( data);
            }
            catch ( Exception e)
            {
                Debug.Log(e.ToString());
                //TODO: disconnect!
                Disconnect();
            }
        }

        void HandleData( byte[] data)
        {
            using ( Packet packet = new Packet( data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes( packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
           {
               using ( Packet packet = new Packet( data))
               {
                   int packetId = packet.ReadInt();
                   packetHandlers[packetId]( packet);
               }
           });
        }

        void Disconnect()
        {
            instance.Disconnect();

            socket = null;
            endPoint = null;
        }
    }

    void Disconnect()
    {
        if ( isConnected)
        {
            isConnected = false;

            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server!");
        }
    }
}
