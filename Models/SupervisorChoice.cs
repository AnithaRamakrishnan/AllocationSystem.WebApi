using System.ComponentModel.DataAnnotations;

namespace AllocationSystem.WebApi.Models
{
    public class SupervisorChoice : BaseEntity
    {
        [Key]
        public long ID { get; set; }
        public long SupervisorID { get; set; }
        public long TopicID{ get; set; }
        public virtual Supervisor Supervisors { get; set; }
        public virtual Topic Topics { get; set; }

    }
    public class SupervisorTopicDto
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public bool IsSelected { get; set; }
    }
}
