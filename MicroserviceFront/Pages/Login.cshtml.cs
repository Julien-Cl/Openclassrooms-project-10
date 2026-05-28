using MicroserviceFront.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

public class LoginModel : PageModel
{
  private readonly HttpClient _http;

  [BindProperty]
  public string Email { get; set; } = string.Empty;

  [BindProperty]
  public string Password { get; set; } = string.Empty;

  public LoginModel(IHttpClientFactory factory)
  {
    _http = factory.CreateClient();
  }

  public void OnGet()
  {
  }

  public async Task<IActionResult> OnPost()
  {
    var response = await _http.PostAsJsonAsync("http://patient-router:8080/Auth/login", new
    {
      email = Email,
      password = Password
    });

    if (!response.IsSuccessStatusCode)
    {
      ModelState.AddModelError(string.Empty, "Identifiants invalides.");
      return Page();
    }

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

    if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.Token))
    {
      ModelState.AddModelError(string.Empty, "Token JWT absent.");
      return Page();
    }

    HttpContext.Session.SetString("AccessToken", loginResponse.Token);

    return RedirectToPage("/Index");
  }
}