using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class Subcategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Subcategory name is required")]
        public string SubcategoryName { get; set; }

        public int CategoryId { get; set; }
        public int TechnicianGroupId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public TechnicianGroup TechnicianGroup { get; set; }

    }
}
