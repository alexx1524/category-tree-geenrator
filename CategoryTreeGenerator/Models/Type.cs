namespace CategoryTreeGenerator.Models
{
    public class Type : BaseItem
    {
        public string AssociatedTypeUrl { get; }

        public Type()
        {
        }

        public Type(string url, string description, string associatedTypeUrl = null) : base(url, description)
        {
            AssociatedTypeUrl = associatedTypeUrl;
        }
    }
}
