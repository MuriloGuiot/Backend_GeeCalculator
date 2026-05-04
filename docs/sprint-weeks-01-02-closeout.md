# Fechamento da sprint 1-2

Data: 29/04/2026

Este documento resume o que ficou concluido na sprint inicial de duas semanas da API `GEE_Calculator`.

## Objetivo da sprint

Construir a fundacao tecnica da calculadora GEE em .NET, com foco em:

- backend conteinerizado;
- banco PostgreSQL local;
- multi-tenancy por `tenant_id`;
- estrutura inicial do dominio;
- estrategia de autenticacao temporaria e plugavel;
- primeiro fluxo funcional de calculo persistido.

## Entregas concluidas

### 1. Estrutura inicial da API .NET

- API ASP.NET Core criada e compilando com sucesso.
- Swagger/OpenAPI habilitado.
- Endpoints iniciais disponiveis:
  - `GET /api/health`
  - `GET /api/auth/me`
  - `POST /api/calculations/preview`
  - `POST /api/calculations/run`

### 2. Docker e ambiente local

- `Dockerfile` da API criado.
- `docker-compose.yml` configurado para API + PostgreSQL.
- PostgreSQL exposto localmente em `127.0.0.1:5433`.
- Ambiente local validado para uso com DBeaver e Swagger.

### 3. Banco PostgreSQL e dicionario de dados

- Dicionario de dados inicial documentado.
- Script SQL inicial versionado em `database/postgresql/001_initial_schema.sql`.
- Modelo de dados preparado para:
  - `tenant_id`
  - empresas
  - inventarios
  - entradas de atividade
  - fatores de emissao
  - execucoes de calculo
  - resultados
  - auditoria

### 4. Multi-tenancy e autenticacao temporaria

- Header `X-Tenant-Id` exigido para endpoints operacionais.
- Header `X-Api-Key` suportado como autenticacao simples temporaria.
- API Key pode resolver o tenant automaticamente ou validar consistencia com o header.
- Estrutura mantida plugavel para futura integracao com Keycloak/JWT sem refatoracao do core.

### 5. Modelagem core da calculadora

- Catalogos tecnicos estruturados para evitar hardcode:
  - `activity_units`
  - `emission_categories`
  - `greenhouse_gases`
  - `emission_factor_sources`
  - `emission_factor_sets`
  - `emission_factors`
- Estrutura preparada para expansao dos Escopos 1, 2 e 3.
- `tenant_id` mantido nas tabelas operacionais para isolamento de dados.

### 6. Fluxo persistido de calculo v0

O endpoint `POST /api/calculations/run` agora:

1. resolve o tenant atual;
2. garante tenant e empresa;
3. cria inventario;
4. grava entradas de atividade;
5. busca fator de emissao por categoria/unidade/conjunto;
6. calcula totais por escopo;
7. grava `calculation_runs`;
8. grava `calculation_results`;
9. registra `audit_logs`.

### 7. Seeds tecnicos de desenvolvimento

Foram adicionados seeds iniciais para ambiente local:

- unidades: `kWh`, `L`, `km`
- categorias: `energia_eletrica_sin`, `diesel_rodoviario`, `viagem_aerea`
- gas: `CO2E`
- fonte de fatores: `agrocarbonbr_dev_seed`
- conjunto de fatores: `dev_seed_2026`
- API Key de desenvolvimento

## Credenciais de desenvolvimento local

Uso exclusivo para desenvolvimento local:

- `X-Tenant-Id: 11111111-1111-1111-1111-111111111111`
- `X-Api-Key: gee_dev_local_2026`

## Validacoes realizadas

- build do projeto principal com sucesso;
- criacao da base EF Core/Npgsql;
- criacao do `DbContext`;
- inicializacao automatica do banco pelo modelo atual;
- endpoint preview funcionando;
- endpoint persistido de calculo implementado;
- seeds locais preparados;
- repositório limpo com `.gitignore`.

## Pendencias residuais

Itens que ainda podem evoluir, mas nao bloqueiam o encerramento desta sprint:

- ampliar cobertura de testes automatizados;
- expandir seeds para mais categorias e fatores oficiais;
- revisar carga oficial de fatores do GHG Protocol/SIN/MCTI;
- aprofundar filtros globais de tenant;
- substituir autenticacao temporaria por integracao oficial quando a GoLedger consolidar a infraestrutura alvo.
