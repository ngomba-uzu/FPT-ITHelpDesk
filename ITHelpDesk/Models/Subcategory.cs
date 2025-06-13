using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class Subcategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subcategory name is required")]
        [Display(Name = "Subcategory Name")]
        public string SubcategoryName { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }

        [ValidateNever]
        [Display(Name = "Technician Groups")]
        public ICollection<TechnicianGroup> TechnicianGroups { get; set; } = new List<TechnicianGroup>();
    }

}
