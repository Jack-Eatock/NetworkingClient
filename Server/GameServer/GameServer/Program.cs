using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameServer {
    class Program {

        private static bool isRunning = false;

        // This starts everything.
        static void Main(string[] args) {

            Console.Title = "GameServer";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 26950); // (max players, port)

        }

        private static void MainThread() {
            Console.WriteLine($"Main Thread has started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning) {

                while (_nextLoop < DateTime.Now) {
                    GameLogic.Update();

                    // Add time till next loop 
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    // Optimisng
                    // Sleep untill time for next thread.
                    if (_nextLoop > DateTime.Now) {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
