using System.Net.Http.Headers;
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

  public IActionResult OnGet()
  {
    if (!SetAuthorizationHeader())
      return RedirectToPage("/Login");

    return Page();
  }

  public async Task<IActionResult> OnPost()
  {
    if (!SetAuthorizationHeader())
      return RedirectToPage("/Login");

    var response = await _http.PostAsJsonAsync("http://patient-router:8080/patients", Patient);

    if (!response.IsSuccessStatusCode)
    {
      ModelState.AddModelError(string.Empty, "Erreur lors de la création du patient.");
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