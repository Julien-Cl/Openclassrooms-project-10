using System.Text;
using MicroserviceBackNote.Data;
using MicroserviceBackNote.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace MicroserviceBackNote
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.Configure<NoteDatabaseSettings>(
          builder.Configuration.GetSection("NoteDatabase"));

      builder.Services.AddControllers();

      var jwtKey = builder.Configuration["Jwt:Key"];
      var jwtIssuer = builder.Configuration["Jwt:Issuer"];
      var jwtAudience = builder.Configuration["Jwt:Audience"];

      if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("Jwt:Key is missing.");

      builder.Services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtIssuer,
          ValidAudience = jwtAudience,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
      });

      builder.Services.AddAuthorization();

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      app.UseSwagger();
      app.UseSwaggerUI();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllers();

      using (var scope = app.Services.CreateScope())
      {
        var settings = scope.ServiceProvider
            .GetRequiredService<IOptions<NoteDatabaseSettings>>()
            .Value;

        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        var notes = database.GetCollection<Note>(settings.CollectionName);

        if (!notes.Find(_ => true).Any())
        {
          notes.InsertMany(new List<Note>
          {
            new Note
            {
              PatientId = 1,
              PatientName = "TestNone",
              Content = "Le patient déclare qu'il 'se sent très bien' Poids égal ou inférieur au poids recommandé",
              CreatedAt = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 2,
              PatientName = "TestBorderline",
              Content = "Le patient déclare qu'il ressent beaucoup de stress au travail Il se plaint également que son audition est anormale dernièrement",
              CreatedAt = new DateTime(2026, 1, 1, 8, 10, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 2,
              PatientName = "TestBorderline",
              Content = "Le patient déclare avoir fait une réaction aux médicaments au cours des 3 derniers mois Il remarque également que son audition continue d'être anormale",
              CreatedAt = new DateTime(2026, 1, 1, 8, 20, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 3,
              PatientName = "TestInDanger",
              Content = "Le patient déclare qu'il fume depuis peu",
              CreatedAt = new DateTime(2026, 1, 1, 8, 30, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 3,
              PatientName = "TestInDanger",
              Content = "Le patient déclare qu'il est fumeur et qu'il a cessé de fumer l'année dernière Il se plaint également de crises d’apnée respiratoire anormales Tests de laboratoire indiquant un taux de cholestérol LDL élevé",
              CreatedAt = new DateTime(2026, 1, 1, 8, 40, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 4,
              PatientName = "TestEarlyOnset",
              Content = "Le patient déclare qu'il lui est devenu difficile de monter les escaliers Il se plaint également d’être essoufflé Tests de laboratoire indiquant que les anticorps sont élevés Réaction aux médicaments",
              CreatedAt = new DateTime(2026, 1, 1, 8, 50, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 4,
              PatientName = "TestEarlyOnset",
              Content = "Le patient déclare qu'il a mal au dos lorsqu'il reste assis pendant longtemps",
              CreatedAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 4,
              PatientName = "TestEarlyOnset",
              Content = "Le patient déclare avoir commencé à fumer depuis peu Hémoglobine A1C supérieure au niveau recommandé",
              CreatedAt = new DateTime(2026, 1, 1, 9, 10, 0, DateTimeKind.Utc)
            },
            new Note
            {
              PatientId = 4,
              PatientName = "TestEarlyOnset",
              Content = "Taille, Poids, Cholestérol, Vertige et Réaction",
              CreatedAt = new DateTime(2026, 1, 1, 9, 20, 0, DateTimeKind.Utc)
            }
          });
        }
      }

      app.Run();
    }
  }
}