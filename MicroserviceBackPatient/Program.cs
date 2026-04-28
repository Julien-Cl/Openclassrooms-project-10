using MicroserviceBackPatient.Data;
using MicroserviceBackPatient.Models;
using Microsoft.EntityFrameworkCore;



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


      // Swagger
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();


      // Swagger
      app.UseSwagger();
      app.UseSwaggerUI();

      app.UseHttpsRedirection();
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