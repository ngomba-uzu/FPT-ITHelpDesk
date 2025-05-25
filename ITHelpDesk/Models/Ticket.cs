using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ITHelpDesk.Areas.Utility;


namespace ITHelpDesk.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ticket Number")]
        [ValidateNever]
        public string TicketNumber { get; set; }

        [Required]
        [Display(Name = "Requester Name")]
        public string RequesterName { get; set; }
        [Required]
        public string Email { get; set; }

        // Foreign keys
        [Required]
        [Display(Name = "Port")]
        public int PortId { get; set; }
        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Required]
        [Display(Name = "Subcategory")]
        public int SubcategoryId { get; set; }
        [Required]
        [Display(Name = "Priority")]
        public int PriorityId { get; set; }
        [Required]
        public string Description { get; set; }
        

        [Display(Name = "File Name")]
         [ValidateNever]
        public string? FileName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? StatusChangedAt { get; set; }

        [NotMapped]
        [ValidateNever]
        public IFormFile? UploadedFile { get; set; }

        [Display(Name = "Status")]
        public int StatusId { get; set; }
        public int? AssignedTechnicianId { get; set; }
        public int TechnicianGroupId { get; set; }
        [ValidateNever]
        public string CreatedBy { get; set; }
        

        [ValidateNever]
        public Status Status { get; set; }

        [ValidateNever]
        public bool? IsResolutionAcknowledged { get; set; } // null = pending, true = yes, false = no

        public DateTime? ClosedDate { get; set; }

        [ValidateNever]
        public string? Resolution { get; set; }

        [ValidateNever]
        public string? ReopenReason { get; set; }

        [ValidateNever]
        public string? ReassignReason { get; set; }

        [Display(Name = "Escalated At")]
        public DateTime? EscalatedDate { get; set; }

        [ValidateNever]
        public string? EscalateReason { get; set; }
        public int? SeniorTechnicianId { get; set; } // who the ticket was escalated to
        public string? SeniorTechnicianResponse { get; set; } // response added by senior tech
        public int? ClosedByTechnicianId { get; set; }
        [ValidateNever]
        public Technician ClosedByTechnician { get; set; }

        [ValidateNever]
        public bool IsAutoEscalated { get; set; } = false;

        // Technician-only fields

        [Display(Name = "Mode of Request")]
        public RequestMode? Mode { get; set; }
        // e.g., "Email", "Call"

        [Display(Name = "Assign To")]
        public int? ManuallyAssignedToId { get; set; } // optional manual assignment

        [Display(Name = "Email to Notify")]
        [EmailAddress]
        public string? EmailToNotify { get; set; }

        [Display(Name = "Organization")]
        public string? Organization { get; set; }

        [ValidateNever]
        public Technician? ManuallyAssignedTo { get; set; } // navigation property

        public SeniorTechnician? SeniorTechnician { get; set; }
        [ValidateNever]
        public Technician AssignedTechnician { get; set; }

        [ValidateNever]
        public TechnicianGroup TechnicianGroup { get; set; }

        [ValidateNever]
        public Port Port { get; set; }
        [ValidateNever]
        public Department Department { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public Subcategory Subcategory { get; set; }
        [ValidateNever]
        public Priority Priority { get; set; }
    }

}

