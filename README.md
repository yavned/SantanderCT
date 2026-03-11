# HackerNews Best Stories API

 ASP.NET Core RESTful API to retrieve the details of the best N stories from the Hacker News API.

## Run

dotnet run

API endpoint:
GET /api/stories/best?n=10

Example:
https://localhost:7101/api/stories/best?n=12

## Design

- Uses HttpClientFactory for efficient HTTP usage
- Uses Caching to avoid excessive calls to HackerNews
- Fetches stories in parallel
- Set limits fetch to 30 stories just for performance

## Possible improvements

- Background periodical cache refresh
- Pagination
- Polly retry policies
- Circuit breaker
- Logging and metrics
- Distributed cache (Redis)
- Unit tests with Mock of HackerNews API 
- Integration tests to test endpoints and caching