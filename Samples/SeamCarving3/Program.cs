using System.Collections.Generic;
using System.Linq;
using System.Text;
//using BenchmarkDotNet.Running;

namespace SeamCarving2
{
    class Program
    {
        static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<SeamCarving>();
            SeamCarving.Remove();
        }
    }
}
