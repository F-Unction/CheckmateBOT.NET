using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckmateBOT.NET
{
    class Program
    {
        static void Main(string[] args)
        {
            //KANABOT_DOTNET
            //IAMBOT.NET
            Console.WriteLine("Username?");
            string username = Console.ReadLine();
            Console.WriteLine("Password?");
            string password = Console.ReadLine();
            Console.WriteLine("RoomID?");
            string roomID = Console.ReadLine();

            CheckmateBOT bot = new CheckmateBOT(username, password, roomID, false, true);
            bot.Init();
        }
    }
}
