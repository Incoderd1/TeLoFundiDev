using AgencyPlatform.Application.DTOs;
using AgencyPlatform.Application.DTOs.Acompanantes;
using AgencyPlatform.Application.DTOs.Acompanantes.RegistroAcompananate;
using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Application.DTOs.Foto;
using AgencyPlatform.Application.DTOs.Usuarios;
using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgencyPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender; // Inyectamos IEmailSender

        public UsersController(IUserService userService, IEmailSender emailSender)
        {
            _userService = userService;
            _emailSender = emailSender;
        }

        // ============================================================
        // 🟩 GET: Obtener todos los usuarios (con paginación)
        // ============================================================
        [HttpGet]
        [Authorize(Roles = "admin")] // Solo administradores pueden listar usuarios
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (users, total) = await _userService.GetAllUsersPagedAsync(page, pageSize);
                return Ok(new
                {
                    Data = users,
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

       
        // ============================================================
        // 🟦 GET: Obtener usuario por ID
        // ============================================================
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { Message = "Usuario no encontrado" });

                return Ok(new
                {
                    Id = user.id,
                    Email = user.email,
                    Telefono = user.telefono,
                    EstaActivo = user.esta_activo,
                    RolId = user.rol_id,
                    FechaRegistro = user.fecha_registro,
                    UltimoAcceso = user.ultimo_acceso
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 🟨 PUT: Actualizar usuario
        // ============================================================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { Message = "El email es obligatorio." });

                await _userService.UpdateUserAsync(id, dto.Email, dto.Telefono, dto.EstaActivo ?? true);
                return Ok(new { Message = "Usuario actualizado correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 🟥 DELETE: Eliminar usuario
        // ============================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Solo administradores pueden eliminar usuarios
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Ok(new { Message = "Usuario eliminado correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 🟦 GET: Verificar autenticación (Get2)
        // ============================================================
        [HttpGet("get2")]
        [Authorize]
        public IActionResult Get2()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("🔐 Usuario autenticado ID: " + userId);

            return Ok(new { Status = "Autenticado", UserId = userId });
        }

        // ============================================================
        // 🟦 POST: Registrar usuario
        // ============================================================

        [HttpPost("register")]
        [AllowAnonymous] // Permitir acceso sin autenticación
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.TipoUsuario))
                    return BadRequest(new { Message = "Email, contraseña y tipo de usuario son obligatorios." });

                var user = await _userService.RegisterUserAsync(
                    request.Email,
                    request.Password,
                    request.TipoUsuario

                );

                return CreatedAtAction(nameof(GetById), new { id = user.id }, new
                {
                    UserId = user.id,
                    Email = user.email,
                    TipoUsuario = request.TipoUsuario
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en RegisterUserAsync:");
                Console.WriteLine($"🔹 Mensaje: {ex.Message}");
                Console.WriteLine($"🔹 StackTrace: {ex.StackTrace}");

                return BadRequest(new
                {
                    Message = ex.Message
                    // StackTrace = ex.StackTrace // Comentado para producción
                });
            }
        }

        // ============================================================
        // 🟦 POST: Login
        // ============================================================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new { Message = "Email y contraseña son obligatorios." });

                var (accessToken, refreshToken) = await _userService.LoginUserAsync(
                    request.Email,
                    request.Password,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    Request.Headers["User-Agent"].ToString() ?? "unknown"
                );

                return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 🔁 POST: Refresh Token
        // ============================================================
        [HttpPost("refresh-token")]
        [AllowAnonymous] // Cambiado a AllowAnonymous ya que el token puede estar expirado
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                    return BadRequest(new { Message = "El refresh token es obligatorio." });

                var newAccessToken = await _userService.RefreshTokenAsync(
                    request.RefreshToken,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    Request.Headers["User-Agent"].ToString() ?? "unknown"
                );

                return Ok(new { AccessToken = newAccessToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 🔑 POST: Solicitar token de reseteo de contraseña
        // ============================================================
        [HttpPost("reset-password-request")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { Message = "El email es obligatorio." });

                await _userService.RequestPasswordResetAsync(request.Email);
                return Ok(new { Message = "Solicitud de restablecimiento enviada. Revisa tu correo electrónico." });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // ============================================================
        // 🔑 POST: Confirmar token de reseteo
        // ============================================================
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                    return BadRequest(new { Message = "El token y la nueva contraseña son obligatorios." });

                await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
                return Ok(new { Message = "Contraseña restablecida con éxito. Ahora puedes iniciar sesión con tu nueva contraseña." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

      


       
    }


   


  



}
