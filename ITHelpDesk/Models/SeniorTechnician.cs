using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class SeniorTechnician
    {
        public int Id { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        public string Email { get; set; }

        // Foreign Key for TechnicianGroup
        [Display(Name = "Technician Group")]
        public int? TechnicianGroupId { get; set; }
        [ValidateNever]
        public virtual TechnicianGroup TechnicianGroup { get; set; }

        // Foreign Key for Port
        [Display(Name = "Port Name")]
        public int? PortId { get; set; }
        [ValidateNever]
        public virtual Port Port { get; set; }
    }
}
