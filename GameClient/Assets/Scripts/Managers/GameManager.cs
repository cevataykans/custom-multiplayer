using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();
    public static Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();
    public static Dictionary<int, EnemyManager> enemies = new Dictionary<int, EnemyManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject itemSpawnerPrefab;
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void SpawnPlayer( int id, string name, Vector3 pos, Quaternion quaternion)
    {
        GameObject player;
        if ( id == Client.instance.myId)
        {
            player = Instantiate( localPlayerPrefab, pos, quaternion);
        }
        else
        {
            player = Instantiate( playerPrefab, pos, quaternion);
        }

        PlayerManager spwndPlyrMan = player.GetComponent<PlayerManager>();
        spwndPlyrMan.Initialize( id, name);

        players.Add( id, spwndPlyrMan);
    }

    public void DestroyPlayer( int playerId)
    {
        Destroy( players[playerId].gameObject);
        players.Remove( playerId);
    }

    public void CreateItemSpawner( int id, Vector3 pos, bool hasItem)
    {
        ItemSpawner itemSpawner = Instantiate(itemSpawnerPrefab, pos, itemSpawnerPrefab.transform.rotation).GetComponent<ItemSpawner>();
        itemSpawner.Initialize(id, hasItem);

        itemSpawners.Add(id, itemSpawner);
    }

    public void CreateProjectile( int id, Vector3 pos)
    {
        ProjectileManager projectile = Instantiate(projectilePrefab, pos, Quaternion.identity).GetComponent<ProjectileManager>();
        projectile.Initialize(id);

        projectiles.Add(id, projectile);
    }

    public void CreateEnemy( int id, Vector3 pos)
    {
        EnemyManager enemy = Instantiate(enemyPrefab, pos, Quaternion.identity).GetComponent<EnemyManager>();
        enemy.Initialize( id);

        enemies.Add(id, enemy);
    }
}
