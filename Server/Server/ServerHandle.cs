using System;
using System.Numerics;

namespace Server
{
    public class ServerHandle
    {
        public static void WelcomeReceived( int fromClient, Packet packet)
        {
            int clientId = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[ fromClient].tcp.socket.Client.RemoteEndPoint} is connected successfully and is now player {fromClient}");
            if ( fromClient != clientId)
            {
                // sender id has to match the client id in the server!
                Console.WriteLine("SOMETHING IS VERY VERY WRONG");
            }

            //TODO send the player into the game! done!
            Server.clients[ fromClient].SendIntoGame( username);
        }

        public static void MovementReceived( int fromClient, Packet packet)
        {
            bool[] inputs = new bool[packet.ReadInt()];
            for ( int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = packet.ReadBool();
            }

            Quaternion rotation = packet.ReadQuaternion();

            Server.clients[fromClient].clientPlayer.SetInputs( inputs, rotation);
        }


        public static void HandleUDPTestReceive( int fromClient, Packet packet)
        {
            string msg = packet.ReadString();

            Console.WriteLine($"Client: {fromClient} has sent message: {msg}");
        }
    }
}
