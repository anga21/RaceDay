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


