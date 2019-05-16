using System;
using CategoryTreeGenerator.Sources;

namespace CategoryTreeGenerator
{
    internal static class Program
    {
        private const string RentFolder = "for-rent";
        private const string SaleFolder = "for-sale";

        private static void Main()
        {
            Console.WriteLine("Started");

            IDataSource source = new MockDataSource();

            //очистка папки генерации
            Clean();
            Console.WriteLine("Previous data cleaned");

            //создание корневых папок
            System.IO.Directory.CreateDirectory(RentFolder);
            System.IO.Directory.CreateDirectory(SaleFolder);

            //генерация ветви для sale
            Generator.BuildForSale(SaleFolder, source);

            //генерация ветви для rent
            Generator.BuildForRent(RentFolder, source);

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
        }
    }
}
