using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Transform camTransform;

    private void Update()
    {
        if ( Input.GetMouseButtonDown( 0))
        {
            Debug.Log("Shoot received!");
            ClientSend.PlayerShoot( camTransform.forward);
        }

        if ( Input.GetKeyDown( KeyCode.G) )
        {
            Debug.Log("Item throw received!");
            ClientSend.PlayerThrowItem( camTransform.forward);
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    void SendInputToServer()
    {
        bool[] inputs = new bool[]
        {
            Input.GetKey( KeyCode.W),
            Input.GetKey( KeyCode.S),
            Input.GetKey( KeyCode.A),
            Input.GetKey( KeyCode.D),
            Input.GetKey( KeyCode.Space)
        };

        ClientSend.PlayerMovement( inputs);
    }
}
