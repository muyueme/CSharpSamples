using Sample0202.ReflectDemo.SDK;

namespace Sample0202_ReflectDemo
{
    public class Sheep : IAnimal, IPlugin
    {

        public string Name { get; set; } = "Sheep";

        public void Voice(int times)
        {
            foreach (var i in Enumerable.Range(1,times))
            {
                Console.Write("Mie~");
            }
            Console.WriteLine();
        }

        // 以下为实现 IPlugin 接口的两个方法
        public void OnLoad()
        {
            Console.WriteLine($"{Name} OnLoad");
        }

        public void OnRemove()
        {
            Console.WriteLine($"{Name} OnRemove");
        }
    }
}
