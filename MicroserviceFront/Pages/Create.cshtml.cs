using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroserviceFront.Models;
using System.Net.Http.Json;

public class CreateModel : PageModel
{
  private readonly HttpClient _http;

  [BindProperty]
  public Patient Patient { get; set; } = new()
  {
    DateOfBirth = DateTime.Today
  };

  public CreateModel(IHttpClientFactory factory)
  {
    _http = factory.CreateClient();
  }

  public void OnGet()
  {
  }

  public async Task<IActionResult> OnPost()
  {
    var response = await _http.PostAsJsonAsync("http://patient-router:8080/patients", Patient);

    if (!response.IsSuccessStatusCode)
    {
      ModelState.AddModelError(string.Empty, "Erreur lors de la création du patient.");
      return Page();
    }

    return RedirectToPage("/Index");
  }
}