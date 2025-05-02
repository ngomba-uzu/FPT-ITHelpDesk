using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models.ViewModels
{
    public class ResolutionViewModel
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Resolution is required.")]
        public string Resolution { get; set; }

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required.")]
        public int StatusId { get; set; }
        public string? SeniorTechnicianResponse { get; set; }


        public IFormFile? Document { get; set; }
        [ValidateNever]
        public List<Status> StatusList { get; set; } = new List<Status>();
    }
}
