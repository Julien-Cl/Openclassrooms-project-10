using System.Net.Http.Headers;
using System.Net.Http.Json;
using MicroserviceFront.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
  private readonly HttpClient _http;

  public List<Patient> Patients { get; set; } = new();

  public IndexModel(IHttpClientFactory factory)
  {
    _http = factory.CreateClient();
  }

  public async Task<IActionResult> OnGet()
  {
    var token = HttpContext.Session.GetString("AccessToken");

    if (string.IsNullOrWhiteSpace(token))
      return RedirectToPage("/Login");

    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    Patients = await _http.GetFromJsonAsync<List<Patient>>("http://patient-router:8080/patients") ?? new();

    return Page();
  }

  public async Task<IActionResult> OnPostDelete(int id)
  {
    var token = HttpContext.Session.GetString("AccessToken");

    if (string.IsNullOrWhiteSpace(token))
      return RedirectToPage("/Login");

    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await _http.DeleteAsync($"http://patient-router:8080/patients/{id}");

    if (!response.IsSuccessStatusCode)
    {
      ModelState.AddModelError(string.Empty, "Erreur lors de la suppression du patient.");
      Patients = await _http.GetFromJsonAsync<List<Patient>>("http://patient-router:8080/patients") ?? new();
      return Page();
    }

    return RedirectToPage("/Index");
  }
}