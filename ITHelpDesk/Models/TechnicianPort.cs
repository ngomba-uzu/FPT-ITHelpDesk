using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class TechnicianPort
    {
        public int Id { get; set; }

        [Display(Name = "Technician Name")]

        public int TechnicianId { get; set; }
        [ValidateNever]
        public Technician Technician { get; set; }

        [Display(Name = "Port Name")]

        public int PortId { get; set; }
        [ValidateNever]
        public Port Port { get; set; }
    }
}
