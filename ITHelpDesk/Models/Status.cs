using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class Status
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status Name")]
        public string StatusName { get; set; } // e.g., Unassigned, Assigned, Closed

    }
}
