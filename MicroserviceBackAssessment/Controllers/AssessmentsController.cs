using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceBackAssessment.Controllers;





[Authorize]
[ApiController]
[Route("[controller]")]
public class AssessmentsController : ControllerBase
{
  private readonly IHttpClientFactory _httpClientFactory;

  public AssessmentsController(IHttpClientFactory httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }


  // API Principale. Prend un PatientId 
  [HttpGet("patient/{patientId:int}")]
  public async Task<ActionResult<AssessmentResponse>> GetByPatientId(int patientId)
  {
    if (patientId <= 0)
      return BadRequest("PatientId must be greater than 0."); // Patient.Id est une clé primaire générée par SQL Server et elles commencent à 1. 

    // Récupération du DTO du patient
    var patient = await GetPatient(patientId); // Ici on utilise await car c'est un appel réseau donc ça doit être asyunc

    if (patient == null)
      return NotFound();

    // Récupération des DTO des notes
    List<NoteDataDto> notes = await GetNotes(patientId);

    // Calculs métiers
    // ---------------
    // Calcul de l'âge
    int age = CalculateAge(patient.DateOfBirth);

    // Calcul du nombre de triggers
    int triggerCount = CountTriggerCategories(notes);

    // Calcul du score de risque final
    string assessmentResult = CalculateAssessment(age, patient.Gender, triggerCount);

    return new AssessmentResponse
    {
      PatientId = patientId,
      AssessmentResult = assessmentResult
    };
  }


  // Récupère les données utiles d'un patient en envoyant une requête HTTP à MicroserviceBackPatient (sans passer par Ocelot). Renvoie un PatientDataDto. 
  private async Task<PatientDataDto?> GetPatient(int patientId)
  {
    // Ici, CreateClient est mal nommé. En fait ici on récupère juste le httpClient configuré pour appeler MicroserviceBackPatient. 
    var client = _httpClientFactory.CreateClient("PatientApi"); 

    // Création d'une requête HTTP GET pour patientId
    using var request = new HttpRequestMessage(HttpMethod.Get, $"patients/{patientId}");

    // Ajout du JWT à la requête
    AddAuthorizationHeader(request);

    // Envoi de la requête à MicroserviceBackPatient
    var response = await client.SendAsync(request);

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
      return null;

    if (!response.IsSuccessStatusCode)
      throw new InvalidOperationException($"Patient API returned {(int)response.StatusCode}.");

    return await response.Content.ReadFromJsonAsync<PatientDataDto>();
  }


  // Récupère les données utiles des notes d'un patient en envoyant une requête HTTP à MicroserviceBackNotes (sans passer par Ocelot). Renvoie un NoteDataDto.
  private async Task<List<NoteDataDto>> GetNotes(int patientId)
  {
    var client = _httpClientFactory.CreateClient("NoteApi");

    using var request = new HttpRequestMessage(HttpMethod.Get, $"notes/patient/{patientId}");
    AddAuthorizationHeader(request);

    var response = await client.SendAsync(request);

    if (!response.IsSuccessStatusCode)
      throw new InvalidOperationException($"Note API returned {(int)response.StatusCode}.");

    return await response.Content.ReadFromJsonAsync<List<NoteDataDto>>() ?? new List<NoteDataDto>();
  }



  private void AddAuthorizationHeader(HttpRequestMessage request)
  {
    var authorization = Request.Headers.Authorization.ToString();

    if (!string.IsNullOrWhiteSpace(authorization))
      request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
  }


  // Calcule combien il y a de triggers dans la liste de notes d'un patient.
  private static int CountTriggerCategories(List<NoteDataDto> notes)
  {
    var foundTriggers = new HashSet<string>(); // HashSet = ensemble de valeurs unique non ordonnées (un set)

    foreach (var note in notes)
    {
      // Normalisation du texte pour tout passer en minuscules et retirer les accents
      var content = NormalizeText(note.Content);

      if (content.Contains("hemoglobine a1c"))
        foundTriggers.Add("HemoglobineA1C");

      if (content.Contains("microalbumine"))
        foundTriggers.Add("Microalbumine");

      if (content.Contains("taille"))
        foundTriggers.Add("Taille");

      if (content.Contains("poids"))
        foundTriggers.Add("Poids");

      if (content.Contains("fumeur") ||
          content.Contains("fumeuse") ||
          content.Contains("fume") ||
          content.Contains("fumer"))
        foundTriggers.Add("Fumeur");

      if (content.Contains("anormal"))
        foundTriggers.Add("Anormal");

      if (content.Contains("cholesterol"))
        foundTriggers.Add("Cholesterol");

      if (content.Contains("vertige"))
        foundTriggers.Add("Vertiges");

      if (content.Contains("rechute"))
        foundTriggers.Add("Rechute");

      if (content.Contains("reaction"))
        foundTriggers.Add("Reaction");

      if (content.Contains("anticorps"))
        foundTriggers.Add("Anticorps");
    }

    return foundTriggers.Count;
  }




  // Normalisation d'une string. Retire les majuscules et les accents.
  private static string NormalizeText(string value)
  {
    var lower = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder();

    foreach (var character in lower)
    {
      var category = CharUnicodeInfo.GetUnicodeCategory(character);

      if (category != UnicodeCategory.NonSpacingMark)
        builder.Append(character);
    }

    return builder.ToString().Normalize(NormalizationForm.FormC);
  }




  private static int CalculateAge(DateTime dateOfBirth)
  {
    var today = DateTime.Today;
    var age = today.Year - dateOfBirth.Year;

    if (dateOfBirth.Date > today.AddYears(-age))
      age--;

    return age;
  }



  /* Calcul du score final. 
  
  Modèle décisionnel: 
    - homme:
	    - moins de 30 ans: 
		    - 3 à 4 déclencheurs: danger
		    - 5+ déclencheurs: early onset
	    - 30+ ans:
		    - 2 à 5 déclencheurs: borderline
		    - 6 à 7 déclencheurs: danger
		    - 8+ déclencheurs: early onset
    - femme: 
	    - moins de 30 ans: 
		    - 4 à 6 déclencheurs: danger
		    - 7+ déclencheurs: early onset
	    - 30+ ans:
		    - 2 à 5 déclencheurs: borderline
		    - 6 à 7 déclencheurs: danger
		    - 8+ déclencheurs: early onset


  
   
   
   */
  private static string CalculateAssessment(int age, string gender, int triggerCount)
  {


    // DEBUG TEST
    //return "InDanger"; 
    // FIN DU DEBUG TEST




    // Hommes
    if (gender == "M")
    {

      // Moins de 30 ans
      if (age < 30)
      {
        if (triggerCount >= 5)
          return "EarlyOnSet";

        if (triggerCount >= 3)
          return "InDanger";

      }

      // 30 ans et plus
      else
      {
        if (triggerCount >= 8)
          return "EarlyOnSet";

        if (triggerCount >= 6)
          return "InDanger";

        if (triggerCount >= 2)
          return "Borderline";
      }


      return "None";
    }


    // Femmes
    if (gender == "F")
    {
      // Moins de 30 ans
      if (age < 30)
      {
        if (triggerCount >= 7)
          return "EarlyOnSet";

        if (triggerCount >= 4)
          return "InDanger";
      }

      // 30 ans et plus
      else
      {
        if (triggerCount >= 8)
          return "EarlyOnSet";

        if (triggerCount >= 6)
          return "InDanger";

        if (triggerCount >= 2)
          return "Borderline";
      }

      return "None";

    }

    return "None";
  }


}




  public class PatientDataDto
{

  public DateTime DateOfBirth { get; set; }

  public string Gender { get; set; } = string.Empty;
}


public class NoteDataDto
{

  public string Content { get; set; } = string.Empty;

}



public class AssessmentResponse
{
  public int PatientId { get; set; }

  public string AssessmentResult { get; set; } = string.Empty;

}