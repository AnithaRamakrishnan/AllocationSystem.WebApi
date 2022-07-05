namespace AllocationSystem.WebApi.Models
{
    public class BaseEntity
    {
        public long CreatedBy { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public long? LastUpdatedBy { get; set; }

        public DateTimeOffset? LastUpdatedDate { get; set; }
    }

    public class BaseEntityDto
    {
        public long CreatedBy { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public long? LastUpdatedBy { get; set; }

        public DateTimeOffset? LastUpdatedDate { get; set; }
    }
}
