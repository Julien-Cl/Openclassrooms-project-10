namespace MicroserviceFront.Models;

public class Assessment
{
  public int PatientId { get; set; }

  public string AssessmentResult { get; set; } = string.Empty;
}