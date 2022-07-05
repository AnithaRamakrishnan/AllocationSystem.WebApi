using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AllocationSystem.WebApi.Models
{
    //This table holds only one records
    public class AdminSetting:BaseEntity
    {   [Key]
        public int ID { get; set; }
        //set Total Number of Team Member in a Group
        public int TeamSize { get; set; }
        //set number of preferences that students can select
        public int NoOfPreferences { get; set; }
        //Provide Cut Off data for students to set the preferences
        public DateTime LastSubmissionDate { get; set; }
        public bool IsTopicMultiple { get; set; }
        public int NoOfGroups { get; set; }
        public bool IsAllocationDone { get; set; }
    }
    public class AdminSettingDto : BaseEntityDto
    {
        public int TeamSize { get; set; }
        public int NoOfPreferences { get; set; }
        public DateTime LastSubmissionDate { get; set; }
        public bool IsTopicMultiple { get; set; }
        public int NoOfGroups { get; set; }
        public bool IsAllocationDone { get; set; }
    }
    public class AdminSettingResponseDto
    {
        public int TeamSize { get; set; }
        public bool IsTeamSizeUsed { get; set; }
        public int NoOfPreferences { get; set; }
        public bool IsPreferenceSelected { get; set; }
        public DateTime LastSubmissionDate { get; set; }
        public string IsTopicMultiple { get; set; }
        public int NoOfGroups { get; set; }
        public bool IsAllocationDone { get; set; }
    }
    
}
