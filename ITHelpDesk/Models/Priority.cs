using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class Priority
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string PriorityName { get; set; }
    }
}
