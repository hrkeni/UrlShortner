# Basic stack and reasoning

The stack for this application includes
- A web service written in .NET 7.0
  - This service is responsible for shortening the URL and storing it in a database
  - This service aims to follow 12 factor app principles
- A Postgres database
- A unit and integration testing suite also written in .NET
- A docker-compose file to run the application locally
  - docker-compose allows us to simplify the whole cycle of building and running the app and ensuring dependencies across environments
