using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroserviceBackNote.Models;

public class Note
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string? Id { get; set; }

  [Range(1, int.MaxValue)]
  public int PatientId { get; set; }

  [Required]
  public string PatientName { get; set; } = string.Empty;

  [Required]
  public string Content { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}