using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EShiftManagementSystem.Models
{
    public class Job
    {
        public int JobId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(200)]
        public string StartLocation { get; set; }

        [Required]
        [StringLength(200)]
        public string Destination { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? ScheduleDate { get; set; } 

        public DateTime? CompletionDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        public decimal Cost { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<Load> Loads { get; set; } = new List<Load>();

        public string DisplayName => $"Job {JobId} - {Customer?.FullName ?? "Unknown Customer"}";

    }
}
