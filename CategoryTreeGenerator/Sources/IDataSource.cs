using System.Collections.Generic;
using CategoryTreeGenerator.Models;

namespace CategoryTreeGenerator.Sources
{
    /// <summary>
    /// Интерфейс для получения данных для генерации дерева
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Список тегов
        /// </summary>
        IEnumerable<Tag> Tags { get; }

        /// <summary>
        /// Список типов недвижимости
        /// </summary>
        IEnumerable<Type> Types { get; }

        /// <summary>
        /// Список типов недвижимости для аренды
        /// </summary>
        IEnumerable<Type> TypesForRent { get; }

        /// <summary>
        /// Список типов недвижимости для продажи
        /// </summary>
        IEnumerable<Type> TypesForSale { get; }
        
        /// <summary>
        /// Список локаций
        /// </summary>
        IEnumerable<Location> Locations { get; set; }
    }
}
