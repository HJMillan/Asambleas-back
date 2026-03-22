using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asambleas.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asambleas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HorarioInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HorarioFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VentanaInscripcionInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VentanaInscripcionFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lugar = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asambleas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Entidad = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "establecimientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Localidad = table.Column<string>(type: "text", nullable: false),
                    CodigoFuncional = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_establecimientos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "listados_oficiales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listados_oficiales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Cuil = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsDomelecVerified = table.Column<bool>(type: "boolean", nullable: false),
                    LoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LdapUsername = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ventanas_horarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    AsambleaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventanas_horarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ventanas_horarias_asambleas_AsambleaId",
                        column: x => x.AsambleaId,
                        principalTable: "asambleas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vacantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cargo = table.Column<string>(type: "text", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TipoCargo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Modulos = table.Column<int>(type: "integer", nullable: false),
                    Horas = table.Column<int>(type: "integer", nullable: false),
                    Turno = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    EstablecimientoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsambleaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vacantes_asambleas_AsambleaId",
                        column: x => x.AsambleaId,
                        principalTable: "asambleas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vacantes_establecimientos_EstablecimientoId",
                        column: x => x.EstablecimientoId,
                        principalTable: "establecimientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "declaraciones_juradas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasoActual = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Completada = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Domicilio = table.Column<string>(type: "text", nullable: false),
                    Localidad = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    TrabajaEnOtraJurisdiccion = table.Column<bool>(type: "boolean", nullable: false),
                    JurisdiccionActual = table.Column<string>(type: "text", nullable: true),
                    HorasTotalesOcupadas = table.Column<int>(type: "integer", nullable: false),
                    DocenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaraciones_juradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_declaraciones_juradas_users_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "docente_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Puntaje = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    NivelEducativo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EstadoRevista = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Embarazo = table.Column<bool>(type: "boolean", nullable: false),
                    DeclaracionJuradaPresentada = table.Column<bool>(type: "boolean", nullable: false),
                    CantidadRenuncias = table.Column<int>(type: "integer", nullable: false),
                    Inhabilitado = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoInhabilitacion = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docente_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_docente_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "postulantes_listado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Puntaje = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Posicion = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListadoOficialId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postulantes_listado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_postulantes_listado_listados_oficiales_ListadoOficialId",
                        column: x => x.ListadoOficialId,
                        principalTable: "listados_oficiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_postulantes_listado_users_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "designaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Instancia = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaRechazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "text", nullable: true),
                    CertificadoUrl = table.Column<string>(type: "text", nullable: true),
                    DocenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsambleaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_designaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_designaciones_asambleas_AsambleaId",
                        column: x => x.AsambleaId,
                        principalTable: "asambleas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_designaciones_users_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_designaciones_vacantes_VacanteId",
                        column: x => x.VacanteId,
                        principalTable: "vacantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "postulaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Posicion = table.Column<int>(type: "integer", nullable: false),
                    Puntaje = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    DocenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    VacanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postulaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_postulaciones_users_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_postulaciones_vacantes_VacanteId",
                        column: x => x.VacanteId,
                        principalTable: "vacantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleos_actuales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Establecimiento = table.Column<string>(type: "text", nullable: false),
                    Cargo = table.Column<string>(type: "text", nullable: false),
                    Horas = table.Column<int>(type: "integer", nullable: false),
                    SituacionRevista = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Jurisdiccion = table.Column<string>(type: "text", nullable: true),
                    HorarioLunes = table.Column<string>(type: "text", nullable: true),
                    HorarioMartes = table.Column<string>(type: "text", nullable: true),
                    HorarioMiercoles = table.Column<string>(type: "text", nullable: true),
                    HorarioJueves = table.Column<string>(type: "text", nullable: true),
                    HorarioViernes = table.Column<string>(type: "text", nullable: true),
                    HorarioSabado = table.Column<string>(type: "text", nullable: true),
                    DeclaracionJuradaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleos_actuales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_empleos_actuales_declaraciones_juradas_DeclaracionJuradaId",
                        column: x => x.DeclaracionJuradaId,
                        principalTable: "declaraciones_juradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "impugnaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<string>(type: "text", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Resuelta = table.Column<bool>(type: "boolean", nullable: false),
                    Resolucion = table.Column<string>(type: "text", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DesignacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReclamanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_impugnaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_impugnaciones_designaciones_DesignacionId",
                        column: x => x.DesignacionId,
                        principalTable: "designaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_impugnaciones_users_ReclamanteId",
                        column: x => x.ReclamanteId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Entidad",
                table: "audit_log",
                column: "Entidad");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_Timestamp",
                table: "audit_log",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_UserId",
                table: "audit_log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_declaraciones_juradas_DocenteId",
                table: "declaraciones_juradas",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_designaciones_AsambleaId",
                table: "designaciones",
                column: "AsambleaId");

            migrationBuilder.CreateIndex(
                name: "IX_designaciones_DocenteId",
                table: "designaciones",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_designaciones_VacanteId",
                table: "designaciones",
                column: "VacanteId");

            migrationBuilder.CreateIndex(
                name: "IX_docente_profiles_UserId",
                table: "docente_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_empleos_actuales_DeclaracionJuradaId",
                table: "empleos_actuales",
                column: "DeclaracionJuradaId");

            migrationBuilder.CreateIndex(
                name: "IX_establecimientos_CodigoFuncional",
                table: "establecimientos",
                column: "CodigoFuncional",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_impugnaciones_DesignacionId",
                table: "impugnaciones",
                column: "DesignacionId");

            migrationBuilder.CreateIndex(
                name: "IX_impugnaciones_ReclamanteId",
                table: "impugnaciones",
                column: "ReclamanteId");

            migrationBuilder.CreateIndex(
                name: "IX_postulaciones_DocenteId_VacanteId",
                table: "postulaciones",
                columns: new[] { "DocenteId", "VacanteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_postulaciones_VacanteId",
                table: "postulaciones",
                column: "VacanteId");

            migrationBuilder.CreateIndex(
                name: "IX_postulantes_listado_DocenteId",
                table: "postulantes_listado",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_postulantes_listado_ListadoOficialId",
                table: "postulantes_listado",
                column: "ListadoOficialId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Cuil",
                table: "users",
                column: "Cuil",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Dni",
                table: "users",
                column: "Dni");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vacantes_AsambleaId",
                table: "vacantes",
                column: "AsambleaId");

            migrationBuilder.CreateIndex(
                name: "IX_vacantes_EstablecimientoId",
                table: "vacantes",
                column: "EstablecimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_ventanas_horarias_AsambleaId",
                table: "ventanas_horarias",
                column: "AsambleaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "docente_profiles");

            migrationBuilder.DropTable(
                name: "empleos_actuales");

            migrationBuilder.DropTable(
                name: "impugnaciones");

            migrationBuilder.DropTable(
                name: "postulaciones");

            migrationBuilder.DropTable(
                name: "postulantes_listado");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "ventanas_horarias");

            migrationBuilder.DropTable(
                name: "declaraciones_juradas");

            migrationBuilder.DropTable(
                name: "designaciones");

            migrationBuilder.DropTable(
                name: "listados_oficiales");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vacantes");

            migrationBuilder.DropTable(
                name: "asambleas");

            migrationBuilder.DropTable(
                name: "establecimientos");
        }
    }
}
