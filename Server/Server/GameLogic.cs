using System;

namespace Server
{
    public class GameLogic
    {
        public static void Update()
        {
            foreach ( Client client in Server.clients.Values)
            {
                if ( client.clientPlayer != null)
                {
                    client.clientPlayer.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
    }
}
