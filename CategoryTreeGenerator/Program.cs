using CategoryTreeGenerator.Sources;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CategoryTreeGenerator
{
    internal static class Program
    {
        private const string RentFolder = "for-rent";
        private const string SaleFolder = "for-sale";

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
            Directory.CreateDirectory(RentFolder);
            Directory.CreateDirectory(SaleFolder);

            //генерация ветви для sale
            Generator.BuildForSale(SaleFolder, source, configuration);

            //генерация ветви для rent
            Generator.BuildForRent(RentFolder, source, configuration);

            Console.WriteLine("Finished");

            Console.ReadLine();
        }

        /// <summary>
        /// Очистка предыдуших сгенерированных данных
        /// </summary>
        private static void Clean()
        {
            if (System.IO.Directory.Exists(RentFolder))
            {
                System.IO.Directory.Delete(RentFolder, true);
            }

            if (System.IO.Directory.Exists(SaleFolder))
            {
                System.IO.Directory.Delete(SaleFolder, true);
            }

            Console.WriteLine("Previous data cleaned");
        }
    }
}
