namespace MicroserviceFront
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddRazorPages();
      builder.Services.AddHttpClient();

      var app = builder.Build();

      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
      }

      app.UseStaticFiles();

      app.UseRouting();

      app.MapRazorPages();

      app.Run();
    }
  }
}