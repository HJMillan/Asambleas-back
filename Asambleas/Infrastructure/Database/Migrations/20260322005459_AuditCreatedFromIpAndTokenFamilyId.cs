using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asambleas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AuditCreatedFromIpAndTokenFamilyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ventanas_horarias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "ventanas_horarias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ventanas_horarias",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ventanas_horarias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ventanas_horarias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "vacantes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "vacantes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "vacantes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "vacantes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "vacantes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveLockouts",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyId",
                table: "refresh_tokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "refresh_tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "refresh_tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "postulantes_listado",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "postulantes_listado",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "postulantes_listado",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "postulantes_listado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "postulantes_listado",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "postulaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "postulaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "postulaciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "postulaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "postulaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "listados_oficiales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "listados_oficiales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "listados_oficiales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "listados_oficiales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "listados_oficiales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "impugnaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "impugnaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "impugnaciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "impugnaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "impugnaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "establecimientos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "establecimientos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "establecimientos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "establecimientos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "establecimientos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "empleos_actuales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "empleos_actuales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "empleos_actuales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "empleos_actuales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "empleos_actuales",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "docente_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "docente_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "docente_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "docente_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "docente_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "designaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "designaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "designaciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "designaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "designaciones",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "declaraciones_juradas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "declaraciones_juradas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "declaraciones_juradas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "declaraciones_juradas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "declaraciones_juradas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "audit_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "audit_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "audit_log",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "audit_log",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "audit_log",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "asambleas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedFromIp",
                table: "asambleas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "asambleas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "asambleas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "asambleas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ventanas_horarias");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "ventanas_horarias");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ventanas_horarias");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ventanas_horarias");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ventanas_horarias");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "vacantes");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "vacantes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "vacantes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "vacantes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "vacantes");

            migrationBuilder.DropColumn(
                name: "ConsecutiveLockouts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "postulantes_listado");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "postulantes_listado");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "postulantes_listado");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "postulantes_listado");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "postulantes_listado");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "postulaciones");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "postulaciones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "postulaciones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "postulaciones");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "postulaciones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "listados_oficiales");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "listados_oficiales");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "listados_oficiales");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "listados_oficiales");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "listados_oficiales");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "impugnaciones");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "impugnaciones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "impugnaciones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "impugnaciones");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "impugnaciones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "establecimientos");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "establecimientos");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "establecimientos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "establecimientos");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "establecimientos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "empleos_actuales");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "empleos_actuales");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "empleos_actuales");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "empleos_actuales");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "empleos_actuales");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "docente_profiles");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "docente_profiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "docente_profiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "docente_profiles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "docente_profiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "designaciones");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "designaciones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "designaciones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "designaciones");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "designaciones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "declaraciones_juradas");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "declaraciones_juradas");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "declaraciones_juradas");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "declaraciones_juradas");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "declaraciones_juradas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "asambleas");

            migrationBuilder.DropColumn(
                name: "CreatedFromIp",
                table: "asambleas");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "asambleas");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "asambleas");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "asambleas");
        }
    }
}
