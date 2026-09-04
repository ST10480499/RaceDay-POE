# RaceDay-POE
## About the System

RaceDay is a web-based event management system built for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse events, enter events, and track their personal results.

This repository contains Part 1 of the Portfolio of Evidence for PROG6212: system planning and database design.

## Roles

- **Organiser** – can create, edit, and delete events, manage event categories, capture participant results, and view all event enrolments.
- **Participant** – can create an account, browse events, enter an event by selecting a category, view their own enrolments, and track their personal results.

## Part 1 Contents

The `/docs` folder contains the planning documents for this part of the POE:

- `RaceDay_ERD.png` – Entity Relationship Diagram for the database
- `RaceDay_API_Endpoint_Plan.pdf` – planned API endpoints, covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results
- `RaceDay_Database.sql` – SQL script that creates and seeds the full database schema in SQL Server Management Studio (SSMS)

## CI/CD

A GitHub Actions workflow (`.github/workflows/validate.yml`) runs on every push to `main` and checks that the `/docs` folder exists and contains the required planning files.

Screenshot of a successful build:

![image alt](https://github.com/ST10480499/RaceDay-POE/blob/main/Screenshot%202026-09-02%20231635.png?raw=true)

## Video

Unlisted YouTube walkthrough of the planning documents, ERD decisions, endpoint plan choices, and a live run of the SQL script in SSMS:

video works well

https://youtu.be/jFkDMj5XdUE
