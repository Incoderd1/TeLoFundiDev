using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Application.Services;
using AgencyPlatform.Infrastructure.Services.Email;
using Microsoft.OpenApi.Models;
using AgencyPlatform.Application.Middleware;
using AgencyPlatform.Application.MapperProfiles;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Infrastructure.Mappers;
using AgencyPlatform.Infrastructure.Services.Agencias;
using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using AgencyPlatform.Infrastructure.Services.Acompanantes;
using AgencyPlatform.Application.Interfaces.Services.Categoria;
using AgencyPlatform.Infrastructure.Services.Categoria;
using AgencyPlatform.Application.Interfaces.Repositories.Archivos;
using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.Authorization.Requirements;
using AgencyPlatform.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Infrastructure;
using AgencyPlatform.API.Hubs;
using AgencyPlatform.API.Utils;
using AgencyPlatform.Application.Interfaces.Utils;
using AgencyPlatform.Application.Interfaces.Services.PagoVerificacion;
using AgencyPlatform.Infrastructure.Services.PagoVerificacion;
using AgencyPlatform.Infrastructure.Services;
using AgencyPlatform.Application.Interfaces.Services.Notificaciones;
using AgencyPlatform.Infrastructure.Services.Notificaciones;
using AgencyPlatform.Application.Interfaces.Services.Foto;
using AgencyPlatform.Infrastructure.Services.Foto;
using AgencyPlatform.Infrastructure.Services.Storage;
using AgencyPlatform.Application.Interfaces.Services.FileStorage;
using AgencyPlatform.Infrastructure.Services.FileStorage;
using AgencyPlatform.Application.Interfaces.Services.Cliente;
using AgencyPlatform.Infrastructure.Services.Cliente;
using AgencyPlatform.Application.DTOs.Payments;
using AgencyPlatform.Infrastructure.Services.Payments;
using AgencyPlatform.Application.Configuration;
using Microsoft.Extensions.Options;
using AgencyPlatform.Infrastructure.Services.Puntos;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.Interfaces.Services.ClienteCache;
using AgencyPlatform.Application.Validators;
using AgencyPlatform.Infrastructure.Services.ClienteCache;
using AgencyPlatform.Application.Interfaces.Services.Recommendation;
using AgencyPlatform.Infrastructure.Services.Recommendation;
using AgencyPlatform.Infrastructure.Services.Usuarios;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;


// 📦 Cargar configuración JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");

// 📂 Configurar DbContext PostgreSQL
builder.Services.AddDbContext<AgencyPlatformDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 Configurar Autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Para API REST
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            // Si el encabezado existe pero no comienza con "Bearer "
            // (usuario ha pasado solamente el token sin el prefijo)
            if (!string.IsNullOrEmpty(authHeader) && !authHeader.StartsWith("Bearer "))
            {
                // Establecer directamente el token para la validación
                context.Token = authHeader;
            }

            // Para SignalR - permitir token en query string
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/api/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

// 💉 Inyección de dependencias
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();
// 🚑 Health Checks (estatus de la API)




// ✅ FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserService>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();


//builder.Services.AddAutoMapper(typeof(AgenciasProfile).Assembly);
builder.Services.AddAutoMapper(typeof(AgenciasProfile).Assembly);
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Registrar repositorios
builder.Services.AddScoped<IAgenciaRepository, AgenciaRepository>();
builder.Services.AddScoped<IAcompananteRepository, AcompananteRepository>();
builder.Services.AddScoped<IVerificacionRepository, VerificacionRepository>();
builder.Services.AddScoped<IAnuncioDestacadoRepository, AnuncioDestacadoRepository>();
builder.Services.AddScoped<IIntentoLoginRepository, IntentoLoginRepository>();
builder.Services.AddScoped<ISolicitudAgenciaRepository, SolicitudAgenciaRepository>();
builder.Services.AddScoped<IComisionRepository, ComisionRepository>();

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IAccionesPuntosRepository, AccionesPuntosRepository>();
builder.Services.AddScoped<IMovimientoPuntosRepository, MovimientoPuntosRepository>();

// Registrar servicios
builder.Services.AddScoped<IAcompananteService, AcompananteService>();
builder.Services.AddScoped<IAgenciaService, AgenciaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IArchivosService, ArchivosService>();
builder.Services.AddScoped<IPagoVerificacionService, PagoVerificacionService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<IAgenciaService, AgenciaService>();
builder.Services.AddScoped<IFotoService, FotoService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistroClienteDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();



// En Startup.cs o DependencyInjection.cs
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICuponClienteRepository, CuponClienteRepository>();
builder.Services.AddScoped<IMovimientoPuntosRepository, MovimientoPuntosRepository>();
builder.Services.AddScoped<IAccionesPuntosRepository, AccionesPuntosRepository>();
builder.Services.AddScoped<IContactoRepository, ContactoRepository>();
builder.Services.AddScoped<IVisitaRepository, VisitaRepository>();
builder.Services.AddScoped<ISorteoRepository, SorteoRepository>();
builder.Services.AddScoped<IParticipanteSorteoRepository, ParticipanteSorteoRepository>();
builder.Services.AddScoped<IPaqueteCuponRepository, PaqueteCuponRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IMembresiaVipRepository, MembresiaVipRepository>();
builder.Services.AddScoped<ISuscripcionVipRepository, SuscripcionVipRepository>();




builder.Services.AddScoped<IServerFileStorageService, ServerFileStorageService>(); // lamacenar las fotos en servidor




// Repositorios adicionales
builder.Services.AddScoped<IFotoRepository, FotoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IVisitaRepository, VisitaRepository>();
builder.Services.AddScoped<IContactoRepository, ContactoRepository>();
builder.Services.AddScoped<IVisitaPerfilRepository, VisitaPerfilRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ISolicitudRegistroAgenciaRepository, SolicitudRegistroAgenciaRepository>();
// En Startup.cs o el lugar donde configuras tus servicios
builder.Services.AddScoped<IPagoVerificacionRepository, PagoVerificacionRepository>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

builder.Services.AddScoped<IStripeEventHandlerService, StripeEventHandlerService>();

builder.Services.Configure<ClienteSettings>(
    builder.Configuration.GetSection("ClienteSettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ClienteSettings>>().Value);

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IPuntosService, PuntosService>();


builder.Services.AddScoped<IClienteCacheService, ClienteCacheService>();
builder.Services.AddScoped<AgencyPlatform.Application.Validators.IValidator<RegistroClienteDto>, RegistroClienteDtoValidator>();

//builder.Services.AddHostedService<ExpiryHostedService>();






// Notificaciones
builder.Services.AddScoped<INotificadorRealTime, NotificadorSignalR>();

// DTO para registro de contactos
builder.Services.AddScoped<RegistrarContactoDto>();

// Agregar HttpContextAccessor para acceder al usuario actual
builder.Services.AddHttpContextAccessor();


// Configurar SignalR con opciones
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

// 🌐 CORS (permitir frontend local)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5500"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AgenciaOwnerOnly", policy =>
        policy.Requirements.Add(new EsDuenoAgenciaRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, EsDuenoAgenciaHandler>();

// 🧪 Swagger con JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AgencyPlatform API", Version = "v1" });

    // Usar nombre completo del tipo como SchemaId para resolver conflictos
    options.CustomSchemaIds(type => type.FullName);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Pega solo el token (sin escribir 'Bearer '). El sistema lo agregará automáticamente 🔐",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 🚑 Health Checks (estatus de la API)
builder.Services.AddHealthChecks();


// 📦 Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapPost("/api/payments/webhook", async (HttpRequest req, IStripeEventHandlerService handler) =>
{
    var json = await new StreamReader(req.Body).ReadToEndAsync();
    try
    {
        await handler.HandleAsync(json, req.Headers["Stripe-Signature"]);
        return Results.Ok();
    }
    catch
    {
        return Results.BadRequest();
    }
});



// 🧰 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseStaticFiles();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health");
app.MapControllers();

// 🔔 SignalR Hub
app.MapHub<NotificacionesHub>("/api/Hubs/notificaciones");

app.Run();
