using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend : MonoBehaviour
{
    #region Sending packets to a specified client or all clients depending on the info!
    public static void Welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(toClient);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerSpawn))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(toClient, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAllExceptOne(player.id, packet);
        }
    }

    public static void PlayerDisconnected( int clientId)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.PlayerDisconnect))
        {
            packet.Write(clientId);

            SendTCPDataToAll( packet);
        }
    }


    public static void PlayerHealth( Player player)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.PlayerHealth))
        {
            packet.Write(player.id);
            packet.Write(player.health);

            SendTCPDataToAll( packet);
        }
    }

    public static void PlayerRespawned( Player player)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.PlayerRespawned))
        {
            packet.Write( player.id);

            SendTCPDataToAll( packet);
        }
    }

    public static void CreateItemSpawner( int toClient, int spawnerId, Vector3 spawnerPos, bool hasItem)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.CreateItemSpawner))
        {
            packet.Write(spawnerId);
            packet.Write(spawnerPos);
            packet.Write(hasItem);

            SendTCPData(toClient, packet);
        }
    }

    public static void ItemSpawned( int spawnerId)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.ItemSpawned))
        {
            packet.Write(spawnerId);

            SendTCPDataToAll( packet);
        }
    }

    public static void ItemPickedUp( int spawnerId, int playerID)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.ItemPickedUp))
        {
            packet.Write( spawnerId);
            packet.Write(playerID);

            SendTCPDataToAll( packet);
        }
    }

    public static void ProjectileSpawned( Projectile projectile, int thrownByPlayer)
    {
        using (Packet packet = new Packet((int)ServerPackets.ProjectileSpawned))
        {
            packet.Write( projectile.id);
            packet.Write( projectile.transform.position);
            packet.Write( thrownByPlayer);

            SendTCPDataToAll(packet);
        }
    }

    public static void ProjectilePosition(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ServerPackets.ProjectilePosition))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void ProjectileExploded(Projectile projectile)
    {
        using (Packet packet = new Packet((int)ServerPackets.ProjectileExploded))
        {
            packet.Write(projectile.id);
            packet.Write(projectile.transform.position);

            SendTCPDataToAll(packet);
        }
    }

    public static void EnemySpawned( Enemy enemy)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.SpawnEnemy))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.transform.position);

            SendTCPDataToAll( packet);
        }
    }

    public static void EnemySpawned( int toClient, Enemy enemy)
    {
        using (Packet packet = new Packet((int)ServerPackets.SpawnEnemy))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.transform.position);

            SendTCPData(toClient, packet);
        }
    }

    public static void EnemyPosition( Enemy enemy)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.EnemyPosition))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void EnemyHealth( Enemy enemy)
    {
        using ( Packet packet = new Packet( (int)ServerPackets.EnemyHealth))
        {
            packet.Write(enemy.id);
            packet.Write(enemy.curHealth);

            SendTCPDataToAll( packet);
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
    private static void SendTCPToAllExceptOne(int exceptClient, Packet toSend)
    {
        toSend.WriteLength();
        for (int i = 1; i <= Server.MAX_PLAYER; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(toSend);
            }
        }
    }

    private static void SendTCPDataToAll(Packet toSend)
    {
        toSend.WriteLength();
        for (int i = 1; i <= Server.MAX_PLAYER; i++)
        {
            Server.clients[i].tcp.SendData(toSend);
        }
    }

    private static void SendTCPData(int toClient, Packet packetToSend)
    {
        packetToSend.WriteLength();
        Server.clients[toClient].tcp.SendData(packetToSend);
    }
    #endregion

    #region UDP Sending data
    private static void SendUDPData(int toClient, Packet packetToSend)
    {
        packetToSend.WriteLength();
        Server.clients[toClient].udp.SendData(packetToSend);
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MAX_PLAYER; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAllExceptOne(int exceptId, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MAX_PLAYER; i++)
        {
            if (i != exceptId)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }
    #endregion
}
