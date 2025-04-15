using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

    }
}
