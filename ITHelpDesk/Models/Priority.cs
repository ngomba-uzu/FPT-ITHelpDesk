using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Models
{
    public class Priority
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Priority Name")]
        public string PriorityName { get; set; }
    }
}
