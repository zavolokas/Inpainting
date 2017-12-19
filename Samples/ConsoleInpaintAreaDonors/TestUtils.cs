using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleInpaintAreaDonors
{
    public static class TestUtils
    {
        public static void PrintResult(bool testSuccess)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = testSuccess ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(testSuccess ? "SUCCEED" : "FAILED");
            Console.ForegroundColor = clr;
        }
    }
}
