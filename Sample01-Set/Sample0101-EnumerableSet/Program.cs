using System.Collections;

namespace Sample0101
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var val = new StringCollection("123456789");
            Show(val);
            val[0] = '0';
            Console.WriteLine("======");
            Console.WriteLine(val[0]);
            Console.WriteLine("======");
            Console.WriteLine(val[0,5]);
            Console.WriteLine("======");
            val[0,5] = "22222";
            Console.WriteLine(val[0,5]);
            Console.ReadLine();
        }

        static void Show(IEnumerable objects)
        {
            foreach (var item in objects)
            {
                Console.WriteLine(item);
            }
        }
    }
}
