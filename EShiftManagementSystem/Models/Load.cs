using System;
using System.ComponentModel.DataAnnotations;

namespace EShiftManagementSystem.Models
{
    //Load Model
    public class Load
    {
        public int LoadId { get; set; }

        [Required]
        public int JobId { get; set; }

        public int? TransportUnitId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public decimal Weight { get; set; }

        [Required]
        public decimal Volume { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; }

        public virtual Job Job { get; set; }
        public virtual TransportUnit TransportUnit { get; set; }
    }
}