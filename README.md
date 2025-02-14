# Expression Calculator

Evaluates expressions in background tasks

#### Repository structure:
1. `.github` - contains GitHub Actions workflows for CI/CD
2. `docs` - project documentation
3. `src` - source code of the project
4. `CHANGELOG.md` - descriptions of changes and version history


#### Project structure (inside `src` folder):
1. `Common`:
   - `ExprCalc.Entities` - shared domain entities
2. `CoreLogic` - business logic:
   - `ExprCalc.CoreLogic` - implementation of the business logic (use-cases for requests processing, status checks and so on)
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