using System;

namespace CategoryTreeGenerator.Models
{
    public class Tag : BaseItem, IComparable<Tag>
    {
        public Tag()
        {
        }

        public Tag(string url, string description) : base(url, description)
        {
        }

        public int CompareTo(Tag obj)
        {
            bool result = Url.Equals(obj.Url);

            if (result)
            {
                return 0;
            }

            return 1;
        }
    }
}