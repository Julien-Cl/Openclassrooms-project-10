namespace MicroserviceBackNote.Data;

public class NoteDatabaseSettings
{
  public string ConnectionString { get; set; } = string.Empty;

  public string DatabaseName { get; set; } = string.Empty;

  public string CollectionName { get; set; } = string.Empty;
}