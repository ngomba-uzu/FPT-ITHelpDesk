using Microsoft.AspNetCore.Mvc;
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

        public ApplicationUser User { get; set; }
        public Port Port { get; set; }
    }
}
