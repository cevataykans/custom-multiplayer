using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    // The class to define various packages to send over the network!
    public class ServerSend
    {
        #region Sending packets to a specified client or all clients depending on the info!
        public static void Welcome( int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData( toClient, packet);
            }
        }

        public static void SpawnPlayer( int toClient, Player player)
        {
            using ( Packet packet = new Packet( (int)ServerPackets.playerSpawn ))
            {
                packet.Write( player.id);
                packet.Write( player.username);
                packet.Write( player.position);
                packet.Write( player.rotation);

                SendTCPData( toClient, packet);
            }
        }

        public static void PlayerPosition( Player player)
        {
            using ( Packet packet = new Packet( (int) ServerPackets.playerPosition))
            {
                packet.Write(player.id);
                packet.Write(player.position);

                SendUDPDataToAll(packet);
            }
        }

        public static void PlayerRotation( Player player)
        {
            using ( Packet packet = new Packet( (int)ServerPackets.playerRotation))
            {
                packet.Write(player.id);
                packet.Write(player.rotation);

                SendUDPDataToAllExceptOne(player.id,  packet);
            }
        }

        public static void UDPTest(int toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.udpTest))
            {
                packet.Write(toClient);

                SendUDPData(toClient, packet);
            }
        }
        #endregion

        #region TCP Data Sending
        private static void SendTCPToAllExceptOne( int exceptClient, Packet toSend)
        {
            toSend.WriteLength();
            for ( int i = 1; i <= Server.MAX_PLAYER; i++)
            {
                if ( i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(toSend);
                }
            }
        }

        private static void SendTCPDataToAll( Packet toSend)
        {
            toSend.WriteLength();
            for ( int i = 1; i <= Server.MAX_PLAYER; i++)
            {
                Server.clients[i].tcp.SendData(toSend);
            }
        }

        private static void SendTCPData( int toClient, Packet packetToSend)
        {
            packetToSend.WriteLength();
            Server.clients[toClient].tcp.SendData( packetToSend);
        }
        #endregion

        #region UDP Sending data
        private static void SendUDPData( int toClient, Packet packetToSend)
        {
            packetToSend.WriteLength();
            Server.clients[toClient].udp.SendData(packetToSend); 
        }

        private static void SendUDPDataToAll( Packet packet)
        {
            packet.WriteLength();
            for ( int i = 1; i <= Server.MAX_PLAYER; i++)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAllExceptOne( int exceptId, Packet packet )
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYER; i++)
            {
                if ( i != exceptId)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }
        #endregion
    }
}
