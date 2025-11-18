namespace Basics.Models
{
    public class Employe
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Fullname => $"{Firstname}{Lastname.ToUpper()}";
        public int Age { get; set; }


    }
}