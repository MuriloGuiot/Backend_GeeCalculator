# Dicionario de dados inicial - PostgreSQL

Data: 20/04/2026

Este documento descreve o modelo inicial de dados da API `GEE_Calculator`. A proposta e servir como contrato tecnico entre a AgrocarbonBR e a GoLedger para a primeira versao do banco PostgreSQL, mantendo foco em multi-tenancy, rastreabilidade e calculo de emissoes conforme GHG Protocol.

## Convencoes

- Banco alvo: PostgreSQL.
- Nomes de tabelas e colunas: `snake_case`.
- Chaves primarias: `uuid`.
- Datas: `timestamp with time zone` (`timestamptz`).
- Valores monetarios, fatores e emissoes: `numeric`, evitando perda de precisao.
- Isolamento logico: tabelas operacionais carregam `tenant_id`.
- Exclusao de dados: inicialmente nao ha soft delete; isso deve ser reavaliado antes de producao.

## Enums

### `emission_scope`

Representa os escopos do GHG Protocol.

| Valor | Descricao |
| --- | --- |
| `scope_1` | Emissoes diretas de fontes controladas pela empresa. |
| `scope_2` | Emissoes indiretas por energia adquirida. |
| `scope_3` | Outras emissoes indiretas da cadeia de valor. |

### `period_type`

Representa o periodo do inventario.

| Valor | Descricao |
| --- | --- |
| `monthly` | Inventario mensal. |
| `annual` | Inventario anual. |

## Tabela `tenants`

Representa a organizacao isolada logicamente na calculadora. Em geral, deve corresponder ao tenant/organizacao recebido da GoGreen.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do tenant. |
| `external_tenant_id` | `varchar(120)` | Sim | Identificador do tenant na GoGreen/Keycloak. |
| `name` | `varchar(200)` | Sim | Nome do tenant/organizacao. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `external_tenant_id` deve ser unico.

## Tabela `companies`

Armazena os dados cadastrais da empresa/CNPJ que utiliza a calculadora.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno da empresa. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario da empresa. |
| `legal_name` | `varchar(240)` | Sim | Razao social. |
| `trade_name` | `varchar(240)` | Nao | Nome fantasia. |
| `tax_id` | `varchar(32)` | Nao | CNPJ ou identificador fiscal. |
| `external_company_id` | `varchar(120)` | Nao | Identificador da empresa na GoGreen. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- `tenant_id + external_company_id` deve ser unico quando `external_company_id` existir.
- `tenant_id + tax_id` deve ser unico quando `tax_id` existir.

## Tabela `external_user_identities`

Mapeia usuarios autenticados na GoGreen/Keycloak para o contexto interno da calculadora.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno da identidade. |
| `tenant_id` | `uuid` | Sim | Tenant ao qual o usuario pertence. |
| `provider` | `varchar(80)` | Sim | Provedor de identidade, por exemplo `keycloak`. |
| `external_user_id` | `varchar(160)` | Sim | `sub` ou identificador equivalente recebido no token. |
| `email` | `varchar(320)` | Sim | E-mail do usuario. |
| `display_name` | `varchar(200)` | Nao | Nome exibido do usuario. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- `tenant_id + provider + external_user_id` deve ser unico.

## Tabela `emission_inventories`

Representa um inventario de emissoes de uma empresa em um periodo mensal ou anual.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do inventario. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario do inventario. |
| `company_id` | `uuid` | Sim | Empresa dona do inventario. |
| `period_type` | `period_type` | Sim | Tipo do periodo: mensal ou anual. |
| `year` | `integer` | Sim | Ano do inventario. |
| `month` | `integer` | Nao | Mes do inventario, obrigatorio apenas quando `period_type = monthly`. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- `company_id` referencia `companies(id)`.
- `month` deve ficar entre 1 e 12 quando informado.
- Inventarios mensais exigem `month`.
- Inventarios anuais nao devem ter `month`.
- `tenant_id + company_id + period_type + year + month` deve evitar duplicidade logica de periodo.

## Tabela `activity_entries`

Guarda os dados de atividade coletados pelo questionario/Smart Survey e usados no calculo GEE.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno da entrada. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario da entrada. |
| `inventory_id` | `uuid` | Sim | Inventario associado. |
| `scope` | `emission_scope` | Sim | Escopo GHG Protocol. |
| `category` | `varchar(120)` | Sim | Categoria normalizada, por exemplo `energia_eletrica_sin`. |
| `activity_unit` | `varchar(40)` | Sim | Unidade do dado de atividade, por exemplo `kWh`, `km`, `litro`. |
| `activity_value` | `numeric(18,6)` | Sim | Valor informado pelo usuario. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- `inventory_id` referencia `emission_inventories(id)`.
- `activity_value` deve ser maior ou igual a zero.

## Tabela `emission_factors`

Armazena fatores de emissao versionados, permitindo atualizar indices sem alterar codigo.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do fator. |
| `scope` | `emission_scope` | Sim | Escopo GHG Protocol ao qual o fator se aplica. |
| `category` | `varchar(120)` | Sim | Categoria normalizada usada para buscar o fator. |
| `source` | `varchar(240)` | Sim | Fonte do fator, por exemplo GHG Protocol, MCTI, SIN ou DEFRA. |
| `unit` | `varchar(40)` | Sim | Unidade base do fator. |
| `factor_kg_co2e` | `numeric(18,8)` | Sim | Fator em kgCO2e por unidade de atividade. |
| `gwp` | `numeric(18,8)` | Sim | Potencial de aquecimento global aplicado. |
| `version_year` | `integer` | Sim | Ano/versao do fator. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `scope + category + unit + version_year + source` deve ser unico.
- `factor_kg_co2e` deve ser maior ou igual a zero.
- `gwp` deve ser maior que zero.

Observacao: fatores de emissao podem ser globais, sem `tenant_id`, para preservar uma base tecnica comum e auditavel. Caso a GoLedger exija fatores customizados por cliente, uma coluna `tenant_id` opcional pode ser adicionada em uma versao posterior.

## Tabela `calculation_runs`

Registra cada execucao do motor de calculo.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno da execucao. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario da execucao. |
| `inventory_id` | `uuid` | Sim | Inventario calculado. |
| `calculation_version` | `varchar(40)` | Sim | Versao do motor, inicialmente `gee-v0`. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- `inventory_id` referencia `emission_inventories(id)`.

## Tabela `calculation_results`

Armazena os resultados agregados por escopo para uma execucao de calculo.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do resultado. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario do resultado. |
| `calculation_run_id` | `uuid` | Sim | Execucao de calculo associada. |
| `scope` | `emission_scope` | Sim | Escopo GHG Protocol agregado. |
| `total_kg_co2e` | `numeric(18,6)` | Sim | Total calculado em kgCO2e. |
| `created_at` | `timestamptz` | Sim | Data de criacao do registro. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- `calculation_run_id` referencia `calculation_runs(id)`.
- `calculation_run_id + scope` deve ser unico.
- `total_kg_co2e` deve ser maior ou igual a zero.
- `total_t_co2e` deve ser calculado na aplicacao como `total_kg_co2e / 1000`.

## Tabela `audit_logs`

Registra eventos sensiveis para rastreabilidade tecnica e governanca.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do evento. |
| `tenant_id` | `uuid` | Sim | Tenant onde o evento ocorreu. |
| `actor_external_user_id` | `varchar(160)` | Sim | Usuario externo responsavel pela acao. |
| `action` | `varchar(120)` | Sim | Acao realizada, por exemplo `activity_entry.created`. |
| `entity_name` | `varchar(120)` | Sim | Nome da entidade afetada. |
| `entity_id` | `varchar(80)` | Nao | Identificador da entidade afetada. |
| `created_at` | `timestamptz` | Sim | Data de criacao do evento. |

Indices e regras:

- `tenant_id` referencia `tenants(id)`.
- Deve haver indice por `tenant_id + created_at` para consultas de auditoria.

## Pendencias para confirmar com a GoLedger

- Nome oficial das claims de `tenant_id`, empresa, CNPJ, roles e usuario.
- Se cada tenant tera uma unica empresa ou multiplas empresas/CNPJs.
- Se fatores de emissao serao globais ou poderao ser customizados por tenant.
- Fonte oficial inicial dos fatores de emissao para Escopos 1, 2 e 3.
- Nivel de detalhamento necessario para auditoria: antes/depois de valores, IP, user agent e origem da requisicao.
- Regras de retencao, anonimizacao e exclusao de dados por LGPD.
