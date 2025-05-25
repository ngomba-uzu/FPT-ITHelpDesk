using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [ForeignKey("Department")]
        public int DepartmentId { get; set; }  // Foreign Key to Department
        public Department Department { get; set; }

        [Required]
        [ForeignKey("Port")]
        public int PortId { get; set; }  // Foreign Key to Port
        public Port Port { get; set; }

        [NotMapped]
        public string Role { get; set; } = string.Empty;

        public DateTime CreatedOnDateTime { get; set; }

        [DisplayName("Full Name")]
        public string FullName { get; set; } = string.Empty;

        public byte[] ProfilePicture { get; set; }
        public string ProfilePictureContentType { get; set; }


    }
}
