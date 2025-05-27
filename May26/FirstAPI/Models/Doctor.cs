namespace FirstAPI.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float YearsOfExperience { get; set; }
        public ICollection<DoctorSpeciality>? DoctorSpecialities { get; set; }
         public ICollection<Appointment>? Appointments { get; set; }
        public Doctor()
        {    
        }
        public Doctor(int id, string name, float experience)
        {
            Id = id;
            Name = name;
            YearsOfExperience = experience;
        }
    }
}