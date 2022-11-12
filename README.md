# UrlShortner
A simple URL shortner service written in .NET

## Installation instructions
- Clone the repository
- Run `docker compose up` in the root directory
- The application will be available at `http://localhost:8080`

## Tests
Unit & Integration tests are written using xUnit and are contained in the `src/UrlShortner.Tests` project.

Smoke tests written in [k6](https://k6.io/) found in `src/UrlShortner.Tests/SmokeTests`

### Running Unit and itegration tests:
`docker compose -f docker-compose.tests.yml up`

Note: can also be run in Visual Studio

## Smoke tests
Install the k6 docker image using `docker pull grafana/k6`

Have the application running using `docker compose up`

Then run k6 load test:
- Windows (Powershell): `cat .\src\UrlShortner.Tests\SmokeTests\k6_smoke_test.js | docker run --rm -i grafana/k6 run -`
- Linux/Mac: `docker run -rm -i grafana/k6 run - < ./src/UrlShortner.Tests/SmokeTests/k6_smoke_test.js`

## Architectural discussions
Architecture discussions can be found in `docs/adr`. These docs follow a very basic Architectural Decision Record format, essentially aiming
to record consequential decisions about the architecture of the application within the codebase itself and committed to source control.