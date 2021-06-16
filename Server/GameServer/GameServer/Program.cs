using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {
    class Program {

        static void Main(string[] args) {

            Console.Title = "GameServer";

            Server.Start(50, 26950);

            Console.ReadKey();
        }
    }
}
