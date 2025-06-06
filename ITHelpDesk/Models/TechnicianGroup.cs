using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITHelpDesk.Models
{
    public class TechnicianGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; }

    }
}