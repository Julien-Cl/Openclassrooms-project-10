using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroserviceFront.Models;
using System.Net.Http.Json;

public class EditModel : PageModel
{
  private readonly HttpClient _http;

  [BindProperty]
  public Patient Patient { get; set; } = new();

  public EditModel(IHttpClientFactory factory)
  {
    _http = factory.CreateClient();
  }

  public async Task<IActionResult> OnGet(int id)
  {
    if (!SetAuthorizationHeader())
      return RedirectToPage("/Login");

    var patient = await _http.GetFromJsonAsync<Patient>($"http://patient-router:8080/patients/{id}");

    if (patient == null)
      return NotFound();

    Patient = patient;

    return Page();
  }

  public async Task<IActionResult> OnPost(int id)
  {
    if (!SetAuthorizationHeader())
      return RedirectToPage("/Login");

    var response = await _http.PutAsJsonAsync($"http://patient-router:8080/patients/{id}", Patient);

    if (!response.IsSuccessStatusCode)
    {
      ModelState.AddModelError(string.Empty, "Erreur lors de la modification du patient.");
      return Page();
    }

    return RedirectToPage("/Index");
  }

  private bool SetAuthorizationHeader()
  {
    var token = HttpContext.Session.GetString("AccessToken");

    if (string.IsNullOrWhiteSpace(token))
      return false;

    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    return true;
  }
}