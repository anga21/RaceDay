# RaceDay
Web based event management system for South Arican road running ,walking and cycling community .Designed to manage events ,track personal performance history and prepare race day .

*PART 1 - SYSTEM PLANNING AND DATABASE 

The system way planned and documented before any application code was written 

#USER ROLES

The system supports two distinct user roles 

-Organizers -Can create ,edit delete and manage events ,categories ,capture participants results and view all event enrolment 

-Participant -Create account,vrowse events ,enter events and view their enrolment and track their personal results 

# REPOSIORY STRUCTURE 

RaceDay /
    README .md 
    docs/
        RaceDay API ENDPOINT.pdf/ 
        RaceDay ERD .pdf/
        RaceDay_Schema.sql 


#what's in/doc 
- ERD — six  entities (User, Event, Category, Participant_Event, Route, Weather_Forecast) ,
  
- API Endpoint  — a full table of planned endpoints covering Authentication, User Profile, Events, Categories, Event Enrolments and Results
  
- SQL Script— creates the full database schema in SQL Server (SSMS), matching the ERD exactly and seeds it with sample Organisers, Participants, Events, Categories and Enrolments 

# YouTube VIDEO PRESENTATION #
https://youtu.be/nJDxrmpHsGQ

# RaceDay: Part 2 (REST API)

RaceDay is a web-based event management system for the South African road running, walking and cycling community. This folder contains Part 2: the ASP.NET Core Web API that powers the whole system. It implements the endpoint plan produced in Part 1 with role-based access, Swagger documentation, unit tests and a CI/CD pipeline.

## The Two Roles

| Role | What they can do |
|---|---|
| **Organiser** | Create, update, and delete their own events; define categories for their events; view all enrolments for their events; capture finish times and positions. |
| **Participant** | Register an account; browse events and categories; enter an event by selecting a category; view their own enrolments; view their own results. |

Organiser-only endpoints return `403 Forbidden` to Participants, and Participant-only endpoints return `403 Forbidden` to Organisers. Unauthenticated requests to protected endpoints return `401 Unauthorized`.

## Tech Stack

- ASP.NET Core Web API (C#), _[.NET version]_
- Entity Framework Core (Code-First) with SQL Server
- Cookie-based session authentication with role claims
- Swagger UI for endpoint documentation and testing
- xUnit for unit tests
- GitHub Actions for CI/CD

## Repository Structure

```
RaceDay/
├── README.md
├── docs/                      
└── Part2/
    ├── README.md              
    ├── RaceDay.Api/           
    │   ├── Controllers/
categories , Enrolment, Results, Routes, Weather
    │   ├── Models/            
    │   ├── Data/              
    │   ├── Dtos/              
    │   ├── Migrations/        
    │   └── Program.cs
    ├── RaceDay.Tests/         
    └── .github/workflows/    
```
### Steps

1.	Clone the repository and open the solution in Visual Studio

2.	Open RaceDay.2 appsettings.json and check the connection string. Change it only if your SQL Server instance is different
3.	Open Tools, then NuGet Package Manager, then Package Manager Console. Set the Default project to RaceDay.2API and run the command below. It creates the RaceDay_Part2 database from the EF Core migrations:
Update-Database

4.	Run the API with the green run button. Swagger UI opens in the browser at /swagger.
5.	The database starts empty. Register accounts through Swagger: anyone can register as a Participant, and an Organiser needs the organiser code RACEDAY-ORG-2026 ,set as OrganiserRegistrationCode in appsettings.json.
6.	Log in with POST /api/auth/login. Swagger keeps the session cookie, so the protected endpoints work straight away. Log out before switching accounts.


## Unit Tests

Tests cover both success and failure scenarios for:

- User registration and login
- Event management by Organisers (create, update, delete)
- Role enforcement, where the wrong role is rejected
- Event enrolments, where Participants can enrol and the enrolment is recorded correctly

# YouTube VIDEO #
https://youtu.be/5xc_kSrUqgk

*PART 3 - POE 

## Features

### Authentication and navigation
- Register and log in through the MVC app, which communicates with the Part 2 API.
- Authenticated state and role are kept across pages (server-side session plus cookie authentication).
- Distinct navigation for each role; protected pages redirect unauthenticated users to the login page.
- Users who access a page they are not allowed to see are shown an Access Denied page.
- Log out ends the session and redirects to the login page.

### Organiser
- **Dashboard:** summary of the organiser's events, total enrolments per event and upcoming event dates.
- **My Events:** create, edit and delete events (name, description, date, location, distance and event type), with an event banner image upload.
- **Categories:** add age or distance categories to each event.
- **Enrolments:** view all participants enrolled in an event, with their category and enrolment status.
- **Results entry:** capture and publish finish times and finishing positions after an event concludes.

### Participant
- **Home:** upcoming events with filtering options.
- **Event details:** full event information, categories, routes and weather forecast.
- **Enrol:** enrol in an event and choose a category; the enrolment is recorded immediately.
- **My Enrolments:** personal list of enrolled events with colour-coded status (green = Confirmed, amber = Pending).
- **My Results:** personal race history showing event name, date, category, finish time and position (for example, 47th out of 312 finishers).
- **Profile:** view and update personal information and upload a profile picture.

### UI and quality
- Sporty RaceDay theme (navy and orange) with consistent fonts, colours and spacing across all pages.
- Responsive layout built with Bootstrap; the menu collapses on small screens.
- Friendly error handling: invalid input and API failures are shown as clear messages, never raw errors.
- The MVC project contains **no database access and no duplicated business logic**; all data operations go through the API.

---

## Tech Stack

| Area | Technology |
|---|---|
| Web front end | ASP.NET Core MVC (.NET) |
| API | RaceDay.2API (ASP.NET Core Web API, Part 2) |
| Styling | Bootstrap, custom CSS, Google Fonts (Barlow Condensed, Inter) |
| Authentication | Cookie authentication and server-side session; API login |
| File storage | Azure Blob Storage (through the API) |
| Containers | Docker |
| CI/CD | GitHub Actions |

---

## Project Structure

```
/
├── RaceDay.2API/          # Part 2 – REST API (with Azure Blob Storage added in Part 3)
├── RaceDay.Web/           # Part 3 – ASP.NET Core MVC application
│   ├── Controllers/
│   ├── Models/
│   ├── Services/          # API client used for all API calls
│   ├── Views/
│   ├── wwwroot/
│   └── Dockerfile
├── .github/workflows/     # GitHub Actions workflow
└── README.md
```

>
