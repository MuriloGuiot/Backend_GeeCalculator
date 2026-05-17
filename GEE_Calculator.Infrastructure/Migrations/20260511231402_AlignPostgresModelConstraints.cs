using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEE_Calculator.Migrations
{
    /// <inheritdoc />
    public partial class AlignPostgresModelConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tenants_external_tenant_id",
                table: "tenants",
                column: "external_tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_greenhouse_gases_code_version_year",
                table: "greenhouse_gases",
                columns: new[] { "code", "version_year" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_greenhouse_gases_default_gwp",
                table: "greenhouse_gases",
                sql: "default_gwp > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_greenhouse_gases_version_year",
                table: "greenhouse_gases",
                sql: "version_year between 2000 and 2100");

            migrationBuilder.CreateIndex(
                name: "IX_external_user_identities_tenant_id",
                table: "external_user_identities",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_external_user_identities_tenant_id_provider_external_user_id",
                table: "external_user_identities",
                columns: new[] { "tenant_id", "provider", "external_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emission_inventories_company_id",
                table: "emission_inventories",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_emission_inventories_tenant_company",
                table: "emission_inventories",
                columns: new[] { "tenant_id", "company_id" });

            migrationBuilder.CreateIndex(
                name: "uq_emission_inventories_period_annual",
                table: "emission_inventories",
                columns: new[] { "tenant_id", "company_id", "period_type", "year" },
                unique: true,
                filter: "period_type = 'annual'");

            migrationBuilder.CreateIndex(
                name: "uq_emission_inventories_period_monthly",
                table: "emission_inventories",
                columns: new[] { "tenant_id", "company_id", "period_type", "year", "month" },
                unique: true,
                filter: "period_type = 'monthly'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_inventories_month",
                table: "emission_inventories",
                sql: "month is null or month between 1 and 12");

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_inventories_period_month",
                table: "emission_inventories",
                sql: "(period_type = 'monthly' and month is not null) or (period_type = 'annual' and month is null)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_inventories_year",
                table: "emission_inventories",
                sql: "year between 2000 and 2100");

            migrationBuilder.CreateIndex(
                name: "IX_emission_factors_activity_unit_id",
                table: "emission_factors",
                column: "activity_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_emission_factors_gas_id",
                table: "emission_factors",
                column: "gas_id");

            migrationBuilder.CreateIndex(
                name: "ix_emission_factors_lookup",
                table: "emission_factors",
                columns: new[] { "category_id", "activity_unit_id", "factor_set_id" });

            migrationBuilder.CreateIndex(
                name: "uq_emission_factors_global_lookup",
                table: "emission_factors",
                columns: new[] { "factor_set_id", "category_id", "activity_unit_id", "gas_id" },
                unique: true,
                filter: "tenant_id is null")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_emission_factors_tenant_lookup",
                table: "emission_factors",
                columns: new[] { "tenant_id", "factor_set_id", "category_id", "activity_unit_id", "gas_id" },
                unique: true,
                filter: "tenant_id is not null")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_factors_factor_kg_co2e",
                table: "emission_factors",
                sql: "factor_kg_co2e_per_unit >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_factors_factor_kg_per_unit",
                table: "emission_factors",
                sql: "factor_kg_per_unit is null or factor_kg_per_unit >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_factors_gwp",
                table: "emission_factors",
                sql: "gwp is null or gwp > 0");

            migrationBuilder.CreateIndex(
                name: "IX_emission_factor_sources_code",
                table: "emission_factor_sources",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emission_factor_sets_code_version_label",
                table: "emission_factor_sets",
                columns: new[] { "code", "version_label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emission_factor_sets_source_id_version_year",
                table: "emission_factor_sets",
                columns: new[] { "source_id", "version_year" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_factor_sets_valid_range",
                table: "emission_factor_sets",
                sql: "valid_to is null or valid_from is null or valid_to >= valid_from");

            migrationBuilder.AddCheckConstraint(
                name: "ck_emission_factor_sets_version_year",
                table: "emission_factor_sets",
                sql: "version_year between 2000 and 2100");

            migrationBuilder.CreateIndex(
                name: "IX_emission_categories_code",
                table: "emission_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emission_categories_parent_category_id",
                table: "emission_categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_emission_categories_scope",
                table: "emission_categories",
                column: "scope");

            migrationBuilder.CreateIndex(
                name: "IX_companies_tenant_id",
                table: "companies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_companies_tenant_id_external_company_id",
                table: "companies",
                columns: new[] { "tenant_id", "external_company_id" },
                unique: true,
                filter: "external_company_id is not null");

            migrationBuilder.CreateIndex(
                name: "IX_companies_tenant_id_tax_id",
                table: "companies",
                columns: new[] { "tenant_id", "tax_id" },
                unique: true,
                filter: "tax_id is not null");

            migrationBuilder.CreateIndex(
                name: "IX_calculation_runs_factor_set_id",
                table: "calculation_runs",
                column: "factor_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculation_runs_inventory_id",
                table: "calculation_runs",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculation_runs_tenant_inventory",
                table: "calculation_runs",
                columns: new[] { "tenant_id", "inventory_id" });

            migrationBuilder.CreateIndex(
                name: "IX_calculation_results_calculation_run_id",
                table: "calculation_results",
                column: "calculation_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculation_results_category_id",
                table: "calculation_results",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculation_results_gas_id",
                table: "calculation_results",
                column: "gas_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculation_results_scope_category",
                table: "calculation_results",
                columns: new[] { "scope", "category_id" });

            migrationBuilder.CreateIndex(
                name: "ix_calculation_results_tenant_run",
                table: "calculation_results",
                columns: new[] { "tenant_id", "calculation_run_id" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_calculation_results_total_kg_co2e",
                table: "calculation_results",
                sql: "total_kg_co2e >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_tenant_created_at",
                table: "audit_logs",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_api_clients_key_hash",
                table: "api_clients",
                column: "key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_clients_key_prefix",
                table: "api_clients",
                column: "key_prefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_clients_tenant_id",
                table: "api_clients",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_units_code",
                table: "activity_units",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_activity_entries_activity_unit_id",
                table: "activity_entries",
                column: "activity_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_category",
                table: "activity_entries",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_entries_inventory_id",
                table: "activity_entries",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_metadata",
                table: "activity_entries",
                column: "metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_tenant_inventory",
                table: "activity_entries",
                columns: new[] { "tenant_id", "inventory_id" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_activity_entries_activity_value",
                table: "activity_entries",
                sql: "activity_value >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_activity_entries_activity_units_activity_unit_id",
                table: "activity_entries",
                column: "activity_unit_id",
                principalTable: "activity_units",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_entries_emission_categories_category_id",
                table: "activity_entries",
                column: "category_id",
                principalTable: "emission_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_entries_emission_inventories_inventory_id",
                table: "activity_entries",
                column: "inventory_id",
                principalTable: "emission_inventories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_entries_tenants_tenant_id",
                table: "activity_entries",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_api_clients_tenants_tenant_id",
                table: "api_clients",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_audit_logs_tenants_tenant_id",
                table: "audit_logs",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_calculation_runs_calculation_run_id",
                table: "calculation_results",
                column: "calculation_run_id",
                principalTable: "calculation_runs",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_emission_categories_category_id",
                table: "calculation_results",
                column: "category_id",
                principalTable: "emission_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_greenhouse_gases_gas_id",
                table: "calculation_results",
                column: "gas_id",
                principalTable: "greenhouse_gases",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_tenants_tenant_id",
                table: "calculation_results",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_runs_emission_factor_sets_factor_set_id",
                table: "calculation_runs",
                column: "factor_set_id",
                principalTable: "emission_factor_sets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_runs_emission_inventories_inventory_id",
                table: "calculation_runs",
                column: "inventory_id",
                principalTable: "emission_inventories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_runs_tenants_tenant_id",
                table: "calculation_runs",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_companies_tenants_tenant_id",
                table: "companies",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_categories_emission_categories_parent_category_id",
                table: "emission_categories",
                column: "parent_category_id",
                principalTable: "emission_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_factor_sets_emission_factor_sources_source_id",
                table: "emission_factor_sets",
                column: "source_id",
                principalTable: "emission_factor_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_factors_activity_units_activity_unit_id",
                table: "emission_factors",
                column: "activity_unit_id",
                principalTable: "activity_units",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_factors_emission_categories_category_id",
                table: "emission_factors",
                column: "category_id",
                principalTable: "emission_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_factors_emission_factor_sets_factor_set_id",
                table: "emission_factors",
                column: "factor_set_id",
                principalTable: "emission_factor_sets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_factors_greenhouse_gases_gas_id",
                table: "emission_factors",
                column: "gas_id",
                principalTable: "greenhouse_gases",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_factors_tenants_tenant_id",
                table: "emission_factors",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_inventories_companies_company_id",
                table: "emission_inventories",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_emission_inventories_tenants_tenant_id",
                table: "emission_inventories",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_external_user_identities_tenants_tenant_id",
                table: "external_user_identities",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activity_entries_activity_units_activity_unit_id",
                table: "activity_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_entries_emission_categories_category_id",
                table: "activity_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_entries_emission_inventories_inventory_id",
                table: "activity_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_entries_tenants_tenant_id",
                table: "activity_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_api_clients_tenants_tenant_id",
                table: "api_clients");

            migrationBuilder.DropForeignKey(
                name: "FK_audit_logs_tenants_tenant_id",
                table: "audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_calculation_runs_calculation_run_id",
                table: "calculation_results");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_emission_categories_category_id",
                table: "calculation_results");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_greenhouse_gases_gas_id",
                table: "calculation_results");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_tenants_tenant_id",
                table: "calculation_results");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_runs_emission_factor_sets_factor_set_id",
                table: "calculation_runs");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_runs_emission_inventories_inventory_id",
                table: "calculation_runs");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_runs_tenants_tenant_id",
                table: "calculation_runs");

            migrationBuilder.DropForeignKey(
                name: "FK_companies_tenants_tenant_id",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_categories_emission_categories_parent_category_id",
                table: "emission_categories");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_factor_sets_emission_factor_sources_source_id",
                table: "emission_factor_sets");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_factors_activity_units_activity_unit_id",
                table: "emission_factors");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_factors_emission_categories_category_id",
                table: "emission_factors");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_factors_emission_factor_sets_factor_set_id",
                table: "emission_factors");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_factors_greenhouse_gases_gas_id",
                table: "emission_factors");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_factors_tenants_tenant_id",
                table: "emission_factors");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_inventories_companies_company_id",
                table: "emission_inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_emission_inventories_tenants_tenant_id",
                table: "emission_inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_external_user_identities_tenants_tenant_id",
                table: "external_user_identities");

            migrationBuilder.DropIndex(
                name: "IX_tenants_external_tenant_id",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "IX_greenhouse_gases_code_version_year",
                table: "greenhouse_gases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_greenhouse_gases_default_gwp",
                table: "greenhouse_gases");

            migrationBuilder.DropCheckConstraint(
                name: "ck_greenhouse_gases_version_year",
                table: "greenhouse_gases");

            migrationBuilder.DropIndex(
                name: "IX_external_user_identities_tenant_id",
                table: "external_user_identities");

            migrationBuilder.DropIndex(
                name: "IX_external_user_identities_tenant_id_provider_external_user_id",
                table: "external_user_identities");

            migrationBuilder.DropIndex(
                name: "IX_emission_inventories_company_id",
                table: "emission_inventories");

            migrationBuilder.DropIndex(
                name: "ix_emission_inventories_tenant_company",
                table: "emission_inventories");

            migrationBuilder.DropIndex(
                name: "uq_emission_inventories_period_annual",
                table: "emission_inventories");

            migrationBuilder.DropIndex(
                name: "uq_emission_inventories_period_monthly",
                table: "emission_inventories");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_inventories_month",
                table: "emission_inventories");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_inventories_period_month",
                table: "emission_inventories");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_inventories_year",
                table: "emission_inventories");

            migrationBuilder.DropIndex(
                name: "IX_emission_factors_activity_unit_id",
                table: "emission_factors");

            migrationBuilder.DropIndex(
                name: "IX_emission_factors_gas_id",
                table: "emission_factors");

            migrationBuilder.DropIndex(
                name: "ix_emission_factors_lookup",
                table: "emission_factors");

            migrationBuilder.DropIndex(
                name: "uq_emission_factors_global_lookup",
                table: "emission_factors");

            migrationBuilder.DropIndex(
                name: "uq_emission_factors_tenant_lookup",
                table: "emission_factors");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_factors_factor_kg_co2e",
                table: "emission_factors");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_factors_factor_kg_per_unit",
                table: "emission_factors");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_factors_gwp",
                table: "emission_factors");

            migrationBuilder.DropIndex(
                name: "IX_emission_factor_sources_code",
                table: "emission_factor_sources");

            migrationBuilder.DropIndex(
                name: "IX_emission_factor_sets_code_version_label",
                table: "emission_factor_sets");

            migrationBuilder.DropIndex(
                name: "IX_emission_factor_sets_source_id_version_year",
                table: "emission_factor_sets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_factor_sets_valid_range",
                table: "emission_factor_sets");

            migrationBuilder.DropCheckConstraint(
                name: "ck_emission_factor_sets_version_year",
                table: "emission_factor_sets");

            migrationBuilder.DropIndex(
                name: "IX_emission_categories_code",
                table: "emission_categories");

            migrationBuilder.DropIndex(
                name: "IX_emission_categories_parent_category_id",
                table: "emission_categories");

            migrationBuilder.DropIndex(
                name: "IX_emission_categories_scope",
                table: "emission_categories");

            migrationBuilder.DropIndex(
                name: "IX_companies_tenant_id",
                table: "companies");

            migrationBuilder.DropIndex(
                name: "IX_companies_tenant_id_external_company_id",
                table: "companies");

            migrationBuilder.DropIndex(
                name: "IX_companies_tenant_id_tax_id",
                table: "companies");

            migrationBuilder.DropIndex(
                name: "IX_calculation_runs_factor_set_id",
                table: "calculation_runs");

            migrationBuilder.DropIndex(
                name: "IX_calculation_runs_inventory_id",
                table: "calculation_runs");

            migrationBuilder.DropIndex(
                name: "ix_calculation_runs_tenant_inventory",
                table: "calculation_runs");

            migrationBuilder.DropIndex(
                name: "IX_calculation_results_calculation_run_id",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "IX_calculation_results_category_id",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "IX_calculation_results_gas_id",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "ix_calculation_results_scope_category",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "ix_calculation_results_tenant_run",
                table: "calculation_results");

            migrationBuilder.DropCheckConstraint(
                name: "ck_calculation_results_total_kg_co2e",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "ix_audit_logs_tenant_created_at",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_api_clients_key_hash",
                table: "api_clients");

            migrationBuilder.DropIndex(
                name: "IX_api_clients_key_prefix",
                table: "api_clients");

            migrationBuilder.DropIndex(
                name: "IX_api_clients_tenant_id",
                table: "api_clients");

            migrationBuilder.DropIndex(
                name: "IX_activity_units_code",
                table: "activity_units");

            migrationBuilder.DropIndex(
                name: "IX_activity_entries_activity_unit_id",
                table: "activity_entries");

            migrationBuilder.DropIndex(
                name: "ix_activity_entries_category",
                table: "activity_entries");

            migrationBuilder.DropIndex(
                name: "IX_activity_entries_inventory_id",
                table: "activity_entries");

            migrationBuilder.DropIndex(
                name: "ix_activity_entries_metadata",
                table: "activity_entries");

            migrationBuilder.DropIndex(
                name: "ix_activity_entries_tenant_inventory",
                table: "activity_entries");

            migrationBuilder.DropCheckConstraint(
                name: "ck_activity_entries_activity_value",
                table: "activity_entries");
        }
    }
}
