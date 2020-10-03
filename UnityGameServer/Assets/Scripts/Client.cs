using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public static int dataBufferSize = 4096;

    public Player clientPlayer;
    public int id;
    public TCP tcp;
    public UDP udp;

    public Client(int clientId)
    {
        id = clientId;
        clientPlayer = null;

        tcp = new TCP(clientId);
        udp = new UDP(clientId);
    }

    public void SendIntoGame(string playerName)
    {
        clientPlayer = NetworkManager.instance.InstantiatePlayer();
        clientPlayer.Initialize(id, playerName);

        foreach (Client client in Server.clients.Values)
        {
            if (client.clientPlayer != null && client.id != id)
            {
                // Send every spawned player information to the newly connected client
                ServerSend.SpawnPlayer(id, client.clientPlayer);
            }
        }

        foreach (Client client in Server.clients.Values)
        {
            if (client.clientPlayer != null)
            {
                // Send the newly connected clients data to every connected client, including the newly connected client too so that it can receive its own spawn information!
                ServerSend.SpawnPlayer(client.id, clientPlayer);
            }
        }

        foreach ( ItemSpawner itemSpawner in ItemSpawner.spawners.Values)
        {
            ServerSend.CreateItemSpawner(id, itemSpawner.spawnerId, itemSpawner.transform.position, itemSpawner.hasItem);
        }

        foreach ( Enemy enemy in Enemy.enemies.Values)
        {
            ServerSend.EnemySpawned(id, enemy);
        }
    }

    public class TCP
    {
        public TcpClient socket;

        readonly int id;
        private NetworkStream networkStream;
        private byte[] receiveBuffer;
        private Packet receivedPacket;

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient client)
        {
            socket = client;

            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            networkStream = socket.GetStream();
            receiveBuffer = new byte[dataBufferSize];
            receivedPacket = new Packet();

            networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, new AsyncCallback(ReceiveCallback), null);

            // TODO: send welcome message to client! (done)
            ServerSend.Welcome(id, "Welcome to server!");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    networkStream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error has happened on sending data to client {id}!");
                Debug.Log(e.ToString());
            }
        }

        void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = networkStream.EndRead(result);
                if (byteLength <= 0)
                {
                    // TODO: Disconnect
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] data = new byte[dataBufferSize];
                Array.Copy(receiveBuffer, data, byteLength);

                // TODO: handle data
                receivedPacket.Reset(HandleData(data));


                networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, new AsyncCallback(ReceiveCallback), null);

            }
            catch (Exception e)
            {
                Debug.Log( $"Error on client id: {id}, error is -> {e.ToString()}");
                Server.clients[id].Disconnect();
            }
        }

        bool HandleData(byte[] data)
        {
            receivedPacket.SetBytes(data);
            int packetLength = 0;

            if (receivedPacket.UnreadLength() >= 4)
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
            {
                byte[] receivedData = receivedPacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(receivedData))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });

                if (receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }
            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            networkStream = null;
            receiveBuffer = null;
            receivedPacket = null;

            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint endPoint = null;
        public int id;

        public UDP(int id)
        {
            this.id = id;
        }

        public void Connect(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            //ServerSend.UDPTest(id);
        }

        public void Disconnect()
        {
            endPoint = null;
        }

        public void SendData(Packet packet)
        {
            Server.SendUDPData(endPoint, packet);
        }

        public void HandleData(Packet _packet)
        {
            int packetLength = _packet.ReadInt();
            byte[] data = _packet.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](id, packet);
                }
            });
        }
    }

    public void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected");

        ThreadManager.ExecuteOnMainThread(() =>
       {
           UnityEngine.Object.Destroy(clientPlayer);
           clientPlayer = null;
       });

        tcp.Disconnect();
        udp.Disconnect();

        ServerSend.PlayerDisconnected( id);
    }
}
