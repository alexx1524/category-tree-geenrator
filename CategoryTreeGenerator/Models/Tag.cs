namespace CategoryTreeGenerator.Models
{
    public class Tag : BaseItem
    {
        public bool IsCondition { get; set; }

        public Tag()
        {
        }

        public Tag(string url, string description, string alias, bool isCondition = false) 
            : base(url, description, alias)
        {
            IsCondition = isCondition;
        }
    }
}
