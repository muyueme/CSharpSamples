using Models.Interface;

namespace Models
{
    public class Car : IDrive
    {
        public void Run()
        {
            Console.WriteLine("Car is Running......");
        }
    }
}
