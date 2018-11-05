using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ParallelProgrammingwithCsharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //_1_Synchronous
            var sw = new Stopwatch();
            sw.Start();
            var primes = GetPrimeNumbers(2, 10000000);
            Console.WriteLine($"_1_Synchronous Primes found: {primes.Count},Total time: {sw.ElapsedMilliseconds}");
            //_5_Parallel
            var sw_5_Parallel = new Stopwatch();
            sw_5_Parallel.Start();
            const int numThreads = 10;
            var primes_5_Parallel = new List<int>[numThreads];
            Parallel.For(0,numThreads,i => primes_5_Parallel[i] = GetPrimeNumbers(i==0 ? 2:i*1000000 + 1, (i+1)*1000000));
            Console.WriteLine($"_5_Parallel Primes founc: {primes_5_Parallel.Sum(p => p.Count)}, Total time: {sw_5_Parallel.ElapsedMilliseconds}");
        }

        private static List<int> GetPrimeNumbers(int minimum, int maximum){
            var count = maximum - minimum + 1;
            return Enumerable.Range(minimum, count).Where(IsPrimeNumber).ToList();
        }

        static bool IsPrimeNumber(int p){
            if(p % 2 == 0)
                return p == 2;
            var topLimit = (int)Math.Sqrt(p);
            for(int i = 3; i <= topLimit; i += 2){
                if(p % i == 0)
                    return false;
            }
            return true;
        }
    }
}
