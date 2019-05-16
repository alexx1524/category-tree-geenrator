namespace CategoryTreeGenerator.Models
{
    public class LocationCategories
    {
        public string CostaId { get; set; }
        public string ProvinceId { get; set; }
        public string AreaId { get; set; }
        public string CityId { get; set; }
        public string EndLocation { get; set; }
    }

    public class LocationDetails : BaseItem
    {
        public string CategoryId { get; set; }

        public LocationDetails()
        {
        }

        public LocationDetails(string url, string description) : base(url, description)
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
