using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class Status
    {
        public int Id { get; set; }

        [Required]
        public string StatusName { get; set; } // e.g., Unassigned, Assigned, Closed

        // Optional: use this later for auto-escalation logic
        public int EscalationThresholdInMinutes { get; set; }
    }
}
