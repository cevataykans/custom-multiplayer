using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rb;
    public int thrownByPlayerId;
    public Vector3 initialForce;
    public float explosionDamage = 75f;
    public float explosionRadius = 1.5f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.ProjectileSpawned(this, thrownByPlayerId);

        rb.AddForce(initialForce);
        Invoke("ExplodeAfterTime", 5f);
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialize(Vector3 initialDirection, float initialThrowStrength, int throwByPlayer)
    {
        thrownByPlayerId = throwByPlayer;
        initialForce = initialDirection * initialThrowStrength;
    }

    void Explode()
    {
        CancelInvoke();
        ServerSend.ProjectileExploded(this);

        Collider[] coliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach ( Collider collider in coliders)
        {
            if ( collider.CompareTag( "Player"))
            {
                collider.GetComponent<Player>().TakeDamage( explosionDamage);
            }
            else if ( collider.CompareTag( "Enemy"))
            {
                collider.GetComponent<Enemy>().TakeDamage( explosionDamage);
            }
        }

        projectiles.Remove( id);
        Destroy( gameObject);
    }

    void ExlodeAfterTime()
    {
        Explode();
    }
}
