fs-2025-assessment-1-74476

This project is an ASP.NET Core Web API that exposes two versions of Dublin Bikes station data.
Version 1 reads from a local JSON file with in-memory caching.
Version 2 reads and writes data to Azure Cosmos DB using the local Cosmos DB Emulator.

The API includes query filtering, sorting, paging, summary statistics, data import helpers, and matching unit tests.

Project Structure

The solution contains two projects:

fs-2025-assessment-1-74476
The main Web API application.

fs_2025_a_api_demo_002.Tests
Contains unit tests for the V1 in-memory service and the V2 Cosmos service.

Technologies Used

.NET 8 Minimal API

ASP.NET Core

In-memory caching (IMemoryCache)

Azure Cosmos DB Emulator

Query filtering, sorting, and paging

xUnit and Moq for unit testing

Version Overview
Version 1 (V1): JSON + Memory Cache

V1 loads the station dataset from Data/dublinbike.json through the BikeData loader.
The BikeQueryService applies:

Text filtering

Status filtering

Minimum bike counts

Sorting (ascending or descending)

Paging

Summary calculations

These endpoints return data without any database dependency.

Version 2 (V2): Cosmos DB

V2 connects to the Cosmos DB Emulator using CosmosBikeService.

Features:

Retrieves stations from the Cosmos container

Filtering, sorting, and paging applied in memory

Creates, updates, and retrieves individual stations

Provides summary statistics

Includes a development-only file import endpoint

The project includes custom client setup to allow the emulator’s self-signed certificate and local connection.

Cosmos DB Configuration

The application reads configuration values from appsettings.json:

"CosmosDb": {
  "AccountEndpoint": "https://localhost:8081/",
  "AccountKey": "YOUR_KEY",
  "DatabaseName": "DublinBikesDb",
  "ContainerName": "stations"
}


A custom CosmosClient configuration is used to allow:

The Cosmos Emulator’s HTTPS certificate

Gateway mode

Short request timeouts

Disabled retry loops for faster error detection

These settings ensure that Swagger, Postman, and the API can communicate reliably with the emulator.

Development Import to Cosmos DB

To populate the Cosmos DB container with all station records from Data/dublinbike.json, the project provides a helper class:

BikeCosmosImporter

This is exposed through a development-only endpoint:

POST /api/v2/stations/dev/import-from-file

This reads the JSON file, updates existing items if they exist, or inserts new ones.
The response includes:

{
  "message": "Import completed",
  "imported": X,
  "totalInFile": X
}


After importing, all V2 endpoints will operate on the database version of the dataset.

Endpoints Summary
Version 1 (JSON)
GET    /api/v1/stations
GET    /api/v1/stations/{number}
GET    /api/v1/stations/summary
POST   /api/v1/stations
PUT    /api/v1/stations/{number}

Version 2 (Cosmos DB)
GET    /api/v2/stations
GET    /api/v2/stations/{number}
GET    /api/v2/stations/summary
POST   /api/v2/stations
PUT    /api/v2/stations/{number}
POST   /api/v2/stations/dev/import-from-file   (development use)

Unit Tests

The Tests project includes two test classes:

BikeQueryServiceTests.cs (V1)

Tests:

Filtering by text, status, min bikes

Sorting logic

Paging

Summary calculation

CosmosBikeServiceTests.cs (V2)

Tests:

Reading and filtering logic

Summary calculation

GetByNumberAsync

CreateAsync and UpdateAsync (mocked Cosmos responses)

The tests do not require the emulator to run; Cosmos DB calls are mocked.

Running the Project
1. Start the Cosmos DB Emulator

Ensure Docker Desktop is running, then start the container:

docker start cosmos-emulator


The Data Explorer should be accessible at:

https://localhost:8081/_explorer/index.html

2. Run the API

Use Visual Studio or:

dotnet run


Swagger UI will be available at:

https://localhost:7259/swagger

3. (Optional) Import the full dataset into Cosmos

Execute in Swagger:

POST /api/v2/stations/dev/import-from-file

Postman Collection

The project was manually verified using a structured Postman collection including:

V1 basic functionality

V2 Cosmos reads and writes

Status checks

Summary validations

Update and creation operations

The tests confirm that both versions behave consistently.

Known Considerations

Cosmos Emulator sometimes takes several seconds to initialise after startup.

Cosmos Data Explorer may not immediately show newly-imported items; refreshing the container usually resolves it.

V2 sorting and filtering is performed in memory to match V1’s logic.

Conclusion

The project implements both required versions of the API, including:

JSON-based V1 with caching

Cosmos DB–based V2 with full read/write support

Development import utility
fs-2025-assessment-2-74476
Dublin Bikes Client Application (Blazor Server)

This project is a Blazor Server application that consumes the V2 Dublin Bikes API created in Assessment 1.
The client connects to the Cosmos DB–backed API and provides a full interface for browsing, filtering, and managing station data.

The UI loads station data from the API, supports search filters, provides paging, and allows creating, updating, and deleting stations.

Project Structure

Pages

Stations.razor – Main page that loads the data and contains all components.

Components

StationFilters.razor – Search box, status filter, and minimum-bikes filter.

StationList.razor – Visual list or table of stations.

StationListItem.razor – One station row with click selection.

StationDetail.razor – Detail panel showing station info and a form for editing or creating stations.

Services

StationsApiClient.cs – Wrapper around HttpClient for calling /api/v2/stations and handling all CRUD operations.

Features

Loads all stations from the API on startup.

Search filter (name or address).

Status filter (Open, Closed, All).

Minimum bikes filter.

Client-side paging.

Station selection and detail view.

Create, update, and delete operations through the V2 API.

Integrated Google Maps link for station coordinates.

API Integration

The client reads the API base URL from appsettings.json:

{
  "ApiBaseUrl": "https://localhost:7259/"
}


HttpClient is registered in Program.cs so all components can call the API through the StationsApiClient service.

How to Run

Start Docker Desktop.

Start the Cosmos DB Emulator container:

docker start cosmos-emulator


Run both projects in the solution:

The Web API (Assessment 1).

The Blazor client (Assessment 2).

Open the client in the browser:

https://localhost:7017/stations


The station list will load and the UI will be fully functional.

Summary

Assessment 2 delivers a working Blazor interface connected to the V2 Dublin Bikes API.
It implements filtering, paging, station details, and full CRUD operations using the same data stored in the Cosmos DB emulator.

Comprehensive unit testing suite

Complete Swagger and Postman validation
