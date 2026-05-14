using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroserviceBackNote.Models;

public class Note
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string? Id { get; set; }

  public int PatientId { get; set; }

  public string PatientName { get; set; } = string.Empty;

  public string Content { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}