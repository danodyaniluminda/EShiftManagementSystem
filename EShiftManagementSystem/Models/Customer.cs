using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EShiftManagementSystem.Models
{
    //Customer Model
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public DateTime RegistrationDate { get; set; }

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
