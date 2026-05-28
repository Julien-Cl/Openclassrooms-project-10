namespace MicroserviceFront
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddRazorPages();
      builder.Services.AddHttpClient();

      builder.Services.AddDistributedMemoryCache();

      builder.Services.AddSession(options =>
      {
        options.IdleTimeout = TimeSpan.FromHours(2);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
      });

      var app = builder.Build();

      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
      }

      app.UseStaticFiles();

      app.UseRouting();

      app.UseSession();

      app.MapRazorPages();

      app.Run();
    }
  }
}