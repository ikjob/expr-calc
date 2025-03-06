# Expression Calculator
[![backend](https://github.com/ikjob/expr-calc/actions/workflows/backend.yaml/badge.svg)](https://github.com/ikjob/expr-calc/actions/workflows/backend.yaml)
[![frontend](https://github.com/ikjob/expr-calc/actions/workflows/frontend.yaml/badge.svg)](https://github.com/ikjob/expr-calc/actions/workflows/frontend.yaml)
[![codecov](https://codecov.io/github/ikjob/expr-calc/graph/badge.svg?token=6UM2YH20NZ)](https://codecov.io/github/ikjob/expr-calc)

Evaluates expressions in background tasks

### Features
1. The backend is written in .NET 9 and uses ASP.NET as the web framework
2. The frontend is written in React 19 and built as an SPA to run in a web browser
3. The backend is cross-platform and can run on both Windows and Linux
4. The backend can receive mathematical expressions for calculation from multiple clients and process them simultaneously
5. The backend stores the history of all submitted calculations in an SQLite database. 
6. The storage period for calculations is limited and can be configured in the config file (parameter `CoreLogic -> StorageCleanupExpiration`, set to 1 day by default). If a calculation is older than the specified value, it will be automatically removed from the database during the cleanup procedure
7. The backend restores its state after a restart. All unfinished calculations will be read from the database and processed again
8. The number of simultaneously processed expressions can be configured in the config file (parameter `CoreLogic -> CalculationProcessorsCount`, which defaults to the number of CPU cores)
9. The backend also stores each new expression for processing in a registry. This registry has a limited size, which can be configured in the config file (parameter `CoreLogic -> MaxRegisteredCalculationsCount`)
10. Expression calculation units take expressions from the registry. The registry makes expressions available for calculation only after a small random delay. This delay is generated between two values specified in the config file: `CoreLogic -> MinCalculationAvailabilityDelay` (0s by default) and `CoreLogic -> MaxCalculationAvailabilityDelay` (15s by default)
11. Supported arithmetic operations are: unary `+`, unary `-`, `+`, `-`, `*`, `/`, `^` (power), and `ln(x)` (logarithm)
12. Each arithmetic operation is performed within a strictly defined time. This times can be configured in the config file (`CoreLogic -> OperationsTime`). By default: unary operations run for 0s, `+` and `-` run for 1s, `*` and `/` run for 2s, `^` runs for 3s, and `ln(x)` runs for 4s
13. The frontend allows users to specify their username
14. The frontend allows users to submit new expressions to the backend
15. The frontend monitors the status of requests and automatically updates the UI when the statuses change on the backend. Supported statuses: Pending, InProgress, Success, Failed, Cancelled
16. The frontend displays the calculation results. Possible results: for the Success status, it is a number; for Failed, it is an error message; for Cancelled, it includes information on who canceled the calculation
17. The frontend allows users to view the history of all requests from the server, displayed as a paginated table
18. The frontend allows users to apply filters to view specific calculations. Currently supported frontend filters: by the current user and by status. The backend supports additional filters: by creation time, update time, username, and substring in the expression
19. The request history table on frontend is automatically updated if a new calculation appears on the server and matches the client's active filters


### Quick start

```
cd ./docker/
docker compose up
```
Web interface will be available at http://127.0.0.1:8123


### Configuration

Application configuration is stored in `appsettings.json` file (you can find one in `./src/backend/ExprCalc/appsettings.json`). Most important parameters are:
1. `Kestrel -> Endpoints -> Http -> Url : "http://0.0.0.0:8123"` - url on which web-server will listen for incomming connections 
2. `RestAPI -> CorsAllowAny : false` - allows cross origin requests. Required when frontend hosted in separate web server
3. `CoreLogic -> CalculationProcessorsCount: -1` - number of execution units that calculate expressions. '-1' means it will be equal to the number of CPU cores
4. `CoreLogic -> MaxRegisteredCalculationsCount: 20000` - max number of registered calculations (pending or in progress ones). New ones will be rejected on overflow
5. `CoreLogic -> StorageCleanupExpiration: "1.00:00:00"` - priodical cleanup job will remove calculations that are older than specified amount of time
6. `CoreLogic -> MinCalculationAvailabilityDelay: "00:00:00"` - min delay before the calculation can be taken for processing after it was submitted. Actual delay sets randomly between 'MinCalculationAvailabilityDelay' and 'MaxCalculationAvailabilityDelay'
7. `CoreLogic -> MaxCalculationAvailabilityDelay: "00:00:15"` - max delay before the calculation can be taken for processing after it was submitted. Actual delay sets randomly between 'MinCalculationAvailabilityDelay' and 'MaxCalculationAvailabilityDelay'
8. `CoreLogic -> OperationsTime` - map of delays for every math operation (supported operations: `+`, `-`, `/`, `*`, `^`, `ln(x)`)
9. `Storage -> DatabaseDirectory: "./db/"` - path to the directory where the database file will be created

### Run from the source code
#### Running backend
Make sure that .NET 9 SDK is installed.

Make sure that `RestAPI -> CorsAllowAny` is set to `true` in the config (`./src/backend/ExprCalc/appsettings.Development.json`) before starting. This is true by default for the development config. Then run the script:
```
cd ./src/backend/ExprCalc/
dotnet restore
dotnet run
```

#### Running fontend
Make sure that `Node.js` version 22 or above installed. Make sure that `yarn` is installed.

Create `./src/frontend/.env.local` file with following content:
```
VITE_BACKEND_URL="http://127.0.0.1:8123"
```
That parameter specifies the backend REST API URL.

Then run the script:
```
cd ./src/frontend/
yarn install
yarn dev
```

Open URL it have printed in Web-browser.


### Repository structure:
1. `.github` - contains GitHub Actions workflows for CI/CD
2. `docs` - project documentation
3. `docker` - contains a Dockerfile and a docker-compose file to build and run the application inside containers
4. `src` - source code of the project
   - `src/backend` - source code of backend
   - `src/frontend` - source code of frontend
5. `CHANGELOG.md` - descriptions of changes and version history


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
   - `ExprCalc.Storage.Tests` - tests for `ExprCalc.Storage`
5. `Tests` - common tests for the system:
   - `ExprCalc.IntegrationTests` - integration tests (this is a placeholder; there are no tests at the moment)
6. `ExprCalc` - entry project, produces main executable file


All dependencies between subsystems go through the Api project, making them loosely coupled. The project structure follows the general principles of Clean Architecture: entities and business logic are at the core of the system and remain independent


#### Frontend project structure (inside `src/frontend` folder):
`app` - main code of the application:
   1. `api` - contains REST API client implementation
   2. `components` - application specific React components
   3. `models` - common frontend entities
   4. `pages` - contains pages of the app (every page have distinct route) and common layout
   5. `redux` - React Redux stores and hooks
   6. `styles` - css with tailwind configuration
   7. `App.tsx` - main component of the application
   8. `main.tsx` - entry point for the React application
