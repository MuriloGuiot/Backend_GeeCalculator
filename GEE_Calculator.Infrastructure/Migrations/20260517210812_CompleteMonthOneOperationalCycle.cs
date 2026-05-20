using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEE_Calculator.Migrations
{
    /// <inheritdoc />
    public partial class CompleteMonthOneOperationalCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "activity_entry_id",
                table: "calculation_results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "activity_unit_id",
                table: "calculation_results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "activity_value",
                table: "calculation_results",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "biogenic_kg_co2",
                table: "calculation_results",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "biogenic_removal_kg_co2",
                table: "calculation_results",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "emission_factor_id",
                table: "calculation_results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "factor_kg_co2e_per_unit",
                table: "calculation_results",
                type: "numeric(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "details",
                table: "audit_logs",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "calculation_method",
                table: "activity_entries",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "factor");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "activity_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_name",
                table: "activity_entries",
                type: "character varying(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "activity_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "survey_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    version_label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    factor_set_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_survey_templates_emission_factor_sets_factor_set_id",
                        column: x => x.factor_set_id,
                        principalTable: "emission_factor_sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "survey_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_survey_sections_survey_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "survey_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "survey_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    prompt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    help_text = table.Column<string>(type: "text", nullable: true),
                    answer_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    visibility_rule = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    mapping = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_survey_questions_survey_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "survey_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "survey_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    label = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    value = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_survey_options_survey_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "survey_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calculation_results_activity_entry_id",
                table: "calculation_results",
                column: "activity_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculation_results_activity_unit_id",
                table: "calculation_results",
                column: "activity_unit_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculation_results_emission_factor_id",
                table: "calculation_results",
                column: "emission_factor_id");

            migrationBuilder.CreateIndex(
                name: "ix_calculation_results_tenant_activity_entry",
                table: "calculation_results",
                columns: new[] { "tenant_id", "activity_entry_id" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_calculation_results_biogenic_kg_co2",
                table: "calculation_results",
                sql: "biogenic_kg_co2 >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_calculation_results_biogenic_removal_kg_co2",
                table: "calculation_results",
                sql: "biogenic_removal_kg_co2 >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_activity_entries_tenant_deleted_at",
                table: "activity_entries",
                columns: new[] { "tenant_id", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_survey_options_question_sort_order",
                table: "survey_options",
                columns: new[] { "question_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "uq_survey_options_question_code",
                table: "survey_options",
                columns: new[] { "question_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_survey_questions_mapping",
                table: "survey_questions",
                column: "mapping")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_survey_questions_section_sort_order",
                table: "survey_questions",
                columns: new[] { "section_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_survey_questions_visibility_rule",
                table: "survey_questions",
                column: "visibility_rule")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "uq_survey_questions_section_code",
                table: "survey_questions",
                columns: new[] { "section_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_survey_sections_template_sort_order",
                table: "survey_sections",
                columns: new[] { "template_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "uq_survey_sections_template_code",
                table: "survey_sections",
                columns: new[] { "template_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_survey_templates_active_code",
                table: "survey_templates",
                columns: new[] { "is_active", "code" });

            migrationBuilder.CreateIndex(
                name: "IX_survey_templates_factor_set_id",
                table: "survey_templates",
                column: "factor_set_id");

            migrationBuilder.CreateIndex(
                name: "uq_survey_templates_code",
                table: "survey_templates",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_activity_entries_activity_entry_id",
                table: "calculation_results",
                column: "activity_entry_id",
                principalTable: "activity_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_activity_units_activity_unit_id",
                table: "calculation_results",
                column: "activity_unit_id",
                principalTable: "activity_units",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculation_results_emission_factors_emission_factor_id",
                table: "calculation_results",
                column: "emission_factor_id",
                principalTable: "emission_factors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_activity_entries_activity_entry_id",
                table: "calculation_results");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_activity_units_activity_unit_id",
                table: "calculation_results");

            migrationBuilder.DropForeignKey(
                name: "FK_calculation_results_emission_factors_emission_factor_id",
                table: "calculation_results");

            migrationBuilder.DropTable(
                name: "survey_options");

            migrationBuilder.DropTable(
                name: "survey_questions");

            migrationBuilder.DropTable(
                name: "survey_sections");

            migrationBuilder.DropTable(
                name: "survey_templates");

            migrationBuilder.DropIndex(
                name: "IX_calculation_results_activity_entry_id",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "IX_calculation_results_activity_unit_id",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "IX_calculation_results_emission_factor_id",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "ix_calculation_results_tenant_activity_entry",
                table: "calculation_results");

            migrationBuilder.DropCheckConstraint(
                name: "ck_calculation_results_biogenic_kg_co2",
                table: "calculation_results");

            migrationBuilder.DropCheckConstraint(
                name: "ck_calculation_results_biogenic_removal_kg_co2",
                table: "calculation_results");

            migrationBuilder.DropIndex(
                name: "ix_activity_entries_tenant_deleted_at",
                table: "activity_entries");

            migrationBuilder.DropColumn(
                name: "activity_entry_id",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "activity_unit_id",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "activity_value",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "biogenic_kg_co2",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "biogenic_removal_kg_co2",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "emission_factor_id",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "factor_kg_co2e_per_unit",
                table: "calculation_results");

            migrationBuilder.DropColumn(
                name: "details",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "calculation_method",
                table: "activity_entries");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "activity_entries");

            migrationBuilder.DropColumn(
                name: "source_name",
                table: "activity_entries");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "activity_entries");
        }
    }
}
