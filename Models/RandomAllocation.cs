namespace AllocationSystem.WebApi.Models
{
    public class BulkStudentsCreator
    {
        public string StudentPrefix { get; set; } = "S";
        public int NoOfStudents { get; set; }
       
    }
    public class BulkTopicsCreator
    {
        public string TopicPrefix { get; set; } = "P";
        public int NoOfTopics { get; set; }
    }
    public class BulkPreferenceCreator
    {
        public string StudentPrefix { get; set; } = "S";
        public int NoOfStudents { get; set; }
        public int NoOfPreferences { get; set; }
        public string TopicPrefix { get; set; } = "P";
        public int NoOfTopics { get; set; }
    }
}
