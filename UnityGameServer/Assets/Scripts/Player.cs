using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

public class Player : MonoBehaviour
{
    public string username;
    public int id;

    private bool[] inputs;

    public CharacterController controller;
    public float gravity = -9.81f;
    public float jumpSpeed = 5f;
    private float moveSpeed = 5f;
    public float yVelocity = 0f;
    public float throwForce = 600f;

    public float health;
    public float maxHealth = 100f;
    public Transform shootOrigin;

    public int curItemCount;
    public const int maxItemCount = 3; 

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int clientID, string name)
    {
        id = clientID;
        username = name;
        health = maxHealth;

        curItemCount = 0;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if ( health <= 0)
        {
            return;
        }

        Vector2 inputDirection = Vector2.zero;
        if (inputs[0]) // W
        {
            inputDirection.y += 1;
        }
        if (inputs[1]) // S
        {
            inputDirection.y -= 1;
        }
        if (inputs[2]) // A
        {
            inputDirection.x -= 1;
        }
        if (inputs[3]) // D
        {
            inputDirection.x += 1;
        }

        Move(inputDirection);
    }

    void Move(Vector2 direction)
    {
        Vector3 moveDirection = direction.x * transform.right + direction.y * transform.forward;
        moveDirection *= moveSpeed;

        if ( controller.isGrounded)
        {
            yVelocity = 0f;
            if ( inputs[ 4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;
        moveDirection.y = yVelocity;

        controller.Move( moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }


    public void SetInputs(bool[] movementInput, Quaternion rotation)
    {
        inputs = movementInput;
        transform.rotation = rotation;
    }

    public void Shoot( Vector3 shootDirection)
    {
        if ( health <= 0f)
        {
            return;
        }

        Debug.Log($"Player {id} is shooting.");
        if ( Physics.Raycast( shootOrigin.position, shootDirection, out RaycastHit hit, 25f))
        {
            if ( hit.collider.CompareTag( "Player") )
            {
                //Damage player, send damage data!
                Player shotPlayer = hit.collider.GetComponent<Player>();
                shotPlayer.TakeDamage(50f);
            }
            else if ( hit.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                enemy.TakeDamage(50f);
            }
        }
    }

    public void ThrowItem( Vector3 throwDirection)
    {
        if ( health <= 0 || curItemCount <= 0)
        {
            return;
        }

        curItemCount--;
        NetworkManager.instance.InstantiateProjectile( shootOrigin).Initialize( throwDirection, throwForce, id);
    }

    public void TakeDamage( float damage)
    {
        if ( health <= 0)
        {
            return;
        }

        health -= damage;
        if ( health <= 0)
        {
            health = 0;
            controller.enabled = false;
            transform.position = new Vector3(0f, 100f, 0f);

            ServerSend.PlayerPosition( this);
            Invoke( "Respawn", 5f);
        }

        ServerSend.PlayerHealth( this);
    }

    void Respawn()
    {
        health = maxHealth;
        controller.enabled = true;

        ServerSend.PlayerRespawned( this);
    }

    public bool AttempPickupItem()
    {
        if ( curItemCount >= maxItemCount)
        {
            return false;
        }

        curItemCount++;
        return true;
    }
}
