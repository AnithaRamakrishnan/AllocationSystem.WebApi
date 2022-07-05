namespace AllocationSystem.WebApi.Models
{
    public class MatchingAlgorithm
    {
    }

    public class AllocatedTopic
    {
        public string GroupName { get; set; }
        public long TopicID { get; set; }
        public long SupervisorID { get; set; }
        public List<long> StudentID { get; set; }
    }

    public class SupChoice
    {
        public long TopicID { get; set; }
        public bool IsAssigned { get; set; }
    }
    public class Result
    {
        public string GroupName { get; set; }
        public string TopicName { get; set; }
        public string SupervisorName { get; set; }

        public List<AllocatedStudents> Students { get; set; }
    }
    public class AllocatedStudents
    {
        public string StudentName { get; set; }
        public int? MatchedPreferenceOrder { get; set; }
    }
    public class PreferenceProvidedStudents
    {
        public int Submitted { get; set; }
        public int NotSubmitted { get; set; }
    }
    public class LikedTopics
    {
        public string TopicName { get; set; }
        public double Percentage { get; set; }
    }
    public class StudentsFinalChoice
    {
        public int? Preference { get; set; } = 0;
        public int Count { get; set; } = 0;
    }
    public class Total
    {
        public long Students { get; set; }
        public long Topics { get; set; }
        public long Groups { get; set; }
    }
}
