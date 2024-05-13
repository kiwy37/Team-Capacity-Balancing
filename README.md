# TeamCapacityBalancing

## Overview

This project is a desktop application designed to assist a team or project leaders in planning the distribution of work among the team members across various projects. It provides functionalities for short-term (couple of sprints) and long-term (full release) resource balancing. The main features include:

- Definition of release calendar
- Definition of resources and teams
- Importing tasks and stories from the local Jira SQL database
- Defining resource distribution on projects/stories
- Defining short-term periods and assigned stories
- Visualization of resource balance for short-term and long-term periods

## Main Functionalities

### Definition of Release Calendar

Allows defining the start and finish date of a release, along with the sprint durations. Sprints can be customized to cover the entire release period.

### Definition of a Resource

Enables defining resources with properties such as name, username, and weekly capacity.

### Definition of a Team

Allows creating teams with a designated team leader and team members from the defined resources.

### Import Tasks and Stories

Queries SQL database to import assigned tasks and stories, ensuring positive remaining time for each.

### Definition of Resource Distribution

Enables defining the percentage of work allocation for each resource on projects/stories. Default distribution can be proposed for generic stories.

### Definition of Short-Term Period

Define the duration of short-term periods and specify stories to be implemented within this period.

### Visualization of Resource Balance

Calculates resource balance based on capacity, remaining tasks, and distribution of work, highlighting positive and negative balances.

## Technical Details

### Application Type

Desktop application

### Technologies Used

- <img src="https://raw.githubusercontent.com/devicons/devicon/master/icons/csharp/csharp-original.svg" alt="csharp" width="20" height="20"/> C# for the Backend
- <img src="https://avaloniateam.gallerycdn.vsassets.io/extensions/avaloniateam/avaloniaforvisualstudio/11.5/1699021556564/Microsoft.VisualStudio.Services.Icons.Default" alt="avalonia" width="20" height="20"/> Avalonia for the UI
- <img src="https://www.vectorlogo.zone/logos/postgresql/postgresql-icon.svg" alt="postgresql" width="20" height="20"/> PostgreSQL for the Database

## Usage
1. Clone the repository.
2. Follow [these steps](https://confluence.atlassian.com/adminjiraserver/connecting-jira-applications-to-postgresql-938846851.html) to bring the Jira database locally.
3. Run `dotnet restore` in the project directory to download and install all required NuGet packages.
4. Run the application.
5. Import tasks and stories.
6. Define resource distribution and short-term periods.
7. Visualize resource balance for effective planning.
   
## Contributors

- [Popa Sebastian](https://github.com/PopaSebastian1)
- [Tăbăcaru Rares-Andrei](https://github.com/Rares-Andrrei)
- [Brinzea Tiberio](https://github.com/Tiberio1234)
