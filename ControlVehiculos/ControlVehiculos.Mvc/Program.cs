

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // =========================================================================
        // REGISTRO DEL CLIENTE HTTP PARA CONSUMIR LA API REST
        // =========================================================================
        var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7200/api/";

        builder.Services.AddHttpClient("VehiculosApi", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // =========================================================================
        // HABILITAR EL SERVICIO DE SESIONES EN MEMORIA (Nativo de .NET Core)
        // =========================================================================
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // La sesión expira tras 30 minutos de inactividad
            options.Cookie.HttpOnly = true;   // Protege la cookie contra ataques de scripts externos
            options.Cookie.IsEssential = true; ;
        });



        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // ACTIVAR MIDDLEWARE DE SESIÓN (Debe ir exactamente aquí)
        app.UseSession();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}