using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Shared.Utils
{
    public static class CsvExporter
    {
        public static byte[] ExportToCsv<T>(List<T> data)
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();

            // Encabezados
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Filas
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item, null);
                    return Escape(value?.ToString() ?? "");
                });
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string Escape(string value)
        {
            if (value.Contains(",") || value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }
    }
}
