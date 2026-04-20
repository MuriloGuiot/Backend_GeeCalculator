# Docker local

Este projeto esta preparado para rodar a API da Calculadora GEE e um PostgreSQL local via Docker Compose.

## Servicos

- `gee-calculator-api`: API ASP.NET Core exposta em `http://localhost:8080`.
- `gee-calculator-postgres`: PostgreSQL exposto em `localhost:5432`.

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

- O banco sobe no Compose, mas a API ainda nao usa EF Core nem migrations.
- A connection string ja aponta para o host interno `gee-calculator-postgres`.
- Os campos de Keycloak e endpoint GoGreen ficam vazios ate recebermos os dados oficiais da GoLedger.
