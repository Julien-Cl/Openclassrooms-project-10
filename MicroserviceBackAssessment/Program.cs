using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MicroserviceBackAssessment
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddControllers();

      builder.Services.AddHttpClient("PatientApi", client =>
      {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:PatientApi"] ?? "http://patient-backend:8080/");
      });

      builder.Services.AddHttpClient("NoteApi", client =>
      {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:NoteApi"] ?? "http://note-backend:8080/");
      });

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

      app.Run();
    }
  }
}