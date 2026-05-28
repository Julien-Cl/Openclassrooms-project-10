using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MicroserviceBackPatient.Models;

namespace MicroserviceBackPatient.Data;

public class PatientDbContext : IdentityDbContext
{
  public PatientDbContext(DbContextOptions<PatientDbContext> options)
      : base(options)
  {
  }

  public DbSet<Patient> Patients => Set<Patient>();
}