using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {
    class Program {

        // This starts everything.
        static void Main(string[] args) {

            Console.Title = "GameServer";

            Server.Start(50, 26950); // (max players, port)

            Console.ReadKey(); 
        }
    }
}
