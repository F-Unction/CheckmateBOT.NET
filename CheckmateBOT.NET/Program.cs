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
            Console.WriteLine("RoomID?");
            string roomID = Console.ReadLine();

            CheckmateBOT bot = new CheckmateBOT("KANABOT_DOTNET", "IAMBOT.NET", roomID, false, true);
            bot.Init();
        }
    }
}
