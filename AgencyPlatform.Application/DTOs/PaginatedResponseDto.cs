﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs
{
    public class PaginatedResponseDto<T>
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}
