using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentApplication.Models
{
    public class AppointmentSearchModel
    {
        public string? PatientName { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public Range<int>? AgeRange { get; set; }

        public override string ToString()
        {
            return $"Name: {PatientName ?? "not selected"}, Date: {(AppointmentDate?.ToShortDateString() ?? "not selected")}, AgeRange: {(AgeRange?.ToString() ?? "not selected")}";

        }
    }

    public class Range<T>
    {
        public T? Min_Val {  get; set; } 
        public T? Max_Val {  get; set; }

        public override string ToString()
        {
            if (Min_Val == null || Max_Val == null)
            {
                return $"Age range not defined.";
            }
            return $"Min value: {Min_Val}, Max value: {Max_Val}";
        }
    }

}
