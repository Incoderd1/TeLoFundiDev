using AgencyPlatform.Application.Interfaces;
using AgencyPlatform.Application.Interfaces.Repositories;
using AgencyPlatform.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using AgencyPlatform.Application.Interfaces.Services; // Añadido para BCrypt
using AgencyPlatform.Core.Enums;
using AgencyPlatform.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using AgencyPlatform.Application.Interfaces.Services.Acompanantes;
using AgencyPlatform.Infrastructure.Services.Acompanantes;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Infrastructure.Repositories;

namespace AgencyPlatform.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        private readonly IIntentoLoginRepository _intentoLoginRepository;
        private readonly IAcompananteService _acompananteService;
        private readonly IAgenciaRepository _agenciaRepository;


        // Lista de contraseñas comunes para prevenir su uso
        private static readonly HashSet<string> CommonPasswords = new HashSet<string> {
            "password", "123456", "12345678", "qwerty", "admin", "welcome",
            "password123", "abc123", "letmein", "monkey", "1234567890"
        };

        public UserService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserService> logger,
            IIntentoLoginRepository intentoLoginRepository,IAcompananteService acompananteService, IAgenciaRepository agenciaRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _intentoLoginRepository = intentoLoginRepository;
            _acompananteService = acompananteService;
            _agenciaRepository = agenciaRepository;
        }
        public async Task<usuario> RegisterUserAsync(string email, string password, string tipoUsuario, string? phone = null)
        {
            try
            {
                _logger.LogInformation("Iniciando registro de usuario con email: {Email}, tipo: {TipoUsuario}", email, tipoUsuario);

                // Validación de parámetros
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("El email es obligatorio.");
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("La contraseña es obligatoria.");
                if (string.IsNullOrWhiteSpace(tipoUsuario))
                    throw new ArgumentException("El tipo de usuario es obligatorio.");

                // Validar formato de email
                if (!IsValidEmail(email))
                    throw new ArgumentException("El formato del email no es válido.");

                // Validar que el dominio del email sea válido
                if (!IsValidEmailDomain(email))
                    throw new ArgumentException("El dominio del email no es válido.");

                // Validar complejidad de contraseña
                if (!IsPasswordStrong(password))
                    throw new ArgumentException("La contraseña debe tener al menos 8 caracteres, incluyendo una letra mayúscula, una minúscula, un número y un carácter especial.");

                // Verificar si es una contraseña común
                if (IsCommonPassword(password))
                    throw new ArgumentException("La contraseña es demasiado común. Por favor, elija una contraseña más segura.");

                tipoUsuario = tipoUsuario.Trim().ToLower();

                // Verificar si el usuario actual es administrador
                bool isCurrentUserAdmin = false;
                try
                {
                    var currentUser = _httpContextAccessor.HttpContext?.User;
                    isCurrentUserAdmin = currentUser?.Identity?.IsAuthenticated == true && currentUser.IsInRole("admin");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al verificar rol de administrador durante registro");
                    isCurrentUserAdmin = false;
                }

                // Solo permitir ciertas combinaciones
                if (tipoUsuario == "admin" && !isCurrentUserAdmin)
                {
                    _logger.LogWarning("Intento no autorizado de crear un usuario administrador desde IP: {IP}",
                        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress);
                    throw new UnauthorizedAccessException("No tienes permisos para crear usuarios administradores.");
                }
                // Si no está autenticado o no es admin, solo puede registrar clientes, agencias o acompañantes
                if (!isCurrentUserAdmin && tipoUsuario != "cliente" && tipoUsuario != "agencia" && tipoUsuario != "acompanante")
                {
                    throw new ArgumentException("Solo puedes registrarte como 'cliente', 'agencia' o 'acompañante'.");
                }

                // Verificar si el usuario ya existe
                var existingUser = await _userRepository.GetByEmailAsync(email);
                if (existingUser != null)
                    throw new Exception("El correo ya está registrado.");

                // Obtener el ID del rol
                var roleId = await _userRepository.GetRoleIdByNameAsync(tipoUsuario);
                if (roleId == 0)
                    throw new Exception("No se encontró el rol en la base de datos.");

                // Crear el nuevo usuario
                var user = new usuario
                {
                    email = email,
                    password_hash = BCrypt.Net.BCrypt.HashPassword(password),
                    rol_id = roleId,
                    telefono = phone,
                    esta_activo = true,
                    provider = "local",
                    password_required = true,
                    fecha_registro = DateTime.UtcNow,
                    created_at = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // Enviar correo de bienvenida
                await EnviarCorreoBienvenida(user, tipoUsuario);

                _logger.LogInformation("Usuario registrado exitosamente con ID: {UserId}, email: {Email}, tipo: {TipoUsuario}",
                    user.id, email, tipoUsuario);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de usuario con email: {Email}, tipo: {TipoUsuario}",
                    email, tipoUsuario);
                throw;
            }
        }

        public async Task<(string AccessToken, string RefreshToken)> LoginUserAsync(string email, string password, string ipAddress, string userAgent)
        {
            try
            {
                _logger.LogInformation("Intento de inicio de sesión para usuario: {Email}", email);

                // Validar parámetros
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("El email es obligatorio.");
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("La contraseña es obligatoria.");

                // Verificar intentos fallidos
                await VerificarIntentosDeLogin(email, ipAddress);

                // Buscar usuario
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Intento de inicio de sesión fallido: usuario no encontrado para {Email} desde IP {IP}",
                        email, ipAddress);
                    await RegistrarIntentoFallido(email, ipAddress);
                    throw new Exception("Credenciales inválidas.");
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(password, user.password_hash))
                {
                    _logger.LogWarning("Intento de inicio de sesión fallido: contraseña incorrecta para {Email} desde IP {IP}",
                        email, ipAddress);
                    await RegistrarIntentoFallido(email, ipAddress);
                    throw new Exception("Credenciales inválidas.");
                }

                // Verificar si la cuenta está activa
                if (user.esta_activo != true)
                {
                    _logger.LogWarning("Intento de inicio de sesión en cuenta desactivada: {Email}", email);
                    throw new Exception("Esta cuenta está desactivada. Contacte al administrador.");
                }

                // Obtener información del rol para los claims
                string roleName = "unknown";
                try
                {
                    roleName = _userRepository.GetRoleNameById(user.rol_id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo obtener el nombre del rol para el usuario {Email}", email);
                    roleName = user.rol_id.ToString();
                }

                // Generar tokens
                var accessToken = GenerateJwtToken(user, roleName);
                var refreshToken = await GenerateRefreshTokenAsync(user, ipAddress, userAgent);

                // Restablecer intentos fallidos
                await _userRepository.ResetFailedLoginAttemptsAsync(email, ipAddress);

                _logger.LogInformation("Inicio de sesión exitoso para usuario: {Email}, ID: {UserId}", email, user.id);

                // Actualizar último acceso
                user.ultimo_acceso = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return (accessToken, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inicio de sesión para usuario: {Email}", email);
                throw;
            }
        }

        public async Task<string> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent)
        {
            try
            {
                _logger.LogInformation("Solicitud de refresh token");

                if (string.IsNullOrWhiteSpace(refreshToken))
                    throw new ArgumentException("El refresh token es obligatorio.");

                var token = await _userRepository.GetRefreshTokenAsync(refreshToken);
                if (token == null || (token.esta_revocado ?? false) || token.fecha_expiracion < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token inválido o expirado desde IP {IP}", ipAddress);
                    throw new Exception("Refresh token inválido o expirado.");
                }

                // Validar que el token pertenezca al mismo dispositivo/IP
                if (token.ip_address != ipAddress)
                {
                    _logger.LogWarning("Intento de usar refresh token desde IP diferente. Token original: {OriginalIP}, intento desde: {CurrentIP}",
                        token.ip_address, ipAddress);
                    throw new Exception("El refresh token no es válido para esta dirección IP.");
                }

                var user = await _userRepository.GetByIdAsync(token.usuario_id);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado para refresh token");
                    throw new Exception("Usuario no encontrado.");
                }

                // Verificar si la cuenta está activa
                if (user.esta_activo != true)
                {
                    _logger.LogWarning("Intento de refresh token en cuenta desactivada: usuario ID {UserId}", user.id);
                    throw new Exception("Esta cuenta está desactivada.");
                }

                // Obtener información del rol para los claims
                string roleName = "unknown";
                try
                {
                    roleName = _userRepository.GetRoleNameById(user.rol_id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo obtener el nombre del rol para el usuario {Email}", user.email);
                    roleName = user.rol_id.ToString();
                }

                _logger.LogInformation("Refresh token exitoso para usuario: {Email}, ID: {UserId}", user.email, user.id);

                //revisar aqui
                // Registrar el uso de refresh token
                token.fecha_expiracion = DateTime.UtcNow;
                await _userRepository.UpdateRefreshTokenAsync(token);
                await _userRepository.SaveChangesAsync();

                return GenerateJwtToken(user, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en refresh token");
                throw;
            }
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            try
            {
                _logger.LogInformation("Solicitud de restablecimiento de contraseña para: {Email}", email);

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("El email es obligatorio.");

                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Solicitud de restablecimiento para email no registrado: {Email}", email);
                    throw new Exception("Usuario no encontrado.");
                }

                // Revocar tokens anteriores no utilizados
                await _userRepository.RevokeAllResetTokensForUserAsync(user.id);

                var resetToken = new tokens_reset_password
                {
                    usuario_id = user.id,
                    token = GenerateRandomToken(),
                    fecha_expiracion = DateTime.UtcNow.AddHours(1),
                    esta_usado = false,
                    created_at = DateTime.UtcNow
                };

                await _userRepository.AddResetTokenAsync(resetToken);
                await _userRepository.SaveChangesAsync();

                // Enviar email con el token
                await EnviarCorreoRestablecimientoContrasena(user, resetToken.token);

                _logger.LogInformation("Solicitud de restablecimiento enviada para: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en solicitud de restablecimiento para: {Email}", email);
                throw;
            }
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                _logger.LogInformation("Intento de restablecimiento de contraseña con token");

                if (string.IsNullOrWhiteSpace(token))
                    throw new ArgumentException("El token es obligatorio.");
                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("La nueva contraseña es obligatoria.");

                // Validar complejidad de contraseña
                if (!IsPasswordStrong(newPassword))
                    throw new ArgumentException("La contraseña debe tener al menos 8 caracteres, incluyendo una letra mayúscula, una minúscula, un número y un carácter especial.");

                // Verificar si es una contraseña común
                if (IsCommonPassword(newPassword))
                    throw new ArgumentException("La contraseña es demasiado común. Por favor, elija una contraseña más segura.");

                var resetToken = await _userRepository.GetResetTokenAsync(token);
                if (resetToken == null)
                {
                    _logger.LogWarning("Token de restablecimiento no encontrado: {Token}", token);
                    throw new Exception("Token no encontrado.");
                }

                if (resetToken.esta_usado)
                {
                    _logger.LogWarning("Intento de usar token de restablecimiento ya utilizado");
                    throw new Exception("Este token ya ha sido utilizado.");
                }

                if (resetToken.fecha_expiracion < DateTime.UtcNow)
                {
                    _logger.LogWarning("Intento de usar token de restablecimiento expirado");
                    throw new Exception("El token ha expirado.");
                }

                var user = await _userRepository.GetByIdAsync(resetToken.usuario_id);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado para token de restablecimiento");
                    throw new Exception("Usuario no encontrado.");
                }

                // Verificar que la nueva contraseña sea diferente a la actual
                if (BCrypt.Net.BCrypt.Verify(newPassword, user.password_hash))
                    throw new ArgumentException("La nueva contraseña debe ser diferente a la actual.");

                //revisar aqui
                // Actualizar contraseña
                user.password_hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                resetToken.esta_usado = true;
                resetToken.fecha_expiracion = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.UpdateResetTokenAsync(resetToken);

                // Revocar todos los refresh tokens existentes
                await _userRepository.RevokeAllRefreshTokensForUserAsync(user.id);

                await _userRepository.SaveChangesAsync();

                // Enviar notificación de cambio de contraseña
                await EnviarCorreoConfirmacionCambioContrasena(user);

                _logger.LogInformation("Restablecimiento de contraseña exitoso para usuario: {Email}, ID: {UserId}",
                    user.email, user.id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en restablecimiento de contraseña");
                throw;
            }
        }

        public async Task<usuario> GetUserByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario por ID: {UserId}", id);

                if (id <= 0)
                    throw new ArgumentException("El ID de usuario debe ser un número positivo.");

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {UserId}", id);
                    throw new Exception("Usuario no encontrado.");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {UserId}", id);
                throw;
            }
        }

        public async Task<List<usuario>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios");
                // En un escenario real, deberías implementar paginación aquí
                return await _userRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw;
            }
        }

        // Método con paginación (no parte de la interfaz original, pero recomendado)
        public async Task<(List<usuario> Usuarios, int Total)> GetAllUsersPagedAsync(int pagina, int elementosPorPagina)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios paginados: página {Pagina}, tamaño {Tamano}",
                    pagina, elementosPorPagina);

                if (pagina < 1)
                    throw new ArgumentException("El número de página debe ser mayor o igual a 1.");
                if (elementosPorPagina < 1)
                    throw new ArgumentException("El número de elementos por página debe ser mayor o igual a 1.");

                return await _userRepository.GetAllPagedAsync(pagina, elementosPorPagina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios paginados: página {Pagina}, tamaño {Tamano}",
                    pagina, elementosPorPagina);
                throw;
            }
        }

        public async Task UpdateUserAsync(int id, string email, string? phone, bool estaActivo)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario ID: {UserId}", id);

                if (id <= 0)
                    throw new ArgumentException("El ID de usuario debe ser un número positivo.");
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("El email es obligatorio.");
                if (!IsValidEmail(email))
                    throw new ArgumentException("El formato del email no es válido.");

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Intento de actualizar usuario no encontrado con ID: {UserId}", id);
                    throw new Exception("Usuario no encontrado.");
                }

                // Verificar permisos
                VerificarPermisosEdicionUsuario(user);

                // Verificar si el email ya está en uso por otro usuario
                if (email != user.email)
                {
                    var existingUser = await _userRepository.GetByEmailAsync(email);
                    if (existingUser != null && existingUser.id != id)
                    {
                        _logger.LogWarning("Email ya registrado en otro usuario: {Email}", email);
                        throw new Exception("El correo ya está registrado por otro usuario.");
                    }
                }

                // Guardar valores antiguos para logging
                var oldEmail = user.email;
                var oldPhone = user.telefono;
                var oldActive = user.esta_activo;

                // Actualizar valores
                user.email = email;
                user.telefono = phone;
                user.esta_activo = estaActivo;
                user.updated_at = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Usuario actualizado ID: {UserId}, Email: {OldEmail} -> {NewEmail}, " +
                    "Activo: {OldActive} -> {NewActive}",
                    id, oldEmail, email, oldActive, estaActivo);

                // Si cambió el email, enviar notificación
                if (oldEmail != email)
                {
                    await EnviarCorreoCambioEmail(user, oldEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario ID: {UserId}", id);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando usuario ID: {UserId}", id);

                if (id <= 0)
                    throw new ArgumentException("El ID de usuario debe ser un número positivo.");

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Intento de eliminar usuario no encontrado con ID: {UserId}", id);
                    throw new Exception("Usuario no encontrado.");
                }

                // Verificar permisos
                VerificarPermisosEliminacionUsuario(user);

                // Revocar todos los tokens
                await _userRepository.RevokeAllRefreshTokensForUserAsync(user.id);
                await _userRepository.RevokeAllResetTokensForUserAsync(user.id);

                await _userRepository.DeleteAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Usuario eliminado ID: {UserId}, Email: {Email}", id, user.email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario ID: {UserId}", id);
                throw;
            }
        }
        public async Task<(usuario Usuario, int AcompananteId)> RegisterUserAcompananteAsync(
                 string email,
                 string password,
                 string telefono, // Ya es igual a WhatsApp
                 string nombrePerfil,
                 string genero,
                 int edad,
                 string? descripcion = null,
                 string? ciudad = null,
                 string? pais = null,
                 string? disponibilidad = "Horario flexible",
                 decimal? tarifaBase = null,
                 string? moneda = "USD", // Forzado a "USD"
                 List<int>? categoriaIds = null,
                 string? whatsapp = null, // Ya no es necesario, lo usamos como teléfono
                 string? emailContacto = null,
                 int altura = 160, // Default
                 int peso = 60, // Default
                 string idiomas = "Español" // Default
                            )
        {
            try
            {
                _logger.LogInformation("Iniciando registro de usuario-acompañante con email: {Email}", email);

                // 1. Crear el usuario con rol "acompanante"
                var user = await RegisterUserAsync(email, password, "acompanante", telefono);

                // 2. Construir el DTO de acompañante
                var acompananteDto = new CrearAcompananteDto
                {
                    NombrePerfil = nombrePerfil,
                    Genero = genero,
                    Edad = edad,
                    Descripcion = descripcion,
                    Ciudad = ciudad,
                    Pais = pais,
                    Disponibilidad = disponibilidad ?? "Horario flexible", // Default
                    TarifaBase = tarifaBase ?? 0,
                    Moneda = "USD", // Forzado a USD
                    CategoriaIds = categoriaIds ?? new List<int>(), // Si no se pasa, asigna lista vacía
                    Telefono = telefono,
                    WhatsApp = telefono, // Usamos teléfono como WhatsApp también
                    EmailContacto = emailContacto ?? email, // Si no se pasa, usa el email
                    Altura = altura,
                    Peso = peso,
                    Idiomas = idiomas
                };

                // 3. Crear el perfil del acompañante
                int acompananteId = await _acompananteService.CrearAsync(acompananteDto, user.id);

                _logger.LogInformation("✅ Registro exitoso. Usuario ID: {UserId}, Acompañante ID: {AcompananteId}",
                    user.id, acompananteId);

                return (user, acompananteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en registro acompañante con email: {Email}", email);
                throw;
            }
        }

        //venir aqui

        public async Task NotificarAdminDeSolicitudAgenciaAsync()
        {
            // Lógica para enviar notificación al administrador (por ejemplo, por correo)
            var admins = await _userRepository.GetUsersByRoleAsync("admin"); // Supongamos que tenemos un repositorio para obtener usuarios por rol

            foreach (var admin in admins)
            {
                await _emailSender.SendEmailAsync(admin.email, "Nueva solicitud de agencia", "Hay una nueva solicitud pendiente de agencia.");
            }
        }





        #region Métodos Privados

        private string GenerateJwtToken(usuario user, string roleName)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                    new Claim(ClaimTypes.Email, user.email),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("rol_id", user.rol_id.ToString()),
                    new Claim("email_verified", (!string.IsNullOrEmpty(user.email)).ToString().ToLower()),
                    new Claim("created_at", DateTime.UtcNow.ToString("o"))
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<string> GenerateRefreshTokenAsync(usuario user, string ipAddress, string userAgent)
        {
            var refreshToken = new refresh_token
            {
                usuario_id = user.id,
                token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ip_address = ipAddress,
                user_agent = userAgent,
                device_info = GetDeviceInfo(userAgent),
                fecha_expiracion = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"])),
                esta_revocado = false,
                created_at = DateTime.UtcNow
            };

            await _userRepository.AddRefreshTokenAsync(refreshToken);
            await _userRepository.SaveChangesAsync();
            return refreshToken.token;
        }

        private string GenerateRandomToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        private async Task EnviarCorreoBienvenida(usuario user, string tipoUsuario)
        {
            try
            {
                var roleLabel = char.ToUpper(tipoUsuario[0]) + tipoUsuario.Substring(1);
                var sitioWeb = _configuration["SiteSettings:WebUrl"] ?? "https://tu-aplicacion.com";

                var bodyHtml = $@"
                <h2>¡Bienvenido/a a Agency Platform!</h2>
                <p>Hola <strong>{user.email}</strong>, tu cuenta ha sido creada exitosamente como <strong>{roleLabel}</strong>.</p>
                <p>Ya puedes iniciar sesión y comenzar a explorar:</p>
                <p><a href='{sitioWeb}' style='background:#007bff;padding:10px 15px;color:white;text-decoration:none;border-radius:4px;'>Ir a Agency Platform</a></p>
                <br/>
                <p style='font-size:12px;color:gray;'>Este es un mensaje automático, por favor no respondas.</p>";

                await _emailSender.SendEmailAsync(user.email, "Bienvenido a Agency Platform", bodyHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de bienvenida a: {Email}", user.email);
                // No re-lanzamos la excepción para no interrumpir el registro
            }
        }

        private async Task EnviarCorreoRestablecimientoContrasena(usuario user, string token)
        {
            try
            {
                var resetUrl = $"{_configuration["SiteSettings:WebUrl"] ?? "https://tu-aplicacion.com"}/reset-password?token={token}";

                var bodyHtml = $@"
                <h2>Solicitud de recuperación de contraseña</h2>
                <p>Hola <strong>{user.email}</strong>, has solicitado restablecer tu contraseña.</p>
                <p>Haz clic en el siguiente enlace para continuar:</p>
                <p><a href='{resetUrl}' style='background:#007bff;padding:10px 15px;color:white;text-decoration:none;border-radius:4px;'>Restablecer contraseña</a></p>
                <p>Este enlace estará activo por 1 hora.</p>
                <br/>
                <p style='font-size:12px;color:gray;'>Si no solicitaste este cambio, ignora este mensaje o contacta a soporte.</p>";

                await _emailSender.SendEmailAsync(user.email, "Restablecimiento de contraseña", bodyHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de restablecimiento a: {Email}", user.email);
                // No re-lanzamos la excepción para no interrumpir el proceso
            }
        }

        private async Task EnviarCorreoConfirmacionCambioContrasena(usuario user)
        {
            try
            {
                var loginUrl = $"{_configuration["SiteSettings:WebUrl"] ?? "https://tu-aplicacion.com"}/login";

                var bodyHtml = $@"
                <h2>Contraseña actualizada correctamente</h2>
                <p>Hola <strong>{user.email}</strong>, tu contraseña ha sido actualizada con éxito.</p>
                <p>Ya puedes iniciar sesión con tu nueva contraseña:</p>
                <p><a href='{loginUrl}' style='background:#007bff;padding:10px 15px;color:white;text-decoration:none;border-radius:4px;'>Iniciar sesión</a></p>
                <br/>
                <p style='font-size:12px;color:gray;'>Si no realizaste este cambio, por favor contacta inmediatamente a soporte.</p>";

                await _emailSender.SendEmailAsync(user.email, "Contraseña actualizada con éxito", bodyHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de confirmación de cambio de contraseña a: {Email}", user.email);
                // No re-lanzamos la excepción para no interrumpir el proceso
            }
        }
        private async Task EnviarCorreoCambioEmail(usuario user, string oldEmail)
        {
            try
            {
                var sitioWeb = _configuration["SiteSettings:WebUrl"] ?? "https://tu-aplicacion.com";

                // Enviar a la dirección antigua
                var bodyHtmlOld = $@"
                <h2>Tu dirección de correo ha sido cambiada</h2>
                <p>Hola,</p>
                <p>Tu dirección de correo electrónico ha sido cambiada de <strong>{oldEmail}</strong> a <strong>{user.email}</strong>.</p>
                <p>Si no realizaste este cambio, por favor contacta inmediatamente a soporte.</p>
                <p><a href='{sitioWeb}/contact' style='background:#007bff;padding:10px 15px;color:white;text-decoration:none;border-radius:4px;'>Contactar Soporte</a></p>
                <br/>
                <p style='font-size:12px;color:gray;'>Este es un mensaje automático, por favor no respondas.</p>";

                await _emailSender.SendEmailAsync(oldEmail, "Tu dirección de correo ha sido cambiada", bodyHtmlOld);

                // Enviar a la nueva dirección
                var bodyHtmlNew = $@"
                <h2>Confirmación de cambio de dirección de correo</h2>
                <p>Hola <strong>{user.email}</strong>,</p>
                <p>Tu dirección de correo electrónico ha sido cambiada de <strong>{oldEmail}</strong> a <strong>{user.email}</strong>.</p>
                <p>Si realizaste este cambio, no necesitas hacer nada más.</p>
                <p>Si no realizasteReintentarClaude alcanzó la longitud máxima para un mensaje y ha pausado su respuesta. Puede escribir Continuar para seguir con la conversación.CcontinuarEditarContinuando con el método para enviar correo de cambio de email:
csharp                <p>Si no realizaste este cambio, por favor contacta inmediatamente a soporte.</p>
                <p><a href='{sitioWeb}/contact' style='background:#007bff;padding:10px 15px;color:white;text-decoration:none;border-radius:4px;'>Contactar Soporte</a></p>
                <br/>
                <p style='font-size:12px;color:gray;'>Este es un mensaje automático, por favor no respondas.</p>";

                await _emailSender.SendEmailAsync(user.email, "Confirmación de cambio de dirección de correo", bodyHtmlNew);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de notificación de cambio de email: {OldEmail} -> {NewEmail}",
                    oldEmail, user.email);
                // No re-lanzamos la excepción para no interrumpir el proceso
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidEmailDomain(string email)
        {
            try
            {
                // Obtener dominio del email
                var parts = email.Split('@');
                if (parts.Length != 2)
                    return false;

                var domain = parts[1];

                // Lista de dominios no permitidos (ejemplo)
                var invalidDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "tempmail.com", "throwaway.com", "mailinator.com", "fakeinbox.com", "yopmail.com"
                };

                // Verificar si está en la lista negra
                if (invalidDomains.Contains(domain))
                    return false;

                // En una implementación real, aquí se podrían hacer comprobaciones adicionales como:
                // 1. Verificar registros MX
                // 2. Consultar una API de validación de email

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsPasswordStrong(string password)
        {
            // Mínimo 8 caracteres, al menos 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
            return regex.IsMatch(password);
        }

        private bool IsCommonPassword(string password)
        {
            // Comprobar en la lista de contraseñas comunes
            return CommonPasswords.Contains(password.ToLower());
        }

        private string GetDeviceInfo(string userAgent)
        {
            // Extraer información básica del user agent
            // En una implementación real, podría usar una biblioteca como UAParser
            return userAgent.Length <= 255 ? userAgent : userAgent.Substring(0, 255);
        }

        private async Task VerificarIntentosDeLogin(string email, string ipAddress)
        {
            try
            {
                var intentos = await _userRepository.GetFailedLoginAttemptsAsync(email, ipAddress);
                var maxIntentos = int.Parse(_configuration["Security:MaxLoginAttempts"] ?? "5");
                var tiempoBloqueo = int.Parse(_configuration["Security:LockoutMinutes"] ?? "15");

                if (intentos >= maxIntentos)
                {
                    var ultimoIntento = await _userRepository.GetLastFailedLoginAttemptTimeAsync(email, ipAddress);
                    var tiempoTranscurrido = DateTime.UtcNow - ultimoIntento;

                    if (tiempoTranscurrido.TotalMinutes < tiempoBloqueo)
                    {
                        var minutosRestantes = tiempoBloqueo - (int)tiempoTranscurrido.TotalMinutes;
                        _logger.LogWarning("Cuenta bloqueada por intentos fallidos: {Email}, IP: {IP}, tiempo restante: {MinutosRestantes} minutos",
                            email, ipAddress, minutosRestantes);
                        throw new Exception($"Demasiados intentos fallidos. Cuenta bloqueada por {minutosRestantes} minutos.");
                    }
                    else
                    {
                        // Si ha pasado el tiempo de bloqueo, reiniciar contador
                        await _userRepository.ResetFailedLoginAttemptsAsync(email, ipAddress);
                    }
                }
            }
            catch (Exception ex) when (!(ex.Message.Contains("Demasiados intentos fallidos")))
            {
                _logger.LogError(ex, "Error al verificar intentos de login para: {Email}, IP: {IP}", email, ipAddress);
                // No relanzamos excepciones técnicas para no revelar información sensible
            }
        }

        private async Task RegistrarIntentoFallido(string email, string ipAddress)
        {
            try
            {
                await _userRepository.RegisterFailedLoginAttemptAsync(email, ipAddress);
                _logger.LogWarning("Intento fallido de login registrado: {Email}, IP: {IP}", email, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar intento fallido para: {Email}, IP: {IP}", email, ipAddress);
                // No relanzamos la excepción para no interrumpir el flujo normal
            }
        }

        private void VerificarPermisosEdicionUsuario(usuario user)
        {
            // Obtener usuario actual
            int currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Verificar permisos
            if (currentUserId != user.id && currentUserRole != "admin")
            {
                _logger.LogWarning("Intento no autorizado de editar usuario. Editor: {EditorId}, Usuario objetivo: {TargetId}",
                    currentUserId, user.id);
                throw new UnauthorizedAccessException("No tienes permisos para modificar este usuario.");
            }
        }

        private void VerificarPermisosEliminacionUsuario(usuario user)
        {
            // Obtener usuario actual
            int currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Solo administradores pueden eliminar usuarios (excepto a sí mismos)
            if (currentUserRole != "admin")
            {
                _logger.LogWarning("Intento no autorizado de eliminar usuario por un no-admin. Editor: {EditorId}, Usuario objetivo: {TargetId}",
                    currentUserId, user.id);
                throw new UnauthorizedAccessException("No tienes permisos para eliminar usuarios.");
            }

            // Un admin no puede eliminarse a sí mismo
            if (currentUserId == user.id)
            {
                _logger.LogWarning("Intento de admin de eliminarse a sí mismo. UserId: {UserId}", currentUserId);
                throw new UnauthorizedAccessException("No puedes eliminar tu propia cuenta.");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
     

        #endregion
    }

}






