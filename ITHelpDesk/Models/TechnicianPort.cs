using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class TechnicianPort
    {
        public int Id { get; set; }

        public int TechnicianId { get; set; }
        [ValidateNever]
        public Technician Technician { get; set; }

        public int PortId { get; set; }
        [ValidateNever]
        public Port Port { get; set; }
    }
}
