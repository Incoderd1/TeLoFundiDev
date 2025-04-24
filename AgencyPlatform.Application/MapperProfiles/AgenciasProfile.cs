using AgencyPlatform.Application.DTOs.Agencias;
using AgencyPlatform.Core.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.MapperProfiles
{
    public class AgenciasProfile : Profile
    {
        public AgenciasProfile()
        {
            CreateMap<CrearAgenciaDto, agencia>();
            CreateMap<UpdateAgenciaDto, agencia>();
            CreateMap<agencia, AgenciaDto>();
        }
    }
}
