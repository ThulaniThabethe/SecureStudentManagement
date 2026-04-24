using System.ComponentModel.DataAnnotations;

namespace SecureStudentManagement.Models
{
    public class Learner
    {
        public string? id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        [Required]
        [RegularExpression("Active|Inactive")]
        public string EnrolmentStatus { get; set; }

        public string? ProfileImageUrl { get; set; }

        public bool IsDeleted { get; set; }
    }
}