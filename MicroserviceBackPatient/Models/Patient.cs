using System.ComponentModel.DataAnnotations;

namespace MicroserviceBackPatient.Models;

public class Patient
{
  public int Id { get; set; }

  [Required]
  [StringLength(100)]
  public string FirstName { get; set; } = string.Empty;

  [Required]
  [StringLength(100)]
  public string LastName { get; set; } = string.Empty;

  public DateTime DateOfBirth { get; set; }

  [Required]
  [RegularExpression("^(M|F)$", ErrorMessage = "Gender must be M or F.")]
  public string Gender { get; set; } = string.Empty;

  [StringLength(255)]
  public string? Address { get; set; }

  [StringLength(30)]
  public string? Phone { get; set; }
}