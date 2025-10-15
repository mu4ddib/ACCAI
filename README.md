# ACCAI — Clean Architecture .NET 8 (Minimal APIs + CQRS + EF/Dapper)
Proyectos: ACCAI.Domain, ACCAI.Application, ACCAI.Infrastructure, ACCAI.Api

## Pasos
cd src
dotnet new sln -n ACCAI
dotnet sln add **/*.csproj
dotnet restore && dotnet build
dotnet tool install --global dotnet-ef
dotnet ef migrations add Initial -p ACCAI.Infrastructure -s ACCAI.Api
dotnet ef database update -p ACCAI.Infrastructure -s ACCAI.Api
dotnet run --project ACCAI.Api
