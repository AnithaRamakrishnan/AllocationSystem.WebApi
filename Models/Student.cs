using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllocationSystem.WebApi.Models
{
    public class Student:BaseEntity
    {
        public Student()
        {
            Preferences = new HashSet<Preference>();
        }
        public long UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }
        public string Course { get; set; }
        public string AcademicYear { get; set; }
        public bool IsActive { get; set; }
        public long? TopicID { get; set; }
        public long? GroupID { get; set; }
        public virtual ICollection<Preference> Preferences { get; set; }
    }
    public class StudentDto : BaseEntityDto
    {
        public long UserID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long ID { get; set; }
        public string Course { get; set; }
        public string AcademicYear { get; set; }
        public bool IsActive { get; set; }
        public long TopicID { get; set; }
        public long GroupID { get; set; }
        public ICollection<Preference> Preferences { get; set; }
    }
    public class StudentList
    {
        public long id { set; get; }
        public string label { get; set; }

    }
}
