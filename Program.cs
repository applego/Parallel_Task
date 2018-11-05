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
            Console.WriteLine($"_5_Parallel Primes found: {primes_5_Parallel.Sum(p => p.Count)}, Total time: {sw_5_Parallel.ElapsedMilliseconds}");
            //_6_Task
            var sw_6_Tasks = new Stopwatch();
            sw_6_Tasks.Start();
            var primes_6_Tasks = new Task<List<int>>[numThreads];
            for(int i = 0; i<numThreads; i++){
                int index = i;
                primes_6_Tasks[i] = Task.Factory.StartNew(() => GetPrimeNumbers(index == 0 ? 2 : index*1000000+1, (index+1)* 1000000));
            }
            Task.WaitAll(primes_6_Tasks);
            Console.WriteLine($"_6_Tasks Primes found: {primes_6_Tasks.Sum(p => p.Result.Count)}, Total time:{sw_6_Tasks.ElapsedMilliseconds}");
            //_7_PLINQ
            var sw_7_PLINQ = new Stopwatch();
            sw_7_PLINQ.Start();
            var primes_7_PLINQ = GetPrimeNumbers_AsParallel(2,10000000);
            Console.WriteLine($"_7_PLINQ Primes found:{primes_7_PLINQ.Count},Total time:{sw_7_PLINQ.ElapsedMilliseconds}");
            //_8_Async
            ProcessPrimesAsync();
            //Console.ReadLine();
            //_9_ParallelAsync
            ProcessPrimesAsync_ParallelAsync();
            Console.ReadLine();

            //Taskを極めろ！
            var task = Task.Delay(1000); //ただのDelayなんだけど、ちょっと違って見えてきません?
            var client = new System.Net.Http.HttpClient();
            var task2 = client.GetAsync("https://www.amazon.co.jp/");//awaitをつけていない
            /*
            「タスク」であるという意味
非同期メソッドはタスクであり、作業手順書です。手順書であるということは、
それが「書かれた通りの順序で実行していけばいい」ということであり、
言い換えるとそれは「記述されたタスクの実行順序さえ間違えなければ、
誰が（どのスレッドが）どのタスクを実行したって構わない」と表明しているような事になります。
つまり、Taskとは「スレッドの存在を意識する必要がない、単なる『処理』のまとまり」だということです。
非同期処理を「タスク」の組み合わせとみなすことで、処理を分割しやすく、そして容易に合成できるようにしたもの。
それが、Taskクラスなのです。
            */
        }

        private static List<int> GetPrimeNumbers(int minimum, int maximum){
            var count = maximum - minimum + 1;
            return Enumerable.Range(minimum, count).Where(IsPrimeNumber).ToList();
        }

        private static List<int> GetPrimeNumbers_AsParallel(int minimum,int maximum){
            var count = maximum - minimum +1;
            return Enumerable.Range(minimum,count).AsParallel().Where(IsPrimeNumber).ToList();
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
        //_8_Async
        private static async void ProcessPrimesAsync(){
            var sw = new Stopwatch();
            sw.Start();
            List<int> primes = await GetPrimeNumbersAsync(2, 10000000);
            Console.WriteLine($"_8_Async Primes found:{primes.Count},Total time:{sw.ElapsedMilliseconds}");
        }
        private static async Task<List<int>> GetPrimeNumbersAsync(int minimum, int maximum){
            var count = maximum - minimum + 1;
            return await Task.Factory.StartNew(()=> Enumerable.Range(minimum,count).Where(IsPrimeNumber).ToList());
        }
        //_9_ParallelAsync
        private static async void ProcessPrimesAsync_ParallelAsync(){
            var sw = new Stopwatch();
            sw.Start();
            const int numThreads = 10;
            Task<List<int>>[] primes = new Task<List<int>>[numThreads];
            for(int i = 0; i<numThreads;i++){
                primes[i] = GetPrimeNumbersAsync(i==0 ? 2:i*1000000+1, (i+1)*1000000);
            }
            var results = await Task.WhenAll(primes);
            Console.WriteLine($"_9_ParallelAsync Primes found:{results.Sum(p => p.Count)},Total time:{sw.ElapsedMilliseconds}");
        }
        //タスクを極めろ
        async Task AsyncMethod(){
            await Task.Delay(1000);//1000ミリ秒待機するという仕事の完了を待ち、
            Console.WriteLine("Done!(IN AsyncMethod)");//Done!をコンソールに出力する
        }//という、「一つのTask」を表す。
        async Task<string> AsyncMethod2(Uri uri){
            using(var client = new System.Net.Http.HttpClient())//<- 本当はHttpClientをusingで使っちゃだめ
            {
                var response = await client.GetAsync(uri);//<- Getせよのタスクを開始し、その完了を待機する
                var text = await response.Content.ReadAsStringAsync();//レスポンスからその本文をstringとして読み出すタスクを開始し、その完了を待機する
                return text;//読み出したtextを返す
                /*
                //↑ちゃんと順番通り実行してくれるなら、
                var resonse = await client.GetAsync(uri);//このタスクを実行する人（スレッド）と
                var text = await response.content.ReadasstringAsync();//このタスクを実行する人が違ったとしても
                return text;//全く問題はない！
                 */
            }
        }//という一つのタスク（Task<string>）を表す。
    }
}
