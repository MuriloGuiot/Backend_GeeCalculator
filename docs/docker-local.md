# Docker local

Este projeto esta preparado para rodar a API da Calculadora GEE e um PostgreSQL local via Docker Compose.

## Servicos

- `gee-calculator-api`: API ASP.NET Core exposta em `http://localhost:8080`.
- `gee-calculator-postgres`: PostgreSQL exposto em `localhost:5433`.

## Como subir

Na raiz do projeto, execute:

```powershell
docker compose up --build
```

Depois acesse:

```text
http://localhost:8080/swagger
```

## Como parar

```powershell
docker compose down
```

Para remover tambem os dados locais do PostgreSQL:

```powershell
docker compose down -v
```

## Observacoes

- A API tenta aplicar as migrations do EF Core no startup e usa bootstrap pelo modelo atual como fallback de desenvolvimento quando houver divergence de snapshot.
- A connection string ja aponta para o host interno `gee-calculator-postgres`.
- O script `database/postgresql/001_initial_schema.sql` permanece como referencia do dicionario inicial e bootstrap manual.
- Os campos de Keycloak e endpoint GoGreen ficam vazios ate recebermos os dados oficiais da GoLedger.

## Recriar o banco local

Para recriar o banco local do zero e repetir o bootstrap da API:

```powershell
docker compose down -v
docker compose up --build
```
