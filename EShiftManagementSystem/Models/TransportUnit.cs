using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EShiftManagementSystem.Models
{
    public class TransportUnit
    {
        public int TransportUnitId { get; set; }

        [Required]
        [StringLength(100)]
        public string UnitType { get; set; }

        [Required]
        [StringLength(50)]
        public string LicensePlate { get; set; }

        [Required]
        public decimal MaxWeight { get; set; }

        [Required]
        public decimal MaxVolume { get; set; }

        [Required]
        [StringLength(100)]
        public string DriverName { get; set; }

        [Required]
        [StringLength(100)]
        public string AssistantName { get; set; }

        [StringLength(20)]
        public string DriverPhone { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Load> Loads { get; set; } = new List<Load>();
    }
}
