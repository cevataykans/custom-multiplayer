using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float frequency = 5f;

    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        while ( true)
        {
            yield return new WaitForSeconds( frequency);

            if ( Enemy.enemies.Count < Enemy.maxEnemyCount)
            {
                NetworkManager.instance.InstantiateEnemy(transform.position);
            }
        }
    }    
}
