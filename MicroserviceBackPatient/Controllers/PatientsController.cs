using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroserviceBackPatient.Models;
using MicroserviceBackPatient.Data;

namespace MicroserviceBackPatient.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PatientsController : ControllerBase
{
  private readonly PatientDbContext _context;

  public PatientsController(PatientDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public IEnumerable<Patient> Get()
  {
    return _context.Patients.ToList();
  }

  [HttpGet("{id}")]
  public ActionResult<Patient> GetById(int id)
  {
    var patient = _context.Patients.FirstOrDefault(p => p.Id == id);

    if (patient == null)
      return NotFound();

    return patient;
  }

  [HttpPost]
  public ActionResult<Patient> Create(Patient patient)
  {
    if (!ValidatePatient(patient))
      return ValidationProblem(ModelState);

    _context.Patients.Add(patient);
    _context.SaveChanges();

    return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
  }

  [HttpPut("{id}")]
  public IActionResult Update(int id, Patient updatedPatient)
  {
    if (updatedPatient.Id != 0 && updatedPatient.Id != id)
      ModelState.AddModelError(nameof(updatedPatient.Id), "Patient id does not match route id.");

    if (!ValidatePatient(updatedPatient))
      return ValidationProblem(ModelState);

    var patient = _context.Patients.FirstOrDefault(p => p.Id == id);

    if (patient == null)
      return NotFound();

    patient.FirstName = updatedPatient.FirstName;
    patient.LastName = updatedPatient.LastName;
    patient.DateOfBirth = updatedPatient.DateOfBirth;
    patient.Gender = updatedPatient.Gender;
    patient.Address = updatedPatient.Address;
    patient.Phone = updatedPatient.Phone;

    _context.SaveChanges();

    return NoContent();
  }

  [HttpDelete("{id}")]
  public IActionResult Delete(int id)
  {
    var patient = _context.Patients.FirstOrDefault(p => p.Id == id);

    if (patient == null)
      return NotFound();

    _context.Patients.Remove(patient);
    _context.SaveChanges();

    return NoContent();
  }

  private bool ValidatePatient(Patient patient)
  {
    if (patient.DateOfBirth.Date > DateTime.Today)
      ModelState.AddModelError(nameof(patient.DateOfBirth), "Date of birth cannot be in the future.");

    return ModelState.IsValid;
  }
}