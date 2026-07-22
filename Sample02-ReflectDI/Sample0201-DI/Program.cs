using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Interface;

namespace Sample02_ReflectDI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("依赖注入");
            // 容器
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped(typeof(IDrive), typeof(Car));
            serviceCollection.AddScoped(typeof(ITank), typeof(Tank));
            //serviceCollection.AddScoped(typeof(IAttack), typeof(Tank));
            // 服务提供者
            var sp = serviceCollection.BuildServiceProvider();
            ITank tank = sp.GetService<ITank>()!;
            //IAttack tank2 = sp.GetService<IAttack>()!;
            tank.Run();
            tank.Attack();
            ITank tank2 = sp.GetService<ITank>()!;
            Console.WriteLine(tank==tank2);

            //依赖注入框架会自动处理类内部的依赖（前提：被依赖的对象被容器所管理）
            serviceCollection.Clear();
            serviceCollection.AddScoped(typeof(IDrive), typeof(Tank));
            serviceCollection.AddScoped<Driver>();
            sp = serviceCollection.BuildServiceProvider();
            var dv = sp.GetService<Driver>();
            dv?.Drive();
            Console.ReadLine();
        }
    }
}
