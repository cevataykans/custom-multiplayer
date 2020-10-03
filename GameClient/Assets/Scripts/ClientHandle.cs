using UnityEngine;

using System.Net;

/**
 * 
 * React to messages come from the server!
 * 
 */
public class ClientHandle : MonoBehaviour
{
    public static void Welcome( Packet packet)
    {
        string msg = packet.ReadString();
        int id = packet.ReadInt();
        Client.instance.myId = id;

        Debug.Log($"Received message i: {msg} for id => {id}");
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect( ((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer( Packet packet)
    {
        int id = packet.ReadInt();
        string name = packet.ReadString();
        Vector3 pos = packet.ReadVector3();
        Quaternion quaternion = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, name, pos, quaternion);
    }

    public static void PlayerPosition( Packet packet)
    {
        int playerId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if ( GameManager.players.TryGetValue( playerId, out PlayerManager player))
        {
            player.transform.position = position;
        }
    }

    public static void PlayerRotation( Packet packet)
    {
        int playerId = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        if (GameManager.players.TryGetValue(playerId, out PlayerManager player))
        {
            player.transform.rotation = rotation;
        }
    }


    public static void PlayerDisconnect( Packet packet)
    {
        int disconnectedPlayerId = packet.ReadInt();

        GameManager.instance.DestroyPlayer(disconnectedPlayerId);
    }

    public static void RespawnPlayer( Packet packet)
    {
        int playerId = packet.ReadInt();

        GameManager.players[playerId].Respawn();
    }

    public static void PlayerHealth( Packet packet)
    {
        int playerId = packet.ReadInt();
        float playerHealth = packet.ReadFloat();

        GameManager.players[playerId].SetHealth( playerHealth);
    }

    public static void CreateItemSpawner( Packet packet)
    {
        int spawnerId = packet.ReadInt();
        Vector3 spawnerPos = packet.ReadVector3();
        bool hasItem = packet.ReadBool();

        GameManager.instance.CreateItemSpawner(spawnerId, spawnerPos, hasItem);
    }

    public static void ItemPickedUp( Packet packet)
    {
        int spawnerId = packet.ReadInt();
        int playerId = packet.ReadInt();

        GameManager.players[playerId].PickupItem();
        GameManager.itemSpawners[spawnerId].ItemPickedUp();
    }

    public static void ItemSpawned( Packet packet)
    {
        int spawnerId = packet.ReadInt();

        GameManager.itemSpawners[spawnerId].SpawnItem();
    }

    public static void HandleProjectileSpawn( Packet packet)
    {
        int projectileID = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();
        int thrownByPlayer = packet.ReadInt();

        GameManager.instance.CreateProjectile(projectileID, pos);
        GameManager.players[thrownByPlayer].DecreaseItemCount();
    }

    public static void HandleProjectilePosition( Packet packet)
    {
        int projectileID = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();

        if ( GameManager.projectiles.TryGetValue( projectileID, out ProjectileManager projectile))
        {
            projectile.transform.position = pos;
        }
    }

    public static void HandleProjectileExplosion( Packet packet)
    {
        int projectileID = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();

        GameManager.projectiles[ projectileID].Explode(pos);
    }

    public static void HandleEnemySpawn( Packet packet)
    {
        int enemyID = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();

        GameManager.instance.CreateEnemy(enemyID, pos);
    }

    public static void HandleEnemyPosition( Packet packet)
    {
        int enemyID = packet.ReadInt();
        Vector3 pos = packet.ReadVector3();

        // Since UDP is faster than TCP, sometimes UDP enemy position package can actually come before enemy spawn packet. To prevent such error, we try to get value!
        if ( GameManager.enemies.TryGetValue( enemyID, out EnemyManager enemy))
        {
            enemy.transform.position = pos;
        }
    }

    public static void HandleEnemyHealth( Packet packet)
    {
        int enemyID = packet.ReadInt();
        float hp = packet.ReadFloat();

        GameManager.enemies[enemyID].SetHealth(hp);
    }

    public static void UDPTestReceived( Packet packet)
    {
        int clientId = packet.ReadInt();

        Debug.Log($"Server message received ->  my id is {clientId} nad server sent my id is {Client.instance.myId}");
        ClientSend.UDPTestReceived();
    }
}
