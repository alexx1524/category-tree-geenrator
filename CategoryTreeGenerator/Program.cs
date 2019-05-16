using CategoryTreeGenerator.Sources;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CategoryTreeGenerator
{
    internal static class Program
    {
        private const string RootFolder = "type";

        private static void Main()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            Console.WriteLine("Started");

            IDataSource source = new MockDataSource();

            //очистка папки генерации
            Clean();

            //создание корневых папок
            Directory.CreateDirectory(RootFolder);

            Generator.Build(RootFolder, source, configuration);

            Console.WriteLine("Finished");

            Console.ReadLine();
        }

        /// <summary>
        /// Очистка предыдуших сгенерированных данных
        /// </summary>
        private static void Clean()
        {
            if (System.IO.Directory.Exists(RootFolder))
            {
                System.IO.Directory.Delete(RootFolder, true);
            }

            Console.WriteLine("Previous data cleaned");
        }
    }
}
