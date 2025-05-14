using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace ITHelpDesk.Models.ViewModels

{
    public class EscalateTicketViewModel
    {
        public int TicketId { get; set; }

        [Required]
        public int SeniorTechnicianId { get; set; }

        [Required]
        public string EscalateReason { get; set; }
        public string Status { get; set; }

        [ValidateNever]
        public List<SeniorTechnician> SeniorTechnicians { get; set; }
    }
}
