using Sample0202.ReflectDemo.SDK;
using System.Collections;
using System.Reflection;
using System.Runtime.Loader;

namespace Sample0202_ReflectDemo
{
    internal class Program
    {
        public static List<IAnimal> AnimalCollection = new List<IAnimal>();
        static void Main(string[] args)
        {
            Console.WriteLine("Load data...");
            LoadSDKTypes();
            while (true)
            {
                ShowAnimals();
                Console.WriteLine("F1: reload addon,F2: exit.");
                Console.WriteLine("================");

                Console.WriteLine("Choose an Animal to play with:");

                string i1 = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(i1))
                {
                    Console.WriteLine("illegal input.");
                    continue;
                }
                if (i1.Equals("F1", StringComparison.OrdinalIgnoreCase))
                {
                    LoadSDKTypes();
                    Console.WriteLine("addon reloaded.");
                    continue;
                }
                if (i1.Equals("F2", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.Exit(0);
                }

                var index = int.Parse(i1.Trim());
                Console.WriteLine("input times:");
                string i2 = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(i2))
                {
                    Console.WriteLine("illegal input.");
                    continue;
                }
                var times = int.Parse(i2.Trim());
                if (times < 1)
                {
                    Console.WriteLine("illegal input.");
                    continue;
                }
                var animal = AnimalCollection[index - 1];
                Console.Write($"{animal.Name}: ");
                animal.Voice(times);
            }
            //Console.ReadLine();
        }

        static void LoadSDKTypes()
        {
            NoticeUnload(AnimalCollection);
            AnimalCollection.Clear();
            //AnimalCollection.Add(new Sheep());
            string sdkPath = Path.Combine(Environment.CurrentDirectory, "Addons");
            var dllFiles = new List<string>();
            var currentFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");
            if (null!=currentFiles && currentFiles.Length>0)
                dllFiles.AddRange(currentFiles);
            var addonsFiles = Directory.GetFiles(sdkPath,"*.dll");
            if (null != addonsFiles && addonsFiles.Length!=0)
                dllFiles.AddRange(addonsFiles);
            if (dllFiles.Count == 0)
                return;
            foreach (var item in dllFiles)
            {
                //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(item);
                byte[] bytes = File.ReadAllBytes(item);
                var assembly = Assembly.Load(bytes);
                var animals = assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAnimal))).ToArray();
                if (animals.Length == 0)
                    continue;
                foreach (var item1 in animals)
                {
                    if(null == item1)
                        continue;
                    if (item1.GetCustomAttribute<UnFinishedAttribute>() != null)
                        continue;
                    var obj = Activator.CreateInstance(item1);
                    if (null == obj)
                        continue;
                    AnimalCollection.Add((obj as IAnimal)!);
                    if(item1.GetInterfaces().Contains(typeof(IPlugin)))
                    {
                        (obj as IPlugin)?.OnLoad();
                    }
                }
            }
        }

        static void ShowAnimals()
        {
            int i = 0;
            foreach (var item in AnimalCollection)
            {
                Console.WriteLine($"{++i}: {item.Name}");
            }
        }

        static void NoticeUnload(IEnumerable<IAnimal> animals)
        {
            if (null == animals || animals.Count() == 0)
                return;
            foreach (var item in animals)
            {
                if (item.GetType().GetInterfaces().Contains(typeof(IPlugin)))
                {
                    (item as IPlugin)?.OnRemove();
                }

            }
        }
    }
}
