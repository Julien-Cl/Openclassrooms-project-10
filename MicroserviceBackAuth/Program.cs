using MicroserviceBackAuth.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceBackAuth
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddControllers();

      builder.Services.AddDbContext<AuthDbContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

      builder.Services.AddIdentity<IdentityUser, IdentityRole>()
          .AddEntityFrameworkStores<AuthDbContext>()
          .AddDefaultTokenProviders();

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
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var adminEmail = app.Configuration["AdminUser:Email"];
        var adminPassword = app.Configuration["AdminUser:Password"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
          var existingAdmin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();

          if (existingAdmin == null)
          {
            var adminUser = new IdentityUser
            {
              UserName = adminEmail,
              Email = adminEmail
            };

            var result = userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();

            if (!result.Succeeded)
            {
              var errors = string.Join("; ", result.Errors.Select(e => e.Description));
              throw new InvalidOperationException($"Admin user seed failed: {errors}");
            }
          }
        }
      }

      app.Run();
    }
  }
}