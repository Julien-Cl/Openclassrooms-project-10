using MicroserviceFront.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

public class DetailsModel : PageModel
{
  private readonly HttpClient _http;

  public Patient Patient { get; set; } = new();

  public List<Note> Notes { get; set; } = new();

  [BindProperty]
  public string NewNoteContent { get; set; } = string.Empty;

  public DetailsModel(IHttpClientFactory factory)
  {
    _http = factory.CreateClient();
  }

  public async Task<IActionResult> OnGet(int id)
  {
    var loaded = await LoadPatientAndNotes(id);

    if (!loaded)
      return NotFound();

    return Page();
  }

  public async Task<IActionResult> OnPostAddNote(int id)
  {
    if (string.IsNullOrWhiteSpace(NewNoteContent))
    {
      ModelState.AddModelError(string.Empty, "La note ne peut pas être vide.");

      var loaded = await LoadPatientAndNotes(id);

      if (!loaded)
        return NotFound();

      return Page();
    }

    var patient = await _http.GetFromJsonAsync<Patient>($"http://patient-router:8080/patients/{id}");

    if (patient == null)
      return NotFound();

    var note = new Note
    {
      PatientId = patient.Id,
      PatientName = patient.FirstName,
      Content = NewNoteContent
    };

    var response = await _http.PostAsJsonAsync("http://patient-router:8080/notes", note);

    if (!response.IsSuccessStatusCode)
    {
      ModelState.AddModelError(string.Empty, "Erreur lors de l'ajout de la note.");

      var loaded = await LoadPatientAndNotes(id);

      if (!loaded)
        return NotFound();

      return Page();
    }

    return RedirectToPage("/Details", new { id });
  }

  private async Task<bool> LoadPatientAndNotes(int id)
  {
    var patient = await _http.GetFromJsonAsync<Patient>($"http://patient-router:8080/patients/{id}");

    if (patient == null)
      return false;

    Patient = patient;

    Notes = await _http.GetFromJsonAsync<List<Note>>($"http://patient-router:8080/notes/patient/{id}") ?? new();

    return true;
  }
}