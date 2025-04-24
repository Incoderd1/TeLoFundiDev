using AgencyPlatform.Application.Authorization.Requirements;
using AgencyPlatform.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Authorization
{
    public class EsDuenoAgenciaHandler : AuthorizationHandler<EsDuenoAgenciaRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAgenciaRepository _agenciaRepository;

        public EsDuenoAgenciaHandler(IHttpContextAccessor httpContextAccessor, IAgenciaRepository agenciaRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _agenciaRepository = agenciaRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EsDuenoAgenciaRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null || !httpContext.User.Identity.IsAuthenticated)
                return;

            var usuarioIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolClaim = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(usuarioIdClaim, out int usuarioId))
                return;

            // Admin tiene acceso total
            if (rolClaim == "admin")
            {
                context.Succeed(requirement);
                return;
            }

            // Obtener agenciaId del route
            var routeValues = httpContext.Request.RouteValues;
            if (!routeValues.TryGetValue("agenciaId", out var agenciaIdObj) || !int.TryParse(agenciaIdObj?.ToString(), out int agenciaId))
                return;

            var agencia = await _agenciaRepository.GetByIdAsync(agenciaId);
            if (agencia != null && agencia.usuario_id == usuarioId)
            {
                context.Succeed(requirement);
            }
        }
    }
}
