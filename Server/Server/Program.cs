using System;
using System.Threading;

namespace Server
{
    class MainClass
    {
        static bool isRunning = false;

        public static void Main(string[] args)
        {
            Console.Beep();
            Console.Title = "Game Server";

            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart( MainThread));
            mainThread.Start();

            Server.StartServer( 50, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread has started! Running at {Constants.TICKS_PER_SECOND} ticks per second");

            DateTime nextLoop = DateTime.UtcNow;

            while ( isRunning)
            {
                while ( nextLoop < DateTime.UtcNow)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds( Constants.MS_PER_TICK);
                    if ( nextLoop > DateTime.UtcNow)
                    {
                        Thread.Sleep(nextLoop - DateTime.UtcNow);
                    }
                }
            }
        }
    }
}
