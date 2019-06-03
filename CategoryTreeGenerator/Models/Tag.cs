namespace CategoryTreeGenerator.Models
{
    public class Tag : BaseItem
    {
        public bool IsCondition { get; set; }
        public int GroupNumber { get; set; }

        public Tag()
        {
        }

        public Tag(string url, string description, string alias, bool isCondition = false, int groupNumber = 0) 
            : base(url, description, alias)
        {
            IsCondition = isCondition;
            GroupNumber = groupNumber;
        }
    }
}
