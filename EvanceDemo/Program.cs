using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParallelProcessPractice.Core;

namespace EvanceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new EvanceTaskRunner();
            run.ExecuteTasks(100);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
