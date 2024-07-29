using System.ComponentModel.DataAnnotations;

namespace MVC_sample_app.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public byte[] Picture { get; set; }

        public IFormFile PictureFile { get; set; }
    }
}
