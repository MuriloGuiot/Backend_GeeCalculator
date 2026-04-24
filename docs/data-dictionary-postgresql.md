# Dicionario de dados inicial - PostgreSQL

Data: 22/04/2026

Este documento descreve o modelo inicial de dados da API `GEE_Calculator`. A modelagem foi revisada apos alinhamento com a GoLedger: a calculadora deve ser agnostica a sistemas externos e priorizar a logica core de calculo GEE. Por isso, a autenticacao nao depende diretamente de Keycloak neste momento; o isolamento por `tenant_id` permanece obrigatorio e pode ser recebido por header ou resolvido via API Key simplificada.

## Decisao arquitetural

- A calculadora deve funcionar de forma independente da plataforma GoGreen.
- O motor core deve ser plugavel em outros sistemas futuramente.
- O `tenant_id` continua sendo parte central do modelo para evitar mistura de dados entre empresas.
- A autenticacao atual pode ser simples: `X-Tenant-Id` e `X-Api-Key`.
- Keycloak/JWT pode ser integrado no futuro sem alterar o modelo principal de dados.
- Os fatores de emissao devem ser cadastraveis e versionados sem alterar codigo.

## Convencoes

- Banco alvo: PostgreSQL.
- Nomes de tabelas e colunas: `snake_case`.
- Chaves primarias: `uuid`.
- Datas: `timestamp with time zone` (`timestamptz`).
- Valores de atividade, GWP e emissoes: `numeric`, evitando perda de precisao.
- Dados operacionais de cliente sempre carregam `tenant_id`.
- Catalogos tecnicos globais, como gases, unidades e categorias, nao dependem obrigatoriamente de tenant.

## Enums

### `emission_scope`

| Valor | Descricao |
| --- | --- |
| `scope_1` | Emissoes diretas de fontes controladas pela empresa. |
| `scope_2` | Emissoes indiretas por energia adquirida. |
| `scope_3` | Outras emissoes indiretas da cadeia de valor. |

### `period_type`

| Valor | Descricao |
| --- | --- |
| `monthly` | Inventario mensal. |
| `annual` | Inventario anual. |

## Tabelas de tenancy e acesso

### `tenants`

Representa uma organizacao isolada logicamente na calculadora.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do tenant. |
| `external_tenant_id` | `varchar(120)` | Sim | Identificador externo enviado por GoGreen ou outro integrador. |
| `name` | `varchar(200)` | Sim | Nome da organizacao. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

Regra principal: `external_tenant_id` e unico.

### `api_clients`

Tabela opcional para autenticacao simplificada enquanto nao houver Keycloak/JWT. Uma API Key identifica um cliente de integracao e aponta para um `tenant_id`.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno do client. |
| `tenant_id` | `uuid` | Sim | Tenant ao qual a chave pertence. |
| `name` | `varchar(160)` | Sim | Nome da integracao ou cliente. |
| `key_prefix` | `varchar(24)` | Sim | Prefixo publico da chave para identificacao. |
| `key_hash` | `varchar(160)` | Sim | Hash da chave, nunca a chave pura. |
| `is_active` | `boolean` | Sim | Indica se a chave esta ativa. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |
| `revoked_at` | `timestamptz` | Nao | Data de revogacao. |

Uso esperado nos endpoints:

- Header `X-Api-Key`: autentica a chamada.
- Header `X-Tenant-Id`: informa ou confirma o tenant da requisicao.

### `companies`

Empresas/CNPJs vinculados a um tenant.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno da empresa. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario. |
| `legal_name` | `varchar(240)` | Sim | Razao social. |
| `trade_name` | `varchar(240)` | Nao | Nome fantasia. |
| `tax_id` | `varchar(32)` | Nao | CNPJ ou identificador fiscal. |
| `external_company_id` | `varchar(120)` | Nao | ID externo no sistema integrador. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

Regras principais:

- `tenant_id + tax_id` e unico quando `tax_id` existir.
- `tenant_id + external_company_id` e unico quando `external_company_id` existir.

### `external_user_identities`

Mapeia usuarios externos de forma generica. O `provider` pode ser `keycloak`, `gogreen`, `api_key`, `manual` ou outro.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador interno. |
| `tenant_id` | `uuid` | Sim | Tenant do usuario. |
| `provider` | `varchar(80)` | Sim | Provedor de identidade. |
| `external_user_id` | `varchar(160)` | Sim | ID externo do usuario. |
| `email` | `varchar(320)` | Nao | E-mail, se disponivel. |
| `display_name` | `varchar(200)` | Nao | Nome exibido. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

## Catalogos tecnicos da calculadora

### `activity_units`

Catalogo de unidades aceitas pela calculadora, como `kWh`, `L`, `km`, `kg`, `m3`.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da unidade. |
| `code` | `varchar(40)` | Sim | Codigo tecnico da unidade. |
| `name` | `varchar(120)` | Sim | Nome legivel. |
| `dimension` | `varchar(80)` | Sim | Dimensao: energia, volume, massa, distancia etc. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `emission_categories`

Catalogo hierarquico de categorias GEE. Esta tabela substitui o uso de strings soltas como `energia_eletrica_sin` dentro das entradas.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da categoria. |
| `scope` | `emission_scope` | Sim | Escopo GHG Protocol. |
| `code` | `varchar(120)` | Sim | Codigo normalizado da categoria. |
| `name` | `varchar(200)` | Sim | Nome legivel. |
| `description` | `text` | Nao | Descricao tecnica. |
| `parent_category_id` | `uuid` | Nao | Categoria pai, para agrupamentos. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

Exemplos futuros:

- `combustao_movel`
- `diesel_rodoviario`
- `gasolina_automotiva`
- `energia_eletrica_sin`
- `residuos_aterro`
- `viagens_aereas`

### `greenhouse_gases`

Catalogo de gases e seus GWPs por versao/ano de referencia.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do gas. |
| `code` | `varchar(24)` | Sim | Codigo: `CO2`, `CH4`, `N2O`, `R410A` etc. |
| `name` | `varchar(120)` | Sim | Nome do gas. |
| `default_gwp` | `numeric(18,8)` | Sim | Potencial de aquecimento global. |
| `gwp_source` | `varchar(240)` | Sim | Fonte do GWP. |
| `version_year` | `integer` | Sim | Ano/versao do GWP. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

## Fatores de emissao

### `emission_factor_sources`

Fonte institucional dos fatores.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da fonte. |
| `code` | `varchar(80)` | Sim | Codigo da fonte. |
| `name` | `varchar(240)` | Sim | Nome da fonte. |
| `publisher` | `varchar(240)` | Nao | Publicador. |
| `source_url` | `text` | Nao | Link de referencia. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `emission_factor_sets`

Agrupa uma versao de fatores. Isso permite rodar calculos historicos com o mesmo conjunto de fatores usado na epoca.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do conjunto. |
| `source_id` | `uuid` | Sim | Fonte dos fatores. |
| `code` | `varchar(120)` | Sim | Codigo do conjunto. |
| `name` | `varchar(240)` | Sim | Nome legivel. |
| `version_label` | `varchar(80)` | Sim | Rotulo de versao. |
| `version_year` | `integer` | Sim | Ano de referencia. |
| `valid_from` | `date` | Nao | Inicio da validade. |
| `valid_to` | `date` | Nao | Fim da validade. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `emission_factors`

Tabela central da logica core. Cada linha diz quanto uma unidade de atividade em determinada categoria emite em kgCO2e.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do fator. |
| `tenant_id` | `uuid` | Nao | Opcional para fatores customizados por tenant. |
| `factor_set_id` | `uuid` | Sim | Conjunto/versionamento do fator. |
| `category_id` | `uuid` | Sim | Categoria GEE. |
| `activity_unit_id` | `uuid` | Sim | Unidade do dado de atividade. |
| `gas_id` | `uuid` | Nao | Gas associado, quando aplicavel. |
| `factor_kg_per_unit` | `numeric(18,8)` | Nao | Fator em kg do gas por unidade. |
| `gwp` | `numeric(18,8)` | Nao | GWP usado no fator. |
| `factor_kg_co2e_per_unit` | `numeric(18,8)` | Sim | Fator final em kgCO2e por unidade. |
| `calculation_notes` | `text` | Nao | Observacoes tecnicas. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

Regra de calculo principal:

```text
emissao_kg_co2e = activity_value * factor_kg_co2e_per_unit
```

Quando o fator for derivado de um gas especifico:

```text
factor_kg_co2e_per_unit = factor_kg_per_unit * gwp
```

Essa estrutura permite cadastrar novos combustiveis, gases refrigerantes, fatores do SIN, residuos ou viagens sem alterar o codigo do motor. O codigo apenas busca o fator correto por categoria, unidade, versao e tenant opcional.

## Dados operacionais da calculadora

### `emission_inventories`

Inventario mensal ou anual de uma empresa.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do inventario. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario. |
| `company_id` | `uuid` | Sim | Empresa dona do inventario. |
| `period_type` | `period_type` | Sim | Mensal ou anual. |
| `year` | `integer` | Sim | Ano. |
| `month` | `integer` | Nao | Mes, obrigatorio para inventario mensal. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `activity_entries`

Entradas informadas pelo usuario ou sistema integrador.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da entrada. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario. |
| `inventory_id` | `uuid` | Sim | Inventario associado. |
| `category_id` | `uuid` | Sim | Categoria GEE. |
| `activity_unit_id` | `uuid` | Sim | Unidade da atividade. |
| `activity_value` | `numeric(18,6)` | Sim | Valor informado. |
| `evidence_ref` | `text` | Nao | Referencia de evidencia ou documento. |
| `metadata` | `jsonb` | Sim | Campos especificos por categoria. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

O campo `metadata` evita alterar a tabela para cada categoria nova. Exemplo para gas refrigerante:

```json
{
  "equipment": "ar_condicionado",
  "refrigerant": "R410A",
  "leakage_rate": 0.08
}
```

### `calculation_runs`

Execucao do motor de calculo.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da execucao. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario. |
| `inventory_id` | `uuid` | Sim | Inventario calculado. |
| `factor_set_id` | `uuid` | Sim | Versao de fatores usada. |
| `calculation_version` | `varchar(40)` | Sim | Versao do motor. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `calculation_results`

Resultados agregados por escopo, categoria e gas quando necessario.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do resultado. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario. |
| `calculation_run_id` | `uuid` | Sim | Execucao associada. |
| `scope` | `emission_scope` | Sim | Escopo agregado. |
| `category_id` | `uuid` | Nao | Categoria agregada. |
| `gas_id` | `uuid` | Nao | Gas agregado. |
| `total_kg_co2e` | `numeric(18,6)` | Sim | Total em kgCO2e. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `audit_logs`

Trilha de auditoria tecnica.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do evento. |
| `tenant_id` | `uuid` | Sim | Tenant onde ocorreu. |
| `actor_external_user_id` | `varchar(160)` | Nao | Usuario externo, quando existir. |
| `action` | `varchar(120)` | Sim | Acao executada. |
| `entity_name` | `varchar(120)` | Sim | Entidade afetada. |
| `entity_id` | `varchar(80)` | Nao | ID da entidade afetada. |
| `created_at` | `timestamptz` | Sim | Data do evento. |

## Fluxo esperado da API

1. A requisicao chega com `X-Tenant-Id` e, opcionalmente, `X-Api-Key`.
2. A API resolve o tenant.
3. A API grava ou consulta dados sempre filtrando por `tenant_id`.
4. O usuario envia entradas de atividade por categoria e unidade.
5. O motor busca o fator correto em `emission_factors`.
6. O calculo aplica `atividade * fator_kg_co2e_per_unit`.
7. A execucao e os resultados sao gravados em `calculation_runs` e `calculation_results`.

## Pendencias para proximas etapas

- Criar seeds iniciais para unidades, categorias, gases e fontes.
- Definir categorias prioritarias dos Escopos 1, 2 e 3.
- Definir fonte oficial inicial dos fatores de emissao.
- Implementar camada de contexto de tenant por header.
- Implementar API Key simplificada para ambiente de desenvolvimento.
- Conectar a API ao PostgreSQL via EF Core/Npgsql.
