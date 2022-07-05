using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AllocationSystem.WebApi.Models
{
    public class AllocationHistory : BaseEntity
    {
        [Key]
        public long ID { get; set; }
        public DateTime ProcessStartDateTime { get; set; }
        public DateTime ProcessEndDateTime { get; set; }
        public String Error { get; set; }
        public bool IsSuccess { get; set; }

    }

}
