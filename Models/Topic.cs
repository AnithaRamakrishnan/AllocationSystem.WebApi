
namespace AllocationSystem.WebApi.Models
{
    public class Topic:BaseEntity
    {
        public Topic()
        {
            Preferences = new HashSet<Preference>();
            Groups = new HashSet<Group>();
        }
        public long TopicID { get; set; }
        public string TopicName { get; set; }
        public virtual ICollection<Preference> Preferences { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
    }
    public class TopicDto : BaseEntityDto
    {
        public long TopicID { get; set; }
        public string TopicName { get; set; }
        public ICollection<Preference> Preferences { get; set; }
        public ICollection<Group> Groups { get; set; }
    }
    public class TopicResponseDto
    {
        public long TopicID { get; set; }
        public string TopicName { get; set; }
    }
    public class TopicListDto
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public bool IsAlreadyUsed { get; set; }
    }
}
