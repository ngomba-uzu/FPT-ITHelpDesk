using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ITHelpDesk.Models
{
    public class Technician
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        [ValidateNever]
        public ApplicationUser? User { get; set; }

        [Required]
        public int TechnicianGroupId { get; set; }

        [ValidateNever]
        public TechnicianGroup? TechnicianGroup { get; set; }

        [ValidateNever]
        public List<TechnicianPort> TechnicianPorts { get; set; } = new List<TechnicianPort>();

        [ValidateNever]
        public List<Ticket> Tickets { get; set; } = new List<Ticket>(); 

    }

}
