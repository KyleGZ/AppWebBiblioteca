using AppWebBiblioteca.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// CONFIGURACIÓN DE AUTENTICACIÓN CON COOKIES
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Usuario/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// CONFIGURACIÓN DE AUTORIZACIÓN CON POLICIES
builder.Services.AddAuthorization(options =>
{
    // Policy para Admin (acceso total)
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy para Supervisor (puede supervisar pero no administrar)
    options.AddPolicy("SupervisorOnly", policy =>
        policy.RequireRole("Supervisor"));

    // Policy para Staff (Admin + Supervisor)
    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("Admin", "Supervisor"));

    // Policy para usuarios autenticados (cualquier rol excepto Lector básico)
    options.AddPolicy("AuthenticatedUsers", policy =>
        policy.RequireAuthenticatedUser());

    // Policy para gestión de usuarios (solo Admin y Supervisor)
    options.AddPolicy("PuedeGestionarUsuarios", policy =>
        policy.RequireRole("Admin", "Supervisor"));

    // Policy para contenido privilegiado (no Lectores básicos)
    options.AddPolicy("ContenidoPrivilegiado", policy =>
        policy.RequireRole("Admin", "Supervisor"));
});

// CONFIGURACIÓN DE SESIÓN
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// REGISTRO DE SERVICIOS
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<ILibroService, LibroService>();
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<IGeneroService, GeneroService>();
builder.Services.AddScoped<ISeccionService, SeccionService>();
builder.Services.AddScoped<IEditorialService, EditorialService>();
builder.Services.AddScoped<IEmailService, EmailService>();

/*
 * Servicio para manejo de imágenes
 */
builder.Services.AddScoped<IImageService, ImageService>();


// CONFIGURAR HTTP CLIENT
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseStaticFiles();

var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "portadas");

// CONFIGURE THE HTTP REQUEST PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANTE: ORDEN CORRECTO DE MIDDLEWARES
app.UseSession();
app.UseAuthentication(); // ← Debe ir antes de Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");

app.Run();