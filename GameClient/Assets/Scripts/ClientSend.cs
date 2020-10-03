using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * 
 * Send messages to the server!
 * 
 */
public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    public static void WelcomeReceived()
    {
        using ( Packet packet = new Packet( (int) ClientPackets.welcomeReceived ) )
        {
            packet.Write( Client.instance.myId);
            packet.Write(UIManager.instance.userNameField.text);
            SendTCPData( packet);
        }
    }

    public static void PlayerMovement( bool[] inputs)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write( inputs.Length);
            foreach ( bool input in inputs)
            {
                packet.Write( input);
            }
            packet.Write( GameManager.players[Client.instance.myId].transform.rotation);

            SendUDPData( packet);
        }
    }

    public static void PlayerShoot( Vector3 facing)
    {
        using (Packet packet = new Packet( (int)ClientPackets.playerShoot))
        {
            packet.Write(facing);

            SendTCPData(packet);
        }
    }

    public static void PlayerThrowItem( Vector3 facing)
    {
        using ( Packet packet = new Packet( (int)ClientPackets.playerThrowItem))
        {
            packet.Write(facing);

            SendTCPData( packet);
        }
    }

    // UDP support
    public static void UDPTestReceived()
    {
        using ( Packet packet = new Packet( (int)ClientPackets.udpTestReceive))
        {
            packet.Write($"My id is {Client.instance.myId} and I just connected to this server :)");

            SendUDPData( packet);
        }
    }
    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }
}
