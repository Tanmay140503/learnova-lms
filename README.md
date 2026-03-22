# Complete Setup Guide + README

## 📋 Kahan Kya Paste Karna Hai

Tumhara project structure ye hona chahiye. Main sirf **changes/missing files** bata raha hoon:

```
Learnova/
├── Controllers/
│   ├── HomeController.cs           ✅ Already hai
│   ├── AccountController.cs        ✅ Already hai
│   ├── InstructorController.cs     🔄 REPLACE (last version use karo)
│   ├── LearnerController.cs        🔄 REPLACE (neeche diya hai)
│   └── ReportingController.cs      ✅ Already hai
│
├── Data/
│   ├── ApplicationDbContext.cs     ✅ Already hai
│   └── SeedData.cs                 ✅ Already hai
│
├── Models/
│   ├── Entities/                   ✅ Already hai (sab 11 files)
│   ├── ViewModels/                 ✅ Already hai
│   └── ErrorViewModel.cs           ✅ Already hai
│
├── Services/
│   ├── Interfaces/                 ✅ Already hai (7 files)
│   └── Implementation/             ✅ Already hai (7 files)
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml                🔄 REPLACE
│   │   ├── _LessonModalPartial.cshtml    ✅ Already hai
│   │   ├── _LessonsListPartial.cshtml    ✅ Already hai
│   │   ├── _QuizzesListPartial.cshtml    ✅ Already hai
│   │   ├── _ValidationScriptsPartial.cshtml ✅ Already hai
│   │   └── Error.cshtml                  ✅ Already hai
│   │
│   ├── Home/
│   │   ├── Index.cshtml                  ✅ Already hai
│   │   └── Privacy.cshtml                ✅ Already hai
│   │
│   ├── Account/
│   │   ├── Login.cshtml                  ✅ Already hai
│   │   ├── Register.cshtml               ✅ Already hai
│   │   ├── Profile.cshtml                ✅ Already hai
│   │   ├── AccessDenied.cshtml           ✅ Already hai
│   │   └── ForgotPassword.cshtml         ✅ Already hai
│   │
│   ├── Instructor/
│   │   ├── Dashboard.cshtml              🔄 REPLACE (last fixed version)
│   │   ├── EditCourse.cshtml             🔄 REPLACE (last fixed version)
│   │   ├── QuizBuilder.cshtml            🔄 REPLACE (last fixed version)
│   │   ├── AddAttendees.cshtml           ✅ Already hai
│   │   └── ContactAttendees.cshtml       ✅ Already hai
│   │
│   ├── Learner/
│   │   ├── Courses.cshtml                🔄 REPLACE (neeche diya hai)
│   │   ├── MyCourses.cshtml              🔄 REPLACE (neeche diya hai)
│   │   ├── CourseDetail.cshtml           🔄 REPLACE (neeche diya hai)
│   │   ├── Player.cshtml                 🔄 REPLACE (neeche diya hai)
│   │   └── TakeQuiz.cshtml              ⭐ NEW FILE (neeche diya hai)
│   │
│   ├── Reporting/
│   │   └── Dashboard.cshtml              ✅ Already hai
│   │
│   ├── _ViewImports.cshtml               ✅ Already hai
│   └── _ViewStart.cshtml                 ✅ Already hai
│
├── wwwroot/
│   ├── css/site.css                      ✅ Already hai
│   ├── js/site.js                        ✅ Already hai
│   └── uploads/                          ✅ Already hai
│
├── Program.cs                            ✅ Already hai
├── appsettings.json                      ✅ Already hai
└── README.md                             ⭐ NEW FILE (neeche diya hai)
```

---

## 🔄 Files Jo REPLACE Karne Hain

Ye files tumne pehle banayi thi, ab inhe latest fixed versions se replace karo:

| File | Kaunsa Version Use Karo |
|------|------------------------|
| `InstructorController.cs` | Previous chat mein diya tha (with IAntiforgery) |
| `Dashboard.cshtml` (Instructor) | Previous chat mein diya tha (script at top) |
| `EditCourse.cshtml` | Previous chat mein diya tha (with AntiForgeryToken) |
| `QuizBuilder.cshtml` | Previous chat mein diya tha (with CSRF header) |

---

## ⭐ NEW + REPLACE Files (Paste Karo)

### File 1: `Views/Learner/TakeQuiz.cshtml` (NEW FILE - Create Karo)

```html
@model Learnova.Models.Entities.Quiz
@{
    Layout = null;
    var attemptCount = (int)ViewBag.AttemptCount;
    var bestScore = (int)ViewBag.BestScore;
    var questions = Model.Questions.OrderBy(q => q.OrderIndex).ToList();
}
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Quiz: @Model.Title</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />
    <style>
        body { background: #f0f2f5; min-height: 100vh; }
        .quiz-container { max-width: 800px; margin: 0 auto; padding: 30px 15px; }
        .quiz-option {
            padding: 15px 20px; margin-bottom: 12px; border: 2px solid #e0e0e0;
            border-radius: 12px; cursor: pointer; transition: all 0.2s;
            display: flex; align-items: center; background: #fff;
        }
        .quiz-option:hover { border-color: #0d6efd; background: #f8f9ff; }
        .quiz-option.selected { border-color: #0d6efd; background: #e7f0ff; box-shadow: 0 2px 8px rgba(13,110,253,0.2); }
        .quiz-option input { margin-right: 15px; width: 20px; height: 20px; }
        .result-card { text-align: center; padding: 40px; }
        .points-display { font-size: 3rem; font-weight: 700; color: #198754; }
    </style>
</head>
<body>
<form style="display:none;" id="csrfForm">@Html.AntiForgeryToken()</form>

<div class="quiz-container">
    <!-- Intro Screen -->
    <div id="quizIntro">
        <div class="card shadow-sm">
            <div class="card-body text-center p-5">
                <i class="fas fa-question-circle fa-5x text-primary mb-4"></i>
                <h2>@Model.Title</h2>
                @if (!string.IsNullOrEmpty(Model.Description))
                {
                    <p class="text-muted">@Model.Description</p>
                }
                <div class="row justify-content-center mt-4 mb-4">
                    <div class="col-4">
                        <h4 class="text-primary">@questions.Count</h4>
                        <small class="text-muted">Questions</small>
                    </div>
                    <div class="col-4">
                        <h4 class="text-info">@attemptCount</h4>
                        <small class="text-muted">Previous Attempts</small>
                    </div>
                    <div class="col-4">
                        <h4 class="text-success">@bestScore%</h4>
                        <small class="text-muted">Best Score</small>
                    </div>
                </div>
                <div class="alert alert-info">
                    <i class="fas fa-info-circle me-1"></i> Multiple attempts allowed. Points decrease with each attempt.
                </div>
                <div class="mb-3">
                    <small class="text-muted">
                        Points: 1st try: <strong>@Model.FirstAttemptPoints</strong> pts |
                        2nd: <strong>@Model.SecondAttemptPoints</strong> pts |
                        3rd: <strong>@Model.ThirdAttemptPoints</strong> pts |
                        4th+: <strong>@Model.FourthAttemptPoints</strong> pts
                    </small>
                </div>
                <button class="btn btn-primary btn-lg px-5" onclick="startQuiz()">
                    <i class="fas fa-play me-2"></i> Start Quiz
                </button>
                <br />
                <a href="/Learner/CourseDetail/@Model.CourseId" class="btn btn-link mt-3">
                    <i class="fas fa-arrow-left me-1"></i> Back to Course
                </a>
            </div>
        </div>
    </div>

    <!-- Question Screen -->
    <div id="quizQuestion" style="display:none;">
        <div class="card shadow-sm">
            <div class="card-header bg-white">
                <div class="d-flex justify-content-between align-items-center">
                    <span id="questionCounter" class="fw-bold">Question 1 of @questions.Count</span>
                    <div class="progress flex-grow-1 mx-3" style="height: 8px;">
                        <div class="progress-bar bg-primary" id="quizProgress" style="width: 0%"></div>
                    </div>
                </div>
            </div>
            <div class="card-body p-4">
                <h4 id="questionText" class="mb-4"></h4>
                <div id="optionsContainer"></div>
            </div>
            <div class="card-footer bg-white d-flex justify-content-between">
                <span id="selectedInfo" class="text-muted small"></span>
                <button class="btn btn-primary" id="proceedBtn" onclick="proceedQuestion()" disabled>
                    Proceed <i class="fas fa-arrow-right ms-1"></i>
                </button>
            </div>
        </div>
    </div>

    <!-- Result Screen -->
    <div id="quizResult" style="display:none;">
        <div class="card shadow-sm">
            <div class="card-body result-card" id="resultContent">
                <div class="spinner-border text-primary"></div>
                <p class="mt-3">Calculating results...</p>
            </div>
        </div>
    </div>
</div>

<script>
    var csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    var questions = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(
        questions.Select(q => new {
            id = q.Id,
            text = q.QuestionText,
            options = q.Options.Select(o => new { id = o.Id, text = o.OptionText })
        })
    ));

    var answers = {};
    var currentQ = 0;

    function startQuiz() {
        document.getElementById('quizIntro').style.display = 'none';
        document.getElementById('quizQuestion').style.display = 'block';
        showQuestion();
    }

    function showQuestion() {
        var q = questions[currentQ];
        document.getElementById('questionCounter').textContent = 'Question ' + (currentQ + 1) + ' of ' + questions.length;
        document.getElementById('quizProgress').style.width = ((currentQ + 1) / questions.length * 100) + '%';
        document.getElementById('questionText').textContent = q.text;
        document.getElementById('selectedInfo').textContent = '';
        document.getElementById('proceedBtn').disabled = true;

        if (currentQ === questions.length - 1) {
            document.getElementById('proceedBtn').innerHTML = 'Complete Quiz <i class="fas fa-check ms-1"></i>';
        } else {
            document.getElementById('proceedBtn').innerHTML = 'Proceed <i class="fas fa-arrow-right ms-1"></i>';
        }

        var html = '';
        q.options.forEach(function(opt) {
            var isSelected = answers[q.id] === opt.id;
            html += '<div class="quiz-option ' + (isSelected ? 'selected' : '') + '" onclick="selectOption(' + q.id + ',' + opt.id + ')">';
            html += '<input type="radio" name="q' + q.id + '" ' + (isSelected ? 'checked' : '') + '>';
            html += '<span>' + opt.text + '</span></div>';
        });
        document.getElementById('optionsContainer').innerHTML = html;
        if (answers[q.id]) document.getElementById('proceedBtn').disabled = false;
    }

    function selectOption(qId, optId) {
        answers[qId] = optId;
        document.getElementById('proceedBtn').disabled = false;
        document.getElementById('selectedInfo').textContent = 'Answer selected';
        showQuestion();
    }

    function proceedQuestion() {
        if (currentQ < questions.length - 1) {
            currentQ++;
            showQuestion();
        } else {
            submitQuiz();
        }
    }

    function submitQuiz() {
        document.getElementById('quizQuestion').style.display = 'none';
        document.getElementById('quizResult').style.display = 'block';
        document.getElementById('resultContent').innerHTML = '<div class="spinner-border text-primary"></div><p class="mt-3">Calculating results...</p>';

        var formData = new FormData();
        formData.append('quizId', @Model.Id);
        formData.append('answersJson', JSON.stringify(answers));
        formData.append('__RequestVerificationToken', csrfToken);

        fetch('/Learner/SubmitQuiz', { method: 'POST', body: formData })
        .then(function(r) { return r.json(); })
        .then(function(data) {
            var html = '';

            if (data.isPassed) {
                html += '<i class="fas fa-check-circle fa-5x text-success mb-3"></i>';
                html += '<h2 class="text-success">Congratulations! You Passed!</h2>';
            } else {
                html += '<i class="fas fa-times-circle fa-5x text-danger mb-3"></i>';
                html += '<h2 class="text-danger">Try Again!</h2>';
            }

            html += '<div class="my-4">';
            html += '<h1>' + data.score + '%</h1>';
            html += '<p class="text-muted">' + data.correctAnswers + ' out of ' + data.totalQuestions + ' correct</p>';
            html += '</div>';

            if (data.isPassed && data.pointsEarned > 0) {
                html += '<div class="alert alert-success">';
                html += '<div class="points-display">+' + data.pointsEarned + ' pts</div>';
                html += '<p class="mb-1">Total Points: <strong>' + data.totalPoints + '</strong></p>';
                html += '<p class="mb-0">Badge: <span class="badge bg-warning fs-5">' + data.badgeLevel + '</span></p>';
                html += '</div>';
            } else if (!data.isPassed) {
                html += '<div class="alert alert-warning">';
                html += '<i class="fas fa-info-circle me-1"></i> You need 60% to pass. Try again!';
                html += '</div>';
            }

            html += '<div class="mt-4">';
            html += '<a href="/Learner/TakeQuiz?quizId=@Model.Id" class="btn btn-primary me-2"><i class="fas fa-redo me-1"></i> Try Again</a>';
            html += '<a href="/Learner/CourseDetail/@Model.CourseId" class="btn btn-secondary"><i class="fas fa-arrow-left me-1"></i> Back to Course</a>';
            html += '</div>';

            document.getElementById('resultContent').innerHTML = html;
        });
    }
</script>
</body>
</html>
```

---

## ⭐ File 2: `README.md` (Root folder mein create karo)

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

### Access the application
- **URL**: `https://localhost:5001` or `http://localhost:5000`

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

## 📸 Screenshots

### Home Page
Landing page with hero section, features, categories, and testimonials.

### Instructor Dashboard
Kanban and List view for managing courses with search and stats.

### Course Editor
Tabbed interface for editing course details, content, description, options, and quizzes.

### Quiz Builder
Interactive quiz creation with drag-and-drop questions and configurable rewards.

### Learner Course Page
Course detail with progress tracking, lessons list, and reviews.

### Full-Screen Player
Immersive learning experience with sidebar navigation and video/document/image viewer.

### Quiz Taking
One-question-per-page quiz with progress bar and instant results.

### Reporting Dashboard
Course-wise learner progress with status filters, stats cards, and CSV export.

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

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

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
