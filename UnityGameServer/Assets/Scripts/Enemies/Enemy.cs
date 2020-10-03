using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public class Enemy : MonoBehaviour
{
    public static int maxEnemyCount = 10;
    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    private static int nextEnemyId = 1;

    public int id;
    public EnemyState enemyState;
    public Player target;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 8f;
    public float curHealth;
    public float maxHealth = 100f;
    public float detectionRange = 30f;
    public float shootRange = 15f;
    public float shootAccuracy = 0.3f;
    public float patrolDuration = 3f;
    public float idleDuration = 1f;

    private bool isPAtrolRoutineRunning = false;
    private float yVelocity = 0f;

    private void Start()
    {
        id = nextEnemyId;
        nextEnemyId++;
        enemies.Add( id, this);

        ServerSend.EnemySpawned(this);

        enemyState = EnemyState.Patrol;
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        patrolSpeed *= Time.fixedDeltaTime;
        chaseSpeed *= Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        switch( enemyState)
        {
            case EnemyState.Idle:
                LookForPlayer();
                break;
            case EnemyState.Patrol:
                if ( !LookForPlayer())
                {
                    Patrol();
                }
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            default:
                break;
        }
    }

    private bool LookForPlayer()
    {
        foreach ( Client client in Server.clients.Values)
        {
            if ( client.clientPlayer != null)
            {
                Vector3 enemyToPlayer = client.clientPlayer.transform.position - transform.position;
                if ( enemyToPlayer.magnitude <= detectionRange)
                {
                    if ( Physics.Raycast( shootOrigin.position, enemyToPlayer, out RaycastHit hit, detectionRange))
                    {
                        if ( hit.collider.CompareTag( "Player"))
                        {
                            if ( isPAtrolRoutineRunning)
                            {
                                isPAtrolRoutineRunning = false;
                                StopCoroutine(StartPatrol());
                            }

                            enemyState = EnemyState.Chase;
                            target = client.clientPlayer;
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void Chase()
    {
        if ( CanSeeTarget())
        {
            Vector3 enemyToPlayer = target.transform.position - transform.position;
            if ( enemyToPlayer.magnitude <= shootRange)
            {
                enemyState = EnemyState.Attack;
            }
            else
            {
                Move(enemyToPlayer, chaseSpeed);
            }
        }
        else
        {
            target = null;
            enemyState = EnemyState.Patrol;
        }
    }

    private void Attack()
    {
        if (CanSeeTarget())
        {
            Vector3 enemyToPlayer = target.transform.position - transform.position;
            transform.forward = new Vector3(enemyToPlayer.x, 0f, enemyToPlayer.z);

            if (enemyToPlayer.magnitude <= shootRange)
            {
                Shoot( enemyToPlayer);
            }
            else
            {
                Move(enemyToPlayer, chaseSpeed);
            }
        }
        else
        {
            target = null;
            enemyState = EnemyState.Patrol;
        }
    }

    private void Patrol()
    {
        if ( !isPAtrolRoutineRunning)
        {
            StartCoroutine(StartPatrol());
        }

        Move(transform.forward, patrolSpeed);
    }

    private void Move( Vector3 direction, float speed)
    {
        direction.y = 0;
        transform.forward = direction;
        Vector3 movement = transform.forward * speed;

        if ( controller.isGrounded)
        {
            yVelocity = 0f;
        }
        yVelocity += gravity;

        movement.y = yVelocity;
        controller.Move(movement);

        ServerSend.EnemyPosition( this);
    }

    private void Shoot( Vector3 direction)
    {
        if ( Physics.Raycast( shootOrigin.position, direction, out RaycastHit hit, shootRange))
        {
            if ( hit.collider.CompareTag( "Player"))
            {
                if ( Random.value <= shootAccuracy)
                {
                    hit.collider.GetComponent<Player>().TakeDamage(50f);
                }
            }
        }
    }

    public void TakeDamage( float dmg)
    {
        curHealth -= dmg;
        if ( curHealth <= 0)
        {
            curHealth = 0;
            enemies.Remove( id);

            Destroy( gameObject);
        }

        ServerSend.EnemyHealth(this);
    }

    private bool CanSeeTarget()
    {
        if ( target == null)
        {
            return false;
        }

        if ( Physics.Raycast( shootOrigin.position, target.transform.position, out RaycastHit hit, detectionRange))
        {
            if ( hit.collider.CompareTag( "Player"))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator StartPatrol()
    {
        isPAtrolRoutineRunning = true;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        transform.forward = new Vector3(randomDirection.x, 0f, randomDirection.y);
        yield return new WaitForSeconds( patrolDuration);

        enemyState = EnemyState.Idle;

        yield return new WaitForSeconds(idleDuration);

        enemyState = EnemyState.Patrol;
        isPAtrolRoutineRunning = false;
    }
}
