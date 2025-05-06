using AgencyPlatform.API.Hubs;
using AgencyPlatform.API.Utils;
using AgencyPlatform.Application.Authorization.Requirements;
using AgencyPlatform.Application.Configuration;
using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.DTOs.Cliente;
using AgencyPlatform.Application.DTOs.Payments;
using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Application.Interfaces.Repositories.Archivos;
using AgencyPlatform.Application.Interfaces.ScheduledTasks;
using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using AgencyPlatform.Application.Interfaces.Services.AdvancedSearch;
using AgencyPlatform.Application.Interfaces.Services.Agencias;
using AgencyPlatform.Application.Interfaces.Services.BackgroundJob;
using AgencyPlatform.Application.Interfaces.Services.Categoria;
using AgencyPlatform.Application.Interfaces.Services.Cliente;
using AgencyPlatform.Application.Interfaces.Services.ClienteCache;
using AgencyPlatform.Application.Interfaces.Services.Cupones;
using AgencyPlatform.Application.Interfaces.Services.EmailAgencia;
using AgencyPlatform.Application.Interfaces.Services.FileStorage;
using AgencyPlatform.Application.Interfaces.Services.Foto;
using AgencyPlatform.Application.Interfaces.Services.Geocalizacion;
using AgencyPlatform.Application.Interfaces.Services.Notificaciones;
using AgencyPlatform.Application.Interfaces.Services.PagoVerificacion;
using AgencyPlatform.Application.Interfaces.Services.Recommendation;
using AgencyPlatform.Application.Interfaces.Utils;
using AgencyPlatform.Application.MapperProfiles;
using AgencyPlatform.Application.Middleware;
using AgencyPlatform.Application.Services;
using AgencyPlatform.Application.Validators;
using AgencyPlatform.Core.Entities;
using AgencyPlatform.Infrastructure.Authorization;
using AgencyPlatform.Infrastructure.Extensions;
using AgencyPlatform.Infrastructure.Mappers;
using AgencyPlatform.Infrastructure.Repositories;
using AgencyPlatform.Infrastructure.Services;
using AgencyPlatform.Infrastructure.Services.Acompanantes;
using AgencyPlatform.Infrastructure.Services.AdvancedSearch;
using AgencyPlatform.Infrastructure.Services.Agencias;
using AgencyPlatform.Infrastructure.Services.BackgroundJobs;
using AgencyPlatform.Infrastructure.Services.Categoria;
using AgencyPlatform.Infrastructure.Services.Cliente;
using AgencyPlatform.Infrastructure.Services.ClienteCache;
using AgencyPlatform.Infrastructure.Services.Cupones;
using AgencyPlatform.Infrastructure.Services.Email;
using AgencyPlatform.Infrastructure.Services.EmailProfecional;
using AgencyPlatform.Infrastructure.Services.FileStorage;
using AgencyPlatform.Infrastructure.Services.Foto;
using AgencyPlatform.Infrastructure.Services.Geocalizacion;
using AgencyPlatform.Infrastructure.Services.Notificaciones;
using AgencyPlatform.Infrastructure.Services.PagoVerificacion;
using AgencyPlatform.Infrastructure.Services.Payments;
using AgencyPlatform.Infrastructure.Services.Puntos;
using AgencyPlatform.Infrastructure.Services.Recommendation;
using AgencyPlatform.Infrastructure.Services.Storage;
using AgencyPlatform.Infrastructure.Services.Usuarios;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;


// 📦 Cargar configuración JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");

// 📂 Configurar DbContext PostgreSQL

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

builder.Services.AddAutoMapper(typeof(SearchMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(PagosMappingProfile).Assembly);




//builder.Services.AddAutoMapper(cfg =>
//{
//    cfg.AddProfile<SearchMappingProfile>();

//});
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(connectionString, new PostgreSqlStorageOptions
    {
        QueuePollInterval = TimeSpan.FromSeconds(15)
    })
    .UseFilter(new AutomaticRetryAttribute
    {
        Attempts = 3,
        DelaysInSeconds = new[] { 60, 300, 600 } // 1min, 5min, 10min
    }));

GlobalJobFilters.Filters.Add(new LogFailureAttribute(builder.Services.BuildServiceProvider().GetRequiredService<ILogger<LogFailureAttribute>>()));



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
builder.Services.AddScoped<IBusquedaRepository, BusquedaRepository>();
builder.Services.AddScoped<IAdvancedSearchService, AdvancedSearchService>();
builder.Services.AddHttpClient("GeoCoding");
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<IPaqueteCuponService, PaqueteCuponService>();
builder.Services.AddScoped<IEmailProfesionalService, EmailProfesionalService>();

builder.Services.AddScoped<ITransaccionRepository, TransaccionRepository>();
builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();




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


builder.Services.AddSingleton<IBackgroundJobService, HangfireBackgroundJobService>();
builder.Services.AddScoped<IScheduledTasksService, ScheduledTasksService>();
builder.Services.AddHangfireServices(builder.Configuration);



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
app.UseHangfireDashboard(builder.Configuration);
app.InitializeRecurringJobs();

app.UseStaticFiles();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();


app.MapHealthChecks("/health");
app.MapControllers();

// 🔔 SignalR Hub
app.MapHub<NotificacionesHub>("/api/Hubs/notificaciones");

app.Run();
