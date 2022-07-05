
namespace AllocationSystem.WebApi.Models
{
    public class Group: BaseEntity
    {
        public long GroupID { get; set; }
        public string GroupName { get; set; }
        public long? TopicID { get; set; }
        public long? SupervisorID { get; set; }
        public virtual Topic Topics { get; set; }
    }
    public class GroupDto : BaseEntityDto
    {
        public long GroupID { get; set; }
        public string GroupName { get; set; }
        public long? TopicID { get; set; }
        public long? SupervisorID { get; set; }
        public Topic Groups { get; set; }
    }
}
