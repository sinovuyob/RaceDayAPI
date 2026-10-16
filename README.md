# RaceDay - Event Management System

## Project Overview

RaceDay is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. The platform streamlines the entire event lifecycle - from creation and registration to results management - replacing outdated paper-based processes with a modern, digital solution.

*Key Features:*
- Event creation and management
- Participant registration and enrolment
- Results tracking and leaderboards
- Live weather information
- Route mapping and details
- Role-based access controL

## User Roles

The system supports two distinct user roles:

 1. Organiser
- Create, edit, and delete events
- Manage event categories (distances, fees, age groups)
- Capture and verify participant results
- View all event enrolments and statistics
- Generate reports and download results
- Manage weather and route information

 2. Participant
- Create an account and manage profile
- Browse and search for upcoming events
- Enrol in events by selecting categories
- Track personal enrolment history
- View personal results and performance
- Access weather and route information for race day preparation

Mark down
## Build Status

![Validation Status](https://github.com/sinovuyob/raceday/actions/workflows/validate-docs.yml/badge.svg)

### CI Workflow
<img width="1427" height="587" alt="image" src="https://github.com/user-attachments/assets/5f671843-0990-4a2e-b59a-922085be7cd2" />
<img width="1422" height="781" alt="image" src="https://github.com/user-attachments/assets/6255fc4e-4330-4997-9df7-fa675314b249" />
<img width="1417" height="872" alt="image" src="https://github.com/user-attachments/assets/123da71e-7a68-435d-b60d-d024c93db306" />
<img width="1422" height="682" alt="image" src="https://github.com/user-attachments/assets/68fef38c-10cf-4939-a7a1-3c59bbd46c4a" />


## Part 2 - RESTful API Development

This part implements the RaceDay API using ASP.NET Core Web API, Entity Framework Core, and JWT authentication.

### Projects
- `RaceDay.API` - Web API project
- `RaceDay.Core` - Domain entities
- `RaceDay.Infrastructure` - EF Core DbContext and migrations
- `RaceDay.Tests` - Unit tests

### Build and Run
1. Open `RaceDay.sln` in Visual Studio 2022
2. Ensure LocalDB is available (or update `appsettings.json`)
3. Run migrations: `dotnet ef database update --project RaceDay.Infrastructure --startup-project RaceDay.API`
4. Press F5 to launch Swagger UI

### CI/CD
GitHub Actions builds the project and runs all unit tests on every push.
