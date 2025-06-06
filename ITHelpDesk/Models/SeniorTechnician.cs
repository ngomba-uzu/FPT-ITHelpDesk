using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class SeniorTechnician
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // Foreign Key for TechnicianGroup
        public int? TechnicianGroupId { get; set; }
        [ValidateNever]
        public virtual TechnicianGroup TechnicianGroup { get; set; }

        // Foreign Key for Port
        public int? PortId { get; set; }
        [ValidateNever]
        public virtual Port Port { get; set; }
    }
}
