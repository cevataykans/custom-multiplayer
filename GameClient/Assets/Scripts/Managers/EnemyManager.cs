using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int id;
    public float maxHealth = 100f;
    public float curHealth;

    public void Initialize( int id)
    {
        this.id = id;
        curHealth = maxHealth;
    }

    public void SetHealth( float hp)
    {
        curHealth = hp;
        if ( curHealth <= 0)
        {
            GameManager.enemies.Remove( id);
            Destroy(gameObject);
        }
    }
}
