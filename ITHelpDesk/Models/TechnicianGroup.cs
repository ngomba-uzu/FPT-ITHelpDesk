using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class TechnicianGroup
    {
        public int Id { get; set; }
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }

        [ValidateNever]
        public ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}