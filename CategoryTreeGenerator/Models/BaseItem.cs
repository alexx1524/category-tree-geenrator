namespace CategoryTreeGenerator.Models
{
    public class BaseItem
    {
        public string Url { get; set; }

        public string Description { get; set; }

        public string Alias { get; set; }

        public BaseItem()
        {
        }

        public BaseItem(string url, string description, string alias)
        {
            Url = url;
            Description = description;
            Alias = alias;
        }
    }
}
