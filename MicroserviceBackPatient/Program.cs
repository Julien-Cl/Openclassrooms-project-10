using System.Text;
using MicroserviceBackPatient.Data;
using MicroserviceBackPatient.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;




namespace MicroserviceBackPatient
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddControllers();

      builder.Services.AddDbContext<PatientDbContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

      builder.Services.AddIdentity<IdentityUser, IdentityRole>()
          .AddEntityFrameworkStores<PatientDbContext>()
          .AddDefaultTokenProviders();

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
        var context = scope.ServiceProvider.GetRequiredService<PatientDbContext>();
        context.Database.Migrate();

        if (!context.Patients.Any(p => p.FirstName == "TestNone"))
        {
          context.Patients.AddRange(
              new Patient
              {
                FirstName = "TestNone",
                LastName = "Test",
                DateOfBirth = new DateTime(1966, 12, 31),
                Gender = "F",
                Address = "1 Brookside St",
                Phone = "100-222-3333"
              },
              new Patient
              {
                FirstName = "TestBorderline",
                LastName = "Test",
                DateOfBirth = new DateTime(1945, 6, 24),
                Gender = "M",
                Address = "2 High St",
                Phone = "200-333-4444"
              },
              new Patient
              {
                FirstName = "TestInDanger",
                LastName = "Test",
                DateOfBirth = new DateTime(2004, 6, 18),
                Gender = "M",
                Address = "3 Club Road",
                Phone = "300-444-5555"
              },
              new Patient
              {
                FirstName = "TestEarlyOnset",
                LastName = "Test",
                DateOfBirth = new DateTime(2002, 6, 28),
                Gender = "F",
                Address = "4 Valley Dr",
                Phone = "400-555-6666"
              }
          );

          context.SaveChanges();
        }
      }

      app.Run();
    }
  }
}