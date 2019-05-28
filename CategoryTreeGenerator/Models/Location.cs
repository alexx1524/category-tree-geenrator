using System.Collections.Generic;

namespace CategoryTreeGenerator.Models
{
    public class LocationCategories
    {
        public string CostaId { get; set; }
        public string ProvinceId { get; set; }
        public string AreaId { get; set; }
        public string CityId { get; set; }
        public string EndLocationId { get; set; }
        public string EndLocation2Id { get; set; }

        public List<string> CostaMasterData { get; set; } = new List<string>();
        public List<string> ProvinceMasterData { get; set; } = new List<string>();
        public List<string> AreaMasterData { get; set; } = new List<string>();
        public List<string> CityMasterData { get; set; } = new List<string>();
        public List<string> EndLocationMasterData { get; set; } = new List<string>();
        public List<string> EndLocation2MasterData { get; set; } = new List<string>();
    }

    public class LocationMasterData
    {
        public string CostaId { get; set; }
        public string ProvinceId { get; set; }
        public string AreaId { get; set; }
        public string CityId { get; set; }
        public string EndLocationId { get; set; }
        public string EndLocation2Id { get; set; }
    }

    public class LocationDetails : BaseItem
    {
        public string CategoryId { get; set; }

        public LocationDetails()
        {
        }

        public LocationDetails(string url, string description, string alias) : base(url, description, alias)
        {
        }

        public LocationDetails(BaseItem item)
        {
            Url = item.Url;
            Description = item.Description;
        }
    }

    public class Location
    {
        public LocationDetails Costa { get; set; }
        public LocationDetails Province { get; set; }
        public LocationDetails Area { get; set; }
        public LocationDetails City { get; set; }
        public LocationDetails EndLocation { get; set; }
        public LocationDetails EndLocation2 { get; set; }

        public Location()
        {
        }

        public Location(BaseItem costa, BaseItem province, BaseItem area, BaseItem city, BaseItem endLocation, BaseItem endLocation2)
        {
            Costa = new LocationDetails(costa);
            Province = new LocationDetails(province);
            Area = new LocationDetails(area);
            City = new LocationDetails(city);
            EndLocation = new LocationDetails(endLocation);
            EndLocation2 = new LocationDetails(endLocation2);
        }
    }
}
