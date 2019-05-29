using CategoryTreeGenerator.Models;
using System.Collections.Generic;

namespace CategoryTreeGenerator.Sources
{
    public class MockDataSource : IDataSource
    {
        public IEnumerable<Location> Locations { get; set; }

        public IEnumerable<Tag> Tags { get; }

        public IEnumerable<Type> Types { get; }

        public MockDataSource()
        {
            Tags = new List<Tag>
            {
                new Tag("coastal", "coastal", "На море"),
                new Tag( "in-the-mountains", "in the mountains", "В горах"),
                new Tag("with-swimming-pool", "with pool", "С бассейном"),
                new Tag("with-seaview", "seaview", "С видом на море"),
            };

            Types = new List<Type>
            {
                new Type("property-for-sale", "Property for sale", "Продажа"),
                new Type("property-for-rent", "Property for rent", "Аренда"),
                new Type("apartments-for-sale", "Apartments for sale", "Апартаменты"),
                new Type("apartments-for-rent", "Apartments for rent", "Апартаменты"),
                new Type("lofts-for-sale", "Loft for sale", "Лофт","apartments-for-sale"),
                new Type("lofts-for-rent", "Loft for rent", "Лофт", "apartments-for-rent")
            };

            Locations = new List<Location>
            {
                new Location
                {
                    Costa = new LocationDetails("costa-blanca", "Costa Blanca", "Коста Бланка"),
                    Province = new LocationDetails("alicante", "Alicante province", "Аликанте"),
                    Area = new LocationDetails("marina-alta", "Marina Alta", "Марина-Альта"),
                    City = new LocationDetails("denia", "Denia", "Дения"),
                    EndLocation = null,
                    EndLocation2 = null
                },
                new Location
                {
                    Costa = new LocationDetails("costa-blanca", "Costa Blanca", "Коста Бланка"),
                    Province = new LocationDetails("alicante", "Alicante province", "Аликанте"),
                    Area = new LocationDetails("marina-alta", "Marina Alta", "Марина-Альта"),
                    City = new LocationDetails("benissa", "Benissa", "Бенисса"),
                    EndLocation = new LocationDetails("cala-advocat-baladrar", "Cala Advocat - Baladrar","Кала Адвокат - Баладар"),
                    EndLocation2 = null
                }
            };
        }
    }
}
