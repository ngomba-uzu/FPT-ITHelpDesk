using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class TechnicianGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        [ValidateNever]
        public List<Technician> Technicians { get; set; } = new List<Technician>();

        [NotMapped]
        [ValidateNever]
        public List<int>? SelectedTechnicianIds { get; set; }
    }
}