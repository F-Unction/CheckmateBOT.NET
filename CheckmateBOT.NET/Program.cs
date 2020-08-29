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
            CheckmateBOT bot=new CheckmateBOT("KANABOT_DOTNET", "IAMBOT.NET", "Bot房",false,true);
            bot.Init();
        }
    }
}
