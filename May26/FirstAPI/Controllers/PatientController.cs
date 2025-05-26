using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/[controller]")]

public class PatientController : ControllerBase 
{

    static List<Patient> patients = new List<Patient> {
        new Patient{Id=101,Name="Aditi",Reason="Typhoid"},
        new Patient{Id=102,Name="Srujana",Reason="Diabetes"},
        new Patient{Id=103,Name="Aakarsh",Reason="Food Poisoning"}

    };

    [HttpGet]
    public ActionResult<IEnumerable<Patient>> GetPatient() {
        return Ok(patients);
    }

    [HttpPost]
    public ActionResult<Patient> PostPatient([FromBody] Patient patient) {
        patients.Add(patient);
        return Created("",patient);
    }

    [HttpPut("{id}")]
    public ActionResult PutPatient(int id, [FromBody] Patient updatedPatient) {
        var patient = patients.FirstOrDefault(x=> x.Id == id);
        if (patient == null) {
            return NotFound("Patient not found");
        }
        patient.Name = updatedPatient.Name;
        patient.Reason = updatedPatient.Reason;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult DeletePatient(int id) {
        var patient = patients.FirstOrDefault(x=> x.Id == id);
        if (patient == null) {
            return NotFound("Patient not found");
        }
        patients.Remove(patient);
        return NoContent();
    }


}