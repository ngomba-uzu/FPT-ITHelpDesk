using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class Port
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PortName { get; set; }
        public List<Ticket> Tickets { get; set; }
        public List<TechnicianPort> TechnicianPorts { get; set; } = new List<TechnicianPort>();

    }
}
