namespace MicroserviceFront.Models;

public class Note
{
  public string? Id { get; set; }

  public int PatientId { get; set; }

  public string PatientName { get; set; } = string.Empty;

  public string Content { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }
}