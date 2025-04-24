using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AgencyPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncFailedLoginAttemptsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Vacío, porque la tabla ya existe
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Vacío, o puedes agregar lógica para revertir si es necesario
        }
    }
}
