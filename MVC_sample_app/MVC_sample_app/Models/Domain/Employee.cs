namespace MVC_sample_app.Models.Domain
{
    public class Employee
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public byte[] Picture { get; set; }
    }

}
