using MicroserviceBackNote.Data;
using MicroserviceBackNote.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroserviceBackNote.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class NotesController : ControllerBase
{
  private readonly IMongoCollection<Note> _notes;

  public NotesController(IOptions<NoteDatabaseSettings> settings)
  {
    var client = new MongoClient(settings.Value.ConnectionString);
    var database = client.GetDatabase(settings.Value.DatabaseName);
    _notes = database.GetCollection<Note>(settings.Value.CollectionName);
  }

  [HttpGet]
  public async Task<ActionResult<List<Note>>> GetAll()
  {
    var notes = await _notes.Find(_ => true)
        .SortBy(n => n.PatientId)
        .ThenBy(n => n.CreatedAt)
        .ToListAsync();

    return notes;
  }

  [HttpGet("patient/{patientId:int}")]
  public async Task<ActionResult<List<Note>>> GetByPatientId(int patientId)
  {
    if (patientId <= 0)
      return BadRequest("PatientId must be greater than 0.");

    var notes = await _notes.Find(n => n.PatientId == patientId)
        .SortBy(n => n.CreatedAt)
        .ToListAsync();

    return notes;
  }

  [HttpPost]
  public async Task<ActionResult<Note>> Create(Note note)
  {
    if (!ValidateNote(note))
      return ValidationProblem(ModelState);

    note.Id = null;
    note.CreatedAt = DateTime.UtcNow;

    await _notes.InsertOneAsync(note);

    return CreatedAtAction(
        nameof(GetByPatientId),
        new { patientId = note.PatientId },
        note);
  }

  private bool ValidateNote(Note note)
  {
    if (note.PatientId <= 0)
      ModelState.AddModelError(nameof(note.PatientId), "PatientId must be greater than 0.");

    if (string.IsNullOrWhiteSpace(note.PatientName))
      ModelState.AddModelError(nameof(note.PatientName), "PatientName is required.");

    if (string.IsNullOrWhiteSpace(note.Content))
      ModelState.AddModelError(nameof(note.Content), "Content is required.");

    return ModelState.IsValid;
  }
}