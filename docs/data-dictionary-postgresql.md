# Dicionario de dados inicial - PostgreSQL

Data: 17/05/2026

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
| `source_name` | `varchar(240)` | Nao | Nome da fonte/registro apresentado ao usuario, como frota, unidade ou fatura. |
| `calculation_method` | `varchar(80)` | Sim | Metodo usado pelo motor: `factor` por padrao, `reported_total` para totais ja calculados por outra ferramenta. |
| `evidence_ref` | `text` | Nao | Referencia de evidencia ou documento. |
| `metadata` | `jsonb` | Sim | Campos especificos por categoria. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |
| `updated_at` | `timestamptz` | Nao | Ultima alteracao da entrada. |
| `deleted_at` | `timestamptz` | Nao | Exclusao logica; entradas removidas deixam de entrar nos calculos futuros. |

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

Resultados por entrada calculada. O dashboard agrega estes registros por escopo, categoria e periodo. Linhas antigas sem `activity_entry_id` continuam compativeis como resultados agregados legados.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do resultado. |
| `tenant_id` | `uuid` | Sim | Tenant proprietario. |
| `calculation_run_id` | `uuid` | Sim | Execucao associada. |
| `activity_entry_id` | `uuid` | Nao | Entrada que originou o resultado. |
| `scope` | `emission_scope` | Sim | Escopo agregado. |
| `category_id` | `uuid` | Nao | Categoria agregada. |
| `gas_id` | `uuid` | Nao | Gas agregado. |
| `emission_factor_id` | `uuid` | Nao | Fator de emissao usado, quando aplicavel. |
| `activity_unit_id` | `uuid` | Nao | Unidade da atividade usada no calculo. |
| `activity_value` | `numeric(18,6)` | Nao | Snapshot do valor de atividade calculado. |
| `factor_kg_co2e_per_unit` | `numeric(18,8)` | Nao | Snapshot do fator usado no calculo. |
| `total_kg_co2e` | `numeric(18,6)` | Sim | Total em kgCO2e. |
| `biogenic_kg_co2` | `numeric(18,6)` | Sim | CO2 biogenico reportado separadamente, em kg. |
| `biogenic_removal_kg_co2` | `numeric(18,6)` | Sim | Remocoes biogenicas reportadas separadamente, em kg. |
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
| `details` | `jsonb` | Sim | Snapshot minimo da acao para auditoria e rastreabilidade. |
| `created_at` | `timestamptz` | Sim | Data do evento. |

## Smart Survey

O Smart Survey e versionado como catalogo global. A ideia e o front-end ler perguntas, opcoes, regras de visibilidade e mapeamentos para gerar `activity_entries`, sem colocar regras do GHG Protocol dentro da interface.

### `survey_templates`

Cabecalho de um questionario versionado.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador do template. |
| `code` | `varchar(120)` | Sim | Codigo tecnico, como `gogreen_month_1_v1`. |
| `name` | `varchar(240)` | Sim | Nome legivel. |
| `version_label` | `varchar(80)` | Sim | Rotulo de versao. |
| `factor_set_id` | `uuid` | Nao | Conjunto de fatores recomendado. |
| `is_active` | `boolean` | Sim | Indica se pode ser usado pelo front-end. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `survey_sections`

Agrupa perguntas por modulo GHG, como combustao movel, fugitivas, mudanca no uso do solo e energia eletrica.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da secao. |
| `template_id` | `uuid` | Sim | Template proprietario. |
| `code` | `varchar(120)` | Sim | Codigo tecnico da secao. |
| `title` | `varchar(240)` | Sim | Titulo exibivel. |
| `description` | `text` | Nao | Descricao tecnica. |
| `sort_order` | `integer` | Sim | Ordem de exibicao. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `survey_questions`

Perguntas dinamicas com regras e mapeamentos em JSON.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da pergunta. |
| `section_id` | `uuid` | Sim | Secao proprietaria. |
| `code` | `varchar(160)` | Sim | Codigo tecnico da pergunta. |
| `prompt` | `varchar(500)` | Sim | Texto da pergunta. |
| `help_text` | `text` | Nao | Orientacao complementar. |
| `answer_type` | `varchar(40)` | Sim | Tipo: `boolean`, `decimal`, `select`, `multiselect`, `text`. |
| `is_required` | `boolean` | Sim | Obrigatoriedade. |
| `sort_order` | `integer` | Sim | Ordem na secao. |
| `visibility_rule` | `jsonb` | Sim | Regra para exibir a pergunta conforme respostas anteriores. |
| `mapping` | `jsonb` | Sim | Mapeamento para `activity_entries` ou `metadata`. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

### `survey_options`

Opcoes de perguntas de selecao.

| Coluna | Tipo | Obrigatorio | Descricao |
| --- | --- | --- | --- |
| `id` | `uuid` | Sim | Identificador da opcao. |
| `question_id` | `uuid` | Sim | Pergunta proprietaria. |
| `code` | `varchar(120)` | Sim | Codigo tecnico. |
| `label` | `varchar(240)` | Sim | Texto exibivel. |
| `value` | `varchar(240)` | Nao | Valor enviado pelo front-end. |
| `sort_order` | `integer` | Sim | Ordem de exibicao. |
| `created_at` | `timestamptz` | Sim | Data de criacao. |

## Fluxo esperado da API

1. A requisicao chega com `X-Tenant-Id` e, opcionalmente, `X-Api-Key`.
2. A API resolve o tenant.
3. A API grava ou consulta dados sempre filtrando por `tenant_id`.
4. O front pode ler `survey_templates` para montar perguntas dinamicas.
5. O usuario envia entradas de atividade por categoria, unidade e metodo de calculo.
6. O motor busca o fator correto em `emission_factors` ou aceita totais reportados em `reported_total`.
7. O calculo aplica `atividade * fator_kg_co2e_per_unit`, quando aplicavel.
8. A execucao e os resultados por entrada sao gravados em `calculation_runs` e `calculation_results`.
9. O dashboard agrega escopo, categoria, linha do tempo, CO2 biogenico e remocoes.

## Estado atual da implementacao

No estado atual do projeto:

- o backend ja usa EF Core/Npgsql;
- o `DbContext` ja existe;
- o contexto de tenant por header ja foi implementado;
- a API Key simplificada de desenvolvimento ja foi prevista;
- existem seeds tecnicos iniciais para ambiente local;
- o fluxo `POST /api/calculations/run` permanece compativel;
- o fluxo preferencial do mes 1 e `empresa -> inventario -> entradas -> calcular inventario -> dashboard -> auditoria`;
- entradas possuem CRUD proprio em `/api/inventories/{inventoryId}/entries`;
- o motor grava resultados detalhados por entrada e snapshots de fator;
- o Smart Survey inicial esta versionado em `/api/surveys/templates/gogreen_month_1_v1`.

Valores de desenvolvimento local:

- `X-Tenant-Id: 11111111-1111-1111-1111-111111111111`
- `X-Api-Key: gee_dev_local_2026`

## Pendencias para proximas etapas

- substituir os fatores de desenvolvimento por carga homologada da ferramenta GHG Protocol 2026;
- ampliar o Smart Survey ate cobrir todas as abas relevantes da ferramenta oficial;
- definir a primeira fonte oficial homologada de fatores de emissao;
- substituir a autenticacao temporaria pela integracao oficial da GoLedger quando disponivel;
- ampliar testes automatizados e cenarios de validacao;
- evoluir filtros de tenant e auditoria conforme a API ganhar novos endpoints.
