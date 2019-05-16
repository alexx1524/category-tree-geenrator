namespace CategoryTreeGenerator.Models
{
    public class BaseItem
    {
        public string Url { get; set; }
        public string Description { get; set; }

        public BaseItem()
        {
        }

        public BaseItem(string url, string description)
        {
            Url = url;
            Description = description;
        }
    }
}
