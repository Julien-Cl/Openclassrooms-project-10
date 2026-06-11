using System.Net.Http.Headers;
using System.Net.Http.Json;
using MicroserviceFront.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : PageModel
{
  private readonly HttpClient _http;

  public Patient Patient { get; set; } = new();

  public List<Note> Notes { get; set; } = new();
  public Assessment? Assessment { get; set; }



  [BindProperty]
  public string NewNoteContent { get; set; } = string.Empty;

  public DetailsModel(IHttpClientFactory factory)
  {
    _http = factory.CreateClient();
  }

  public async Task<IActionResult> OnGet(int id)
  {
    if (!SetAuthorizationHeader())
      return RedirectToPage("/Login");

    var loaded = await LoadPatientAndNotes(id);

    if (!loaded)
      return NotFound();

    return Page();
  }

  public async Task<IActionResult> OnPostAddNote(int id)
  {
    if (!SetAuthorizationHeader())
      return RedirectToPage("/Login");

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

  private bool SetAuthorizationHeader()
  {
    var token = HttpContext.Session.GetString("AccessToken");

    if (string.IsNullOrWhiteSpace(token))
      return false;

    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    return true;
  }

  private async Task<bool> LoadPatientAndNotes(int id)
  {
    var patient = await _http.GetFromJsonAsync<Patient>($"http://patient-router:8080/patients/{id}");

    if (patient == null)
      return false;

    Patient = patient;

    Notes = await _http.GetFromJsonAsync<List<Note>>($"http://patient-router:8080/notes/patient/{id}") ?? new();

    Assessment = await _http.GetFromJsonAsync<Assessment>($"http://patient-router:8080/assessments/patient/{id}");

    return true;
  }
}