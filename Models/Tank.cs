using Models.Interface;

namespace Models
{
    public class Tank : ITank
    {
        public void Attack()
        {
            Console.WriteLine("Fire, boom boom......");
        }

        public void Run()
        {
            Console.WriteLine("Tank is Running......");
        }
    }
}
