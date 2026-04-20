# Dia 01 - Escopo e arquitetura inicial

Data: 20/04/2026

## Objetivo da API

A API `GEE_Calculator` sera o backend da Calculadora GEE da AgrocarbonBR integrada a plataforma GoGreen. Sua responsabilidade e receber dados autenticados da GoGreen, persistir dados por empresa/tenant e calcular emissoes de gases de efeito estufa com base no GHG Protocol.

## Responsabilidades da AgrocarbonBR

- Desenvolver o motor de calculo GEE em .NET.
- Modelar as entidades de negocio necessarias para empresas, inventarios, atividades, fatores de emissao e resultados.
- Expor contratos HTTP documentados em OpenAPI/Swagger.
- Validar tokens Keycloak emitidos pela GoGreen.
- Preservar isolamento logico por `tenant_id`.
- Retornar resultados consolidados por escopo, periodo e unidade de emissao.

## Responsabilidades esperadas da GoLedger/GoGreen

- Autenticar usuarios B2B no Keycloak.
- Enviar JWT Bearer para a API da calculadora.
- Fornecer endpoint ou payload com dados do usuario logado.
- Desenvolver UI/UX, Smart Survey, dashboards e integracao visual.
- Provisionar infraestrutura cloud, quando aplicavel.

## Decisoes de stack

- Runtime: ASP.NET Core em .NET 10, conforme projeto atual.
- Banco alvo: PostgreSQL.
- ORM planejado: Entity Framework Core com provider Npgsql.
- Auth planejado: JWT Bearer validando issuer/audience/JWKS do Keycloak da GoGreen.
- Documentacao: OpenAPI exposta em desenvolvimento.
- Conteinerizacao: Dockerfile existente sera mantido e complementado com Docker Compose na proxima etapa.

## Modulos iniciais

- `Api`: controllers HTTP, contratos de request/response e configuracao da aplicacao.
- `Application`: servicos de aplicacao e regras de orquestracao.
- `Domain`: entidades, enums e objetos centrais da calculadora.
- `Infrastructure`: persistencia PostgreSQL, contexto EF Core, repositorios e seeds de fatores.

## Entidades iniciais

- `Tenant`: representa uma organizacao/empresa isolada logicamente.
- `Company`: dados cadastrais da empresa vinculada ao tenant.
- `ExternalUserIdentity`: identidade recebida da GoGreen/Keycloak.
- `EmissionInventory`: inventario mensal/anual de emissoes.
- `ActivityEntry`: dado de atividade informado pelo usuario.
- `EmissionFactor`: fator de emissao versionado.
- `CalculationRun`: execucao de calculo.
- `CalculationResult`: resultado agregado por escopo e periodo.
- `AuditLog`: trilha de auditoria para eventos sensiveis.

## Endpoints minimos do Dia 01

- `GET /api/health`: verifica se a API esta viva.
- `GET /api/auth/me`: retorna dados extraidos das claims recebidas no request.
- `POST /api/calculations/preview`: calcula uma previa sem persistencia usando a formula `atividade * fator * GWP`.

## Pendencias externas

- Confirmar URL, realm, issuer, audience, client ID e JWKS do Keycloak.
- Confirmar nomes das claims de `tenant_id`, empresa, CNPJ, roles e usuario.
- Confirmar se a GoGreen enviara dados completos via JWT ou por endpoint de usuario logado.
- Confirmar fonte oficial inicial dos fatores de emissao.
- Confirmar regra exata para comparacao com limites/reguas legais brasileiras.
