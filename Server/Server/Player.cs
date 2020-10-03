using System;
using System.Numerics;

namespace Server
{
    public class Player
    {
        public Vector3 position;
        public Quaternion rotation;

        public string username;
        public int id;

        private bool[] inputs;
        private float moveSpeed = 5f / Constants.TICKS_PER_SECOND;

        public Player( int clientID, string name, Vector3 spawnPos)
        {
            id = clientID;
            username = name;

            position = spawnPos;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 inputDirection = Vector2.Zero;
            if ( inputs[ 0] ) // W
            {
                inputDirection.Y += 1;
            }
            if ( inputs[ 1]) // S
            {
                inputDirection.Y -= 1;
            }
            if ( inputs[ 2]) // A
            {
                inputDirection.X += 1;
            }
            if ( inputs[ 3]) // D
            {
                inputDirection.X -= 1;
            }

            Move( inputDirection);
        }

        void Move( Vector2 direction)
        {
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            Vector3 moveDirection = direction.X * right + direction.Y * forward;
            position += moveDirection * moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }
        

        public void SetInputs( bool[] movementInput, Quaternion rotation)
        {
            inputs = movementInput;
            this.rotation = rotation;
        }
    }
}
