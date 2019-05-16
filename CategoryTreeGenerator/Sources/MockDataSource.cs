using CategoryTreeGenerator.Models;
using System.Collections.Generic;
using System.Linq;

namespace CategoryTreeGenerator.Sources
{
    public class MockDataSource : IDataSource
    {
        public IEnumerable<Location> Locations { get; set; }

        public IEnumerable<Tag> Tags { get; }

        public IEnumerable<Type> Types { get; }

        public IEnumerable<Type> TypesForRent => Types.Where(x => x.Url.Contains("for-rent"));

        public IEnumerable<Type> TypesForSale => Types.Where(x => x.Url.Contains("for-sale"));

        public MockDataSource()
        {
            Tags = new List<Tag>
            {
                new Tag("coastal", "coastal"),
                new Tag( "in-the-mountains", "in the mountains"),
                new Tag("with-swimming-pool", "with pool"),
                new Tag("with-seaview", "seaview"),
            };

            Types = new List<Type>
            {
                new Type("property-for-sale", "For sale"),
                new Type("property-for-rent", "For rent"),
                new Type("apartments-for-sale", "Apartments for sale"),
                new Type("apartments-for-rent", "Apartments for rent")
            };

            Locations = new List<Location>
            {
                new Location
                {
                    Costa = new LocationDetails("costa-blanca", "Costa Blanca"),
                    Province = new LocationDetails("alicante", "Alicante province"),
                    Area = new LocationDetails("marina-alta", "Marina Alta"),
                    City = new LocationDetails("denia", "Denia"),
                    EndLocation = new LocationDetails("loc1", "EndLocation"),
                    EndLocation2 = new LocationDetails("loc2", "EndLocation2")
                }
            };
        }
    }
}
