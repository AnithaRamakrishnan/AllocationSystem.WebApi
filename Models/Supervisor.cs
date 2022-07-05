using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllocationSystem.WebApi.Models
{
    public class Supervisor:BaseEntity
    {
        public long UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }
        public bool IsActive { get; set; }     
    }
    public class SupervisorDto : BaseEntityDto  
    {
        public long UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long ID { get; set; }
        public bool IsActive { get; set; }
    }
    public class SupervisorResult
    {
        public string groupname { get; set; }
        public string topicname { get; set; }
    }
}
