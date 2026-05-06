using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEE_Calculator.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    evidence_ref = table.Column<string>(type: "text", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "activity_units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    dimension = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_units", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    key_prefix = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    key_hash = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_external_user_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calculation_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calculation_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    gas_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_kg_co2e = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculation_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calculation_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    factor_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calculation_version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculation_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    trade_name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    external_company_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emission_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emission_factor_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    version_label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    version_year = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_factor_sets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emission_factor_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    publisher = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_factor_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emission_factors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    factor_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gas_id = table.Column<Guid>(type: "uuid", nullable: true),
                    factor_kg_per_unit = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    gwp = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    factor_kg_co2e_per_unit = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    calculation_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_factors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emission_inventories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_type = table.Column<string>(type: "text", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_inventories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "external_user_identities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    external_user_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_user_identities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "greenhouse_gases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    default_gwp = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    gwp_source = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    version_year = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_greenhouse_gases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_tenant_id = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_entries");

            migrationBuilder.DropTable(
                name: "activity_units");

            migrationBuilder.DropTable(
                name: "api_clients");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "calculation_results");

            migrationBuilder.DropTable(
                name: "calculation_runs");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "emission_categories");

            migrationBuilder.DropTable(
                name: "emission_factor_sets");

            migrationBuilder.DropTable(
                name: "emission_factor_sources");

            migrationBuilder.DropTable(
                name: "emission_factors");

            migrationBuilder.DropTable(
                name: "emission_inventories");

            migrationBuilder.DropTable(
                name: "external_user_identities");

            migrationBuilder.DropTable(
                name: "greenhouse_gases");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
