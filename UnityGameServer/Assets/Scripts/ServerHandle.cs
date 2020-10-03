using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ServerHandle
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        int clientId = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} is connected successfully and is now player {fromClient}");
        if (fromClient != clientId)
        {
            // sender id has to match the client id in the server!
            Debug.Log("SOMETHING IS VERY VERY WRONG");
        }

        //TODO send the player into the game! done!
        Server.clients[fromClient].SendIntoGame(username);
    }

    public static void MovementReceived(int fromClient, Packet packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        Server.clients[fromClient].clientPlayer.SetInputs(inputs, rotation);
    }

    public static void ShootReceived( int fromClient, Packet packet)
    {
        Vector3 shootDirection = packet.ReadVector3();

        Server.clients[fromClient].clientPlayer.Shoot( shootDirection);
    }

    public static void PlayerThrowItemReceived( int fromClient, Packet packet)
    {
        Vector3 throwDirection = packet.ReadVector3();

        Server.clients[fromClient].clientPlayer.ThrowItem( throwDirection);
    }

    public static void HandleUDPTestReceive(int fromClient, Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Client: {fromClient} has sent message: {msg}");
    }
}
