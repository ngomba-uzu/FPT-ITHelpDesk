using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class UserPorts
    {
        public int Id { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Port")]
      
        public int PortId { get; set; }

        [ValidateNever]
        public ApplicationUser User { get; set; }

        [ValidateNever]
        public Port Port { get; set; }
    }
}
