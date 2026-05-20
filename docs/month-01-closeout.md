# Fechamento do mes 1 - GoGreen API

Data: 17/05/2026

Este fechamento consolida a primeira entrega operacional da API GoGreen/GEE Calculator. O objetivo do mes 1 e deixar o ciclo principal completo, auditavel e pronto para receber o Smart Survey sem acoplar o motor de calculo a uma interface especifica.

## Fluxo entregue

Fluxo preferencial:

```text
empresa/CNPJ -> inventario -> entradas -> calculo -> dashboard -> auditoria
```

Endpoints principais:

- `POST /api/companies`
- `POST /api/inventories`
- `GET /api/inventories`
- `GET /api/inventories/{inventoryId}`
- `GET /api/inventories/{inventoryId}/entries`
- `POST /api/inventories/{inventoryId}/entries`
- `PUT /api/inventories/{inventoryId}/entries/{entryId}`
- `DELETE /api/inventories/{inventoryId}/entries/{entryId}`
- `POST /api/inventories/{inventoryId}/calculate`
- `GET /api/reports/emissions-dashboard`
- `GET /api/surveys/templates`
- `GET /api/surveys/templates/gogreen_month_1_v1`

O endpoint legado `POST /api/calculations/run` permanece compativel, mas agora cria entradas persistidas e calcula o inventario usando o mesmo motor do fluxo novo.

## Decisoes tecnicas

- Entradas de atividade agora sao recursos persistidos e editaveis antes do calculo.
- Exclusao de entrada e logica (`deleted_at`), mantendo historico para auditoria.
- Calculo roda sobre entradas ativas de um inventario.
- Resultados sao gravados por entrada, com snapshot de fator, valor de atividade, unidade e categoria.
- Dashboard agrega resultados por escopo, categoria e linha do tempo.
- CO2 biogenico e remocoes biogenicas sao guardados separadamente, alinhado ao resumo da ferramenta GHG.
- `calculation_method = factor` cobre o caso padrao `atividade * fator`.
- `calculation_method = reported_total` permite receber totais vindos de outra ferramenta ou modulo complexo da planilha.

## Smart Survey

Foi criada a fundacao versionada do Smart Survey:

- `survey_templates`
- `survey_sections`
- `survey_questions`
- `survey_options`

O template inicial `gogreen_month_1_v1` cobre a primeira trilha de perguntas para:

- combustao movel;
- emissoes fugitivas/RAC;
- mudanca no uso do solo;
- energia eletrica SIN;
- triagem de categorias de Escopo 3.

Cada pergunta pode trazer:

- tipo de resposta;
- regra de visibilidade;
- mapeamento para `activity_entries`;
- mapeamento para `metadata`.

## Referencias dos materiais GHG

Os materiais fornecidos indicam que:

- inventario completo deve cobrir Escopos 1 e 2;
- Escopo 3 e opcional, mas deve ser preparado como trilha evolutiva;
- a ferramenta GHG 2026 opera como uma matriz de modulos, fatores, listas, formulas e resumo;
- os cenarios de gabarito devem virar testes de aceite por modulo.

Foram preparados seeds para os primeiros casos de aceite:

- energia eletrica SIN em `MWh`;
- refrigerantes `R-404A`, `R-407C` e `HFC-134a`;
- mudanca no uso do solo reportada como CH4 em `tCO2e`;
- metadados para CO2 biogenico e remocoes biogenicas.

## Proximos passos naturais

- Carga homologada dos fatores oficiais da ferramenta GHG Protocol 2026.
- Conversao dos cenarios 3, 5 e 6 em testes integrados de aceite com payloads reais.
- Endpoint para transformar respostas do Smart Survey em entradas automaticamente.
- Expansao do template para todas as abas relevantes dos Escopos 1, 2 e 3.
