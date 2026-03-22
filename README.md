```markdown
# 🎓 Learnova - eLearning Platform

A comprehensive eLearning platform built with **ASP.NET Core MVC 8.0** featuring course management, interactive learning, quizzes with gamification, and progress tracking.

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![C#](https://img.shields.io/badge/C%23-12-blue)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-blueviolet)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-red)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Database Design](#-database-design)
- [Getting Started](#-getting-started)
- [Default Credentials](#-default-credentials)
- [Project Structure](#-project-structure)
- [Screenshots](#-screenshots)
- [API Endpoints](#-api-endpoints)
- [Business Rules](#-business-rules)

---

## ✨ Features

### 👨‍🏫 Instructor/Admin Module
- **Course Dashboard** with Kanban & List views
- **Course Management** - Create, Edit, Delete, Publish/Unpublish
- **Lesson Management** - Video (YouTube/Drive), Document (PDF), Image support
- **Quiz Builder** - Interactive drag-and-drop style quiz creation
- **Attendee Management** - Invite learners via email
- **Contact Attendees** - Send messages to enrolled learners
- **Reporting Dashboard** - Track learner progress with filters & CSV export
- **Access Control** - Open, Invitation-only, or Paid courses

### 👨‍🎓 Learner Module
- **Browse Courses** - Search, filter, and discover courses
- **My Courses Dashboard** - Track enrolled courses and progress
- **Course Detail Page** - Overview, lessons list, reviews & ratings
- **Full-Screen Player** - Immersive learning with sidebar navigation
- **Quiz System** - One question per page, multiple attempts
- **Points & Badges** - Gamification with 6 badge levels
- **Reviews & Ratings** - 5-star rating system with text reviews
- **Progress Tracking** - Per-lesson completion, course percentage

### 🔐 Authentication & Authorization
- **3 Roles**: Admin, Instructor, Learner
- **ASP.NET Core Identity** with cookie authentication
- **Role-based access control** on controllers and views

---

## 🛠 Tech Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | ASP.NET Core MVC 8.0 |
| **Language** | C# 12 |
| **Database** | SQL Server (LocalDB) |
| **ORM** | Entity Framework Core 8.0 |
| **Authentication** | ASP.NET Core Identity |
| **Frontend** | Bootstrap 5.3, Font Awesome 6 |
| **JavaScript** | Vanilla JS (No jQuery dependency) |

---

## 🏗 Architecture

```
┌──────────────────────────────────────────────┐
│                 PRESENTATION                  │
│  Views (Razor) ←→ Controllers ←→ ViewModels  │
├──────────────────────────────────────────────┤
│                BUSINESS LOGIC                 │
│  Services (Interfaces + Implementations)      │
├──────────────────────────────────────────────┤
│                 DATA ACCESS                   │
│  Entity Framework Core ←→ SQL Server          │
│  Models (Entities) ←→ ApplicationDbContext     │
└──────────────────────────────────────────────┘
```

### Design Patterns Used
- **Service Layer Pattern** - Business logic separated from controllers
- **Dependency Injection** - All services registered in DI container
- **Repository Pattern** (via EF Core DbContext)
- **MVC Pattern** - Model-View-Controller
- **Async/Await** - All database operations are asynchronous

---

## 🗄 Database Design

### Entity Relationship Diagram

```
ApplicationUser (ASP.NET Identity)
    │
    ├── 1:N ── Course (as Instructor)
    ├── 1:N ── CourseEnrollment (as Learner)
    ├── 1:N ── LessonProgress
    ├── 1:N ── QuizAttempt
    └── 1:N ── CourseReview

Course
    ├── 1:N ── Lesson
    │              └── 1:N ── LessonAttachment
    ├── 1:N ── Quiz
    │              └── 1:N ── QuizQuestion
    │                             └── 1:N ── QuizOption
    ├── 1:N ── CourseEnrollment
    └── 1:N ── CourseReview
```

### Tables (11 Total)

| # | Table | Purpose |
|---|-------|---------|
| 1 | AspNetUsers | User accounts (extended with Points, Badge) |
| 2 | AspNetRoles | Roles (Admin, Instructor, Learner) |
| 3 | AspNetUserRoles | User-Role mapping |
| 4 | Courses | Course information |
| 5 | Lessons | Video/Document/Image lessons |
| 6 | LessonAttachments | Additional resources |
| 7 | Quizzes | Quiz configuration & rewards |
| 8 | QuizQuestions | Quiz questions |
| 9 | QuizOptions | Answer options |
| 10 | CourseEnrollments | User enrollment & progress |
| 11 | LessonProgresses | Per-lesson completion |
| 12 | QuizAttempts | Quiz attempt history |
| 13 | CourseReviews | Ratings & reviews |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (comes with Visual Studio)
- Git

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/learnova.git
cd learnova

# 2. Restore packages
dotnet restore

# 3. Create database
dotnet ef database update

# 4. Run the application
dotnet run
```


---

## 🔑 Default Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | admin@learnova.com | Admin@123 |
| **Instructor** | instructor@learnova.com | Instructor@123 |
| **Learner** | learner@learnova.com | Learner@123 |

> These accounts are automatically created on first run via SeedData.

---

## 📁 Project Structure

```
Learnova/
├── Controllers/           # MVC Controllers (5 files)
├── Data/                  # DbContext & Seed Data
├── Models/
│   ├── Entities/          # Database entities (11 files)
│   └── ViewModels/        # Form models (4 files)
├── Services/
│   ├── Interfaces/        # Service contracts (7 files)
│   └── Implementation/    # Service implementations (7 files)
├── Views/
│   ├── Shared/            # Layout & Partials
│   ├── Home/              # Landing page
│   ├── Account/           # Auth pages (5 files)
│   ├── Instructor/        # Admin/Instructor pages (5 files)
│   ├── Learner/           # Learner pages (5 files)
│   └── Reporting/         # Reports page
├── wwwroot/               # Static files (CSS, JS, uploads)
├── Program.cs             # App configuration
└── appsettings.json       # Connection string
```

---

## 🔌 API Endpoints

### Instructor
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Instructor/Dashboard` | Courses dashboard |
| POST | `/Instructor/QuickCreateCourse` | Create course |
| GET | `/Instructor/EditCourse/{id}` | Edit course page |
| POST | `/Instructor/EditCourse` | Save course changes |
| POST | `/Instructor/TogglePublish` | Publish/Unpublish |
| POST | `/Instructor/SaveLesson` | Create/Edit lesson |
| POST | `/Instructor/DeleteLesson` | Delete lesson |
| POST | `/Instructor/SaveQuiz` | Create/Edit quiz |

### Learner
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Learner/Courses` | Browse courses |
| GET | `/Learner/MyCourses` | My enrollments |
| GET | `/Learner/CourseDetail/{id}` | Course details |
| POST | `/Learner/JoinCourse` | Enroll in course |
| GET | `/Learner/Player?lessonId={id}` | Lesson player |
| POST | `/Learner/CompleteLesson` | Mark lesson done |
| GET | `/Learner/TakeQuiz?quizId={id}` | Take quiz |
| POST | `/Learner/SubmitQuiz` | Submit quiz answers |
| POST | `/Learner/AddReview` | Add review |

---

## 📜 Business Rules

### Visibility vs Access
| Setting | Visibility | Access Rule |
|---------|-----------|-------------|
| **Everyone + Open** | All users see it | Anyone can enroll |
| **SignedIn + Open** | Only logged-in users | Anyone logged-in can enroll |
| **Everyone + OnInvitation** | All see it | Only invited can learn |
| **Everyone + OnPayment** | All see it | Must pay to learn |

### Badge Levels
| Badge | Points Required |
|-------|----------------|
| Beginner | 0-19 |
| Newbie | 20-39 |
| Explorer | 40-59 |
| Achiever | 60-79 |
| Specialist | 80-99 |
| Expert | 100-119 |
| Master | 120+ |

### Quiz Scoring
- **Pass Mark**: 60%
- **Points decrease with attempts**: 1st > 2nd > 3rd > 4th+
- **Multiple attempts allowed**
- **Points only awarded on pass**

---


## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Built For

**24-Hour Hackathon Challenge** - Building a complete eLearning platform from scratch.

---

### ⭐ If you found this project helpful, please give it a star!
```

---

## 🚀 GitHub Push Commands

```bash
# 1. Initialize git (agar nahi kiya hai)
git init

# 2. Add .gitignore
# Create .gitignore file with content below

# 3. Add all files
git add .

# 4. Commit
git commit -m "Learnova - Complete eLearning Platform"

# 5. Add remote
git remote add origin https://github.com/Tanmay140503/learnova-lms.git

# 6. Push
git push -u origin main
```
