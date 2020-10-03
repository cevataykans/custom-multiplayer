using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string playername;

    public float health;
    public float maxHealth = 100f;
    public MeshRenderer model;

    public int itemCount = 0;

    public void Initialize( int id, string name)
    {
        this.id = id;
        playername = name;

        health = maxHealth;
    }

    public void SetHealth( float health)
    {
        this.health = health;
        if ( health <= 0)
        {
            Die();
        }
    }

    public void DecreaseItemCount()
    {
        itemCount--;
    }

    public void Die()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth( maxHealth);
    }

    public void PickupItem()
    {
        itemCount++;
    }
}
