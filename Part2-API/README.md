RaceDay API (Part 2)
About the System
The RaceDay API is the ASP.NET Core Web API (C#, .NET 8) behind the RaceDay event management system for South African road running, walking and cycling events. It handles registration, login, events, categories, enrolments and results, and it enforces role-based access. It uses Entity Framework Core (code-first) with SQL Server and is documented and testable through Swagger.
Roles
Organiser - can create, update and delete their own events, define categories for their events, view all enrolments for their events, and capture finish times and positions.
Participant - can browse events and categories, enrol in an event by selecting a category, view their own enrolments, and view their own results.
Both roles can register, log in, and view and update their own profile. Organiser functions are blocked for Participants (403) and the other way round, and logged-out users get 401.
How Authentication Works
Login creates a cookie session (`RaceDay.Session`) that stores the user's id and role. Passwords are hashed with the ASP.NET Core password hasher and never stored as plain text. Every endpoint needs a logged-in user, and role rules are enforced with `[Authorize(Roles = ...)]` plus ownership checks, so an organiser can only change their own events.
Setup and Run Instructions
Requirements: Visual Studio 2022 with the ASP.NET workload, the .NET 8 SDK, and SQL Server LocalDB (installed with Visual Studio) or SQL Server.
Clone the repository: `git clone https://github.com/ST10480499/RaceDay-POE.git`
Open `Part2-API/RaceDay/RaceDay.sln` in Visual Studio.
Check the connection string in `RaceDay.Api/appsettings.json`. It defaults to `(localdb)\MSSQLLocalDB` and a database named `RaceDayApiDb`.
Open View > Other Windows > Package Manager Console, set the default project to `RaceDay.Api`, and run `Update-Database`. This creates the database and tables.
Select the `https` profile and press F5. Swagger opens at `https://localhost:7150/swagger`.
In Swagger, register an Organiser and a Participant with `POST /api/auth/register`, then log in with `POST /api/auth/login`. Swagger keeps the session cookie, so later calls are authenticated.
Running the Unit Tests
In Visual Studio use Test > Run All Tests, or run `dotnet test Part2-API/RaceDay/RaceDay.sln`. The tests start the API in memory and cover registration and login, event management, role rejection, and enrolments.
Endpoints


Method
Route
Who can use it
POST
/api/auth/register
Anyone
POST
/api/auth/login
Anyone
POST
/api/auth/logout
Logged in
GET
/api/users/me
Logged in
PUT
/api/users/me
Logged in
GET
/api/events
Logged in
GET
/api/events/{id}
Logged in
POST
/api/events
Organiser
PUT
/api/events/{id}
Organiser (owner)
DELETE
/api/events/{id}
Organiser (owner)
GET
/api/events/{eventId}/categories
Logged in
POST
/api/events/{eventId}/categories
Organiser (owner)
PUT
/api/categories/{id}
Organiser (owner)
DELETE
/api/categories/{id}
Organiser (owner)
POST
/api/categories/{id}/enrol
Participant
GET
/api/users/me/enrolments
Participant
GET
/api/events/{eventId}/enrolments
Organiser (owner)
POST
/api/enrolments/{id}/result
Organiser (owner)
GET
/api/users/me/results
Participant
GET
/api/events/{eventId}/results
Logged in
Changes From the Part 1 Plan
Login creates a cookie session instead of returning a token, because the Part 2 brief asks for session management.
A logout endpoint was added so a session can end.
Viewing events, categories and results needs a login. The Part 1 plan listed these as public.
Events store a description, distance and event type (run, walk or cycle), as the Part 2 requirements ask. These three columns are added to the SQL script in `docs`.
CI/CD
The workflow `.github/workflows/build-and-test.yml` builds the solution and runs all unit tests on every push.
Screenshot of a successful build:
(drag the CI screenshot here)
Video
Unlisted YouTube walkthrough of the running API, code structure, endpoints, authentication and unit tests:
(paste the YouTube link here)
