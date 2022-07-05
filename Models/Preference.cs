using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllocationSystem.WebApi.Models
{
    public class Preference:BaseEntity
    {
        public long StudentID { get; set; }
        public int PreferenceOrder { get; set; }
        public long TopicID { get; set; }
        [Key]
        public long PreferenceID { get; set; }
        //public virtual long StudentNumber { get; set; }
        // [ForeignKey("StudentNumber​")]
        // public Student FK_Preference_Student_StudentNumber { get; set; }
        public virtual Student Student { get; set; }
        public virtual Topic Topic { get; set; }

    }
    public class PreferenceDto : BaseEntityDto
    {        
        public long StudentNumber { get; set; }
        public int PreferenceOrder { get; set; }
        public long TopicID { get; set; }
        public long PreferenceID { get; set; }
        public Student Student { get; set; }
        public Topic Topic { get; set; }
    }
    public class SavePreferences
    {
        public long ID { get; set; }
        public List<long> topicPriorities { get; set; }

    }
    public class AdminSettingValidate
    {
        public long NoOfPreferences { get; set; }
        public bool LastSubmissionDate { get; set; }
        public bool IsAllocationDone { get; set; }
    }
    public class StudentResult
    {
        public string groupname { get; set; }
        public string topicname { get; set; }
        public List<string> Studentlist { get; set; }

    }
}
