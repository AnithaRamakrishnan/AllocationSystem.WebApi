# AllocationSystemApp

**AllocationSystem app** - The backend app for AllocationSystem-UI app.

> ** WARNING: Before starting development.**  
>
> **This is an .Net 6 Application**. Please install **VS 2022 Preview** for development.
>
> **Note**: _Database creation is not required. But it is recommended that to create an empty database before migration._
>
> Set **AllocationSystem.WebApi** as startup project and configure connection strings.
>
> Run the following Database Update Commands (_**in Package Manager Console**_) to migrate the databases
>
> **Update-Database**

## To run the app

* Restore the **NuGet** packages and **Compile**
* Configure the **ConnectionStrings**. 
* Set **AllocationSystem.WebApi** as start-up in Solution properties.
* **AllocationSystem.WebApi IIS Express** runs on <https://localhost:44366/swagger>
* **AllocationSystem.WebApi App** runs on <https://localhost:5001/swagger> (run as a self-hosted app)

## App

* AllocationSystem.WebApi - NetCore WebAPI application

## Tests

* Unit Tests - Tests for API endpoints (uses in-memory database)