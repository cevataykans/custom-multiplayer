using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public int spawnerId;
    public bool hasItem;
    public MeshRenderer itemModel;
    public Vector3 basePosition;

    public float itemRotationSpeed = 50f;
    public float itemBobSpeed = 2f;

    private void Update()
    {
        if ( hasItem)
        {
            transform.Rotate(Vector3.up, itemRotationSpeed * Time.deltaTime, Space.World);
            transform.position = basePosition + new Vector3(0f, 0.25f * Mathf.Sin(itemBobSpeed * Time.time), 0f);
        }
    }

    public void Initialize( int id, bool hasItem)
    {
        spawnerId = id;
        this.hasItem = hasItem;
        itemModel.enabled = hasItem;

        basePosition = transform.position;
    }

    public void SpawnItem()
    {
        hasItem = true;
        itemModel.enabled = true;
    }

    public void ItemPickedUp()
    {
        hasItem = false;
        itemModel.enabled = false;
    }
}
