# Expression Calculator
[![backend](https://github.com/ikjob/expr-calc/actions/workflows/backend.yaml/badge.svg)](https://github.com/ikjob/expr-calc/actions/workflows/backend.yaml)
[![frontend](https://github.com/ikjob/expr-calc/actions/workflows/frontend.yaml/badge.svg)](https://github.com/ikjob/expr-calc/actions/workflows/frontend.yaml)
[![codecov](https://codecov.io/github/ikjob/expr-calc/graph/badge.svg?token=6UM2YH20NZ)](https://codecov.io/github/ikjob/expr-calc)

Evaluates expressions in background tasks

#### How to start

```
cd ./docker/
docker compose up
```
Web interface will be available at http://127.0.0.1:8123


#### Repository structure:
1. `.github` - contains GitHub Actions workflows for CI/CD
2. `docs` - project documentation
3. `src` - source code of the project
   - `src/backend` - source code of backend
   - `src/frontend` - source code of frontend
4. `CHANGELOG.md` - descriptions of changes and version history


#### Backend project structure (inside `src/backend` folder):
1. `Common`:
   - `ExprCalc.Entities` - shared domain entities
   - `ExprCalc.Common` - common types shared between all projects
2. `CoreLogic` - business logic:
   - `ExprCalc.CoreLogic` - implementation of the business logic (use-cases for requests processing, status checks and so on)
   - `ExprCalc.CoreLogic.Tests` - tests for `ExprCalc.CoreLogic`
   - `ExprCalc.CoreLogic.Api` - api to access use-cases of business logic (interfaces, types, exceptions)
   - `ExprCalc.ExpressionParsing` - core code for expression parsing and calculation
   - `ExprCalc.ExpressionParsing.Tests` - tests for `ExprCalc.ExpressionParsing`
3. `RestApi`:
   - `ExprCalc.RestApi` - Rest API implementation
4. `Storage` - component that responsible for storing of the requests history:
   - `ExprCalc.Storage` - storage subsystem implementation
   - `ExprCalc.Storage.Api` - storage api interfaces, types, exceptions)
5. `Tests` - common tests for the system
6. `ExprCalc` - entry project, produces main executable file


All dependencies between subsystems go through the Api project, making them loosely coupled. The project structure follows the general principles of Clean Architecture: entities and business logic are at the core of the system and remain independent
