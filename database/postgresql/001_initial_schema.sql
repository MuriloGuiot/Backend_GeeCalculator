create extension if not exists pgcrypto;

do $$
begin
    if not exists (select 1 from pg_type where typname = 'emission_scope') then
        create type emission_scope as enum ('scope_1', 'scope_2', 'scope_3');
    end if;

    if not exists (select 1 from pg_type where typname = 'period_type') then
        create type period_type as enum ('monthly', 'annual');
    end if;
end
$$;

create table if not exists tenants (
    id uuid primary key default gen_random_uuid(),
    external_tenant_id varchar(120) not null,
    name varchar(200) not null,
    created_at timestamptz not null default now(),
    constraint uq_tenants_external_tenant_id unique (external_tenant_id)
);

create table if not exists api_clients (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    name varchar(160) not null,
    key_prefix varchar(24) not null,
    key_hash varchar(160) not null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    revoked_at timestamptz,
    constraint uq_api_clients_key_prefix unique (key_prefix),
    constraint uq_api_clients_key_hash unique (key_hash)
);

create index if not exists ix_api_clients_tenant_id
    on api_clients (tenant_id);

create table if not exists companies (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    legal_name varchar(240) not null,
    trade_name varchar(240),
    tax_id varchar(32),
    external_company_id varchar(120),
    created_at timestamptz not null default now()
);

create unique index if not exists uq_companies_tenant_external_company_id
    on companies (tenant_id, external_company_id)
    where external_company_id is not null;

create unique index if not exists uq_companies_tenant_tax_id
    on companies (tenant_id, tax_id)
    where tax_id is not null;

create index if not exists ix_companies_tenant_id
    on companies (tenant_id);

create table if not exists external_user_identities (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    provider varchar(80) not null,
    external_user_id varchar(160) not null,
    email varchar(320),
    display_name varchar(200),
    created_at timestamptz not null default now(),
    constraint uq_external_user_identities_provider_user
        unique (tenant_id, provider, external_user_id)
);

create index if not exists ix_external_user_identities_tenant_id
    on external_user_identities (tenant_id);

create table if not exists activity_units (
    id uuid primary key default gen_random_uuid(),
    code varchar(40) not null,
    name varchar(120) not null,
    dimension varchar(80) not null,
    created_at timestamptz not null default now(),
    constraint uq_activity_units_code unique (code)
);

create table if not exists emission_categories (
    id uuid primary key default gen_random_uuid(),
    scope emission_scope not null,
    code varchar(120) not null,
    name varchar(200) not null,
    description text,
    parent_category_id uuid references emission_categories (id),
    created_at timestamptz not null default now(),
    constraint uq_emission_categories_code unique (code)
);

create index if not exists ix_emission_categories_scope
    on emission_categories (scope);

create table if not exists greenhouse_gases (
    id uuid primary key default gen_random_uuid(),
    code varchar(24) not null,
    name varchar(120) not null,
    default_gwp numeric(18, 8) not null,
    gwp_source varchar(240) not null,
    version_year integer not null,
    created_at timestamptz not null default now(),
    constraint uq_greenhouse_gases_code_version unique (code, version_year),
    constraint ck_greenhouse_gases_default_gwp check (default_gwp > 0),
    constraint ck_greenhouse_gases_version_year check (version_year between 2000 and 2100)
);

create table if not exists emission_factor_sources (
    id uuid primary key default gen_random_uuid(),
    code varchar(80) not null,
    name varchar(240) not null,
    publisher varchar(240),
    source_url text,
    created_at timestamptz not null default now(),
    constraint uq_emission_factor_sources_code unique (code)
);

create table if not exists emission_factor_sets (
    id uuid primary key default gen_random_uuid(),
    source_id uuid not null references emission_factor_sources (id),
    code varchar(120) not null,
    name varchar(240) not null,
    version_label varchar(80) not null,
    version_year integer not null,
    valid_from date,
    valid_to date,
    created_at timestamptz not null default now(),
    constraint uq_emission_factor_sets_code_version unique (code, version_label),
    constraint ck_emission_factor_sets_version_year check (version_year between 2000 and 2100),
    constraint ck_emission_factor_sets_valid_range check (valid_to is null or valid_from is null or valid_to >= valid_from)
);

create index if not exists ix_emission_factor_sets_source_year
    on emission_factor_sets (source_id, version_year);

create table if not exists emission_factors (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid references tenants (id),
    factor_set_id uuid not null references emission_factor_sets (id),
    category_id uuid not null references emission_categories (id),
    activity_unit_id uuid not null references activity_units (id),
    gas_id uuid references greenhouse_gases (id),
    factor_kg_per_unit numeric(18, 8),
    gwp numeric(18, 8),
    factor_kg_co2e_per_unit numeric(18, 8) not null,
    calculation_notes text,
    created_at timestamptz not null default now(),
    constraint ck_emission_factors_factor_kg_per_unit
        check (factor_kg_per_unit is null or factor_kg_per_unit >= 0),
    constraint ck_emission_factors_gwp
        check (gwp is null or gwp > 0),
    constraint ck_emission_factors_factor_kg_co2e
        check (factor_kg_co2e_per_unit >= 0)
);

create unique index if not exists uq_emission_factors_global_lookup
    on emission_factors (factor_set_id, category_id, activity_unit_id, coalesce(gas_id, '00000000-0000-0000-0000-000000000000'::uuid))
    where tenant_id is null;

create unique index if not exists uq_emission_factors_tenant_lookup
    on emission_factors (tenant_id, factor_set_id, category_id, activity_unit_id, coalesce(gas_id, '00000000-0000-0000-0000-000000000000'::uuid))
    where tenant_id is not null;

create index if not exists ix_emission_factors_lookup
    on emission_factors (category_id, activity_unit_id, factor_set_id);

create table if not exists emission_inventories (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    company_id uuid not null references companies (id),
    period_type period_type not null,
    year integer not null,
    month integer,
    created_at timestamptz not null default now(),
    constraint ck_emission_inventories_year
        check (year between 2000 and 2100),
    constraint ck_emission_inventories_month
        check (month is null or month between 1 and 12),
    constraint ck_emission_inventories_period_month
        check (
            (period_type = 'monthly' and month is not null)
            or (period_type = 'annual' and month is null)
        )
);

create unique index if not exists uq_emission_inventories_period_monthly
    on emission_inventories (tenant_id, company_id, period_type, year, month)
    where period_type = 'monthly';

create unique index if not exists uq_emission_inventories_period_annual
    on emission_inventories (tenant_id, company_id, period_type, year)
    where period_type = 'annual';

create index if not exists ix_emission_inventories_tenant_company
    on emission_inventories (tenant_id, company_id);

create table if not exists activity_entries (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    inventory_id uuid not null references emission_inventories (id),
    category_id uuid not null references emission_categories (id),
    activity_unit_id uuid not null references activity_units (id),
    activity_value numeric(18, 6) not null,
    source_name varchar(240),
    calculation_method varchar(80) not null default 'factor',
    evidence_ref text,
    metadata jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now(),
    updated_at timestamptz,
    deleted_at timestamptz,
    constraint ck_activity_entries_activity_value
        check (activity_value >= 0)
);

create index if not exists ix_activity_entries_tenant_inventory
    on activity_entries (tenant_id, inventory_id);

create index if not exists ix_activity_entries_category
    on activity_entries (category_id);

create index if not exists ix_activity_entries_tenant_deleted_at
    on activity_entries (tenant_id, deleted_at);

create index if not exists ix_activity_entries_metadata
    on activity_entries using gin (metadata);

create table if not exists calculation_runs (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    inventory_id uuid not null references emission_inventories (id),
    factor_set_id uuid not null references emission_factor_sets (id),
    calculation_version varchar(40) not null default 'gee-v0',
    created_at timestamptz not null default now()
);

create index if not exists ix_calculation_runs_tenant_inventory
    on calculation_runs (tenant_id, inventory_id);

create table if not exists calculation_results (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    calculation_run_id uuid not null references calculation_runs (id),
    activity_entry_id uuid references activity_entries (id),
    scope emission_scope not null,
    category_id uuid references emission_categories (id),
    gas_id uuid references greenhouse_gases (id),
    emission_factor_id uuid references emission_factors (id),
    activity_unit_id uuid references activity_units (id),
    activity_value numeric(18, 6),
    factor_kg_co2e_per_unit numeric(18, 8),
    total_kg_co2e numeric(18, 6) not null,
    biogenic_kg_co2 numeric(18, 6) not null default 0,
    biogenic_removal_kg_co2 numeric(18, 6) not null default 0,
    created_at timestamptz not null default now(),
    constraint ck_calculation_results_total_kg_co2e
        check (total_kg_co2e >= 0),
    constraint ck_calculation_results_biogenic_kg_co2
        check (biogenic_kg_co2 >= 0),
    constraint ck_calculation_results_biogenic_removal_kg_co2
        check (biogenic_removal_kg_co2 >= 0)
);

create index if not exists ix_calculation_results_tenant_run
    on calculation_results (tenant_id, calculation_run_id);

create index if not exists ix_calculation_results_scope_category
    on calculation_results (scope, category_id);

create index if not exists ix_calculation_results_tenant_activity_entry
    on calculation_results (tenant_id, activity_entry_id);

create table if not exists audit_logs (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references tenants (id),
    actor_external_user_id varchar(160),
    action varchar(120) not null,
    entity_name varchar(120) not null,
    entity_id varchar(80),
    details jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);

create index if not exists ix_audit_logs_tenant_created_at
    on audit_logs (tenant_id, created_at desc);

create table if not exists survey_templates (
    id uuid primary key default gen_random_uuid(),
    code varchar(120) not null,
    name varchar(240) not null,
    version_label varchar(80) not null,
    factor_set_id uuid references emission_factor_sets (id),
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    constraint uq_survey_templates_code unique (code)
);

create index if not exists ix_survey_templates_active_code
    on survey_templates (is_active, code);

create table if not exists survey_sections (
    id uuid primary key default gen_random_uuid(),
    template_id uuid not null references survey_templates (id) on delete cascade,
    code varchar(120) not null,
    title varchar(240) not null,
    description text,
    sort_order integer not null,
    created_at timestamptz not null default now(),
    constraint uq_survey_sections_template_code unique (template_id, code)
);

create index if not exists ix_survey_sections_template_sort_order
    on survey_sections (template_id, sort_order);

create table if not exists survey_questions (
    id uuid primary key default gen_random_uuid(),
    section_id uuid not null references survey_sections (id) on delete cascade,
    code varchar(160) not null,
    prompt varchar(500) not null,
    help_text text,
    answer_type varchar(40) not null,
    is_required boolean not null default false,
    sort_order integer not null,
    visibility_rule jsonb not null default '{}'::jsonb,
    mapping jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now(),
    constraint uq_survey_questions_section_code unique (section_id, code)
);

create index if not exists ix_survey_questions_section_sort_order
    on survey_questions (section_id, sort_order);

create index if not exists ix_survey_questions_visibility_rule
    on survey_questions using gin (visibility_rule);

create index if not exists ix_survey_questions_mapping
    on survey_questions using gin (mapping);

create table if not exists survey_options (
    id uuid primary key default gen_random_uuid(),
    question_id uuid not null references survey_questions (id) on delete cascade,
    code varchar(120) not null,
    label varchar(240) not null,
    value varchar(240),
    sort_order integer not null,
    created_at timestamptz not null default now(),
    constraint uq_survey_options_question_code unique (question_id, code)
);

create index if not exists ix_survey_options_question_sort_order
    on survey_options (question_id, sort_order);
