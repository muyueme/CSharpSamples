namespace Sample0401.DBHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var helper = SqlHelper.CreateInstance();
            var isOk = helper.TestConnect();
            if (isOk)
            {
                Console.WriteLine("connect db success.");
                Console.WriteLine($"db time now: {helper.GetDbTime():yyyy-MM-dd HH:mm:ss.fff}.");
            }
            else
            {
                Console.WriteLine("fail to connect.");
            }
        }
    }
}
