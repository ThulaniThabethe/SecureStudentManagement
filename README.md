If you want to use this repo please fork it as this is a school project t hat im am sharing eith my lecturer  any changes will severly harm my grades.

Project Overview
Project Type:
•	ASP.NET Core Razor Pages web application
•	Targeting .NET 8
•	Implements a Secure Student Management System
---
Key Features & Structure
1. Authentication
•	OAuth 2.0 authentication is handled in Controllers/AccountController.cs and Views/Account/Login.cshtml.
•	Only authorized administrators can access the dashboard and student management features.
2. Student Management
•	CRUD Operations:
•	Controllers/StudentController.cs manages student records.
•	Views for Create, Edit, Details, Delete, and List are in Views/Student/.
•	Model:
•	Models/Learner.cs defines the student entity.
•	Models/PagedLearnerViewModel.cs supports pagination for large datasets.
•	Soft Delete:
•	Implemented via an "Active/Inactive" status in the model.
3. Azure Integration
•	Cosmos DB:
•	Services/StudentCosmosDbService.cs handles student data storage and retrieval.
•	Blob Storage:
•	Services/StudentCloudStorageService.cs manages profile image uploads and secure access (using SAS tokens).
•	Profile Images:
•	Images are uploaded, validated, and stored in Azure Blob Storage.
•	The image URL is saved in Cosmos DB, not the image itself.
4. Security
•	Role-based Access:
•	Enforced via Helpers/StudentAccessHelper.cs and controller logic.
•	Input Validation:
•	Validation scripts and logic in views and possibly in controllers/models.
5. UI & Views
•	Razor Pages in Views/Student/ for all student management actions.
•	Shared layout and validation scripts in Views/Shared/.
6. Configuration
•	appsettings.json contains configuration for Azure services and authentication.
7. Testing
•	Unit tests in SecureStudentManagement.Tests/ (e.g., LearnerTests.cs, UnitTest1.cs).
---
Summary
The project is a secure, cloud-integrated student management system using Razor Pages, with:
•	OAuth 2.0 authentication for admin access
•	CRUD operations for students (with profile image upload)
•	Azure Cosmos DB for student data
•	Azure Blob Storage for images (with SAS token security)
•	Unit tests for core logic
