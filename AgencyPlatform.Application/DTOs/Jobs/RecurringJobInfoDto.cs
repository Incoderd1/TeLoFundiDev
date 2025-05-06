using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.DTOs.Jobs
{
    public class RecurringJobInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string NextExecution { get; set; } = string.Empty;
        public string LastExecution { get; set; } = string.Empty;
        public string LastStatus { get; set; } = string.Empty;
    }
}
