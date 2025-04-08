# 📚 BookHive - Library Management System

A powerful and modern ASP.NET Core MVC application designed for efficient library resource management.

---

## 🚀 Overview

**BookHive** is a full-stack, role-based **Library Management System** that streamlines the management of books, authors, categories, and users. With a clean UI and robust backend, it's built to handle everything from user authentication to book rentals and cataloging.

---

## ✨ Features

### 👤 User Management
- 🔐 Role-based access control (Admin/User)
- 📧 Email confirmation for account activation
- 🔄 Password reset functionality
- ✅ Account locking/unlocking and status toggling
- ✏️ Create, edit, and manage user accounts

### 📚 Book Catalog
- ➕ Full CRUD operations for books
- 🔍 Advanced search, filtering & sorting
- 🏷️ Categorization & multiple editions per book
- 📷 Image upload with Cloudinary & thumbnail support
- 📦 Real-time book availability tracking

### ✍️ Author Management
- 👥 Associate books with authors
- 🗃️ Manage author database
- 🧼 Soft-delete support

### 🗂️ Category System
- 🧭 Hierarchical categories
- 🔗 Category-book relationships
- 🔄 CRUD operations

---

## 🔧 Technologies Used

- 🧩 **ASP.NET Core MVC**
- 🗄️ **Entity Framework Core**
- 🔄 **AutoMapper**
- ☁️ **Cloudinary** (for image handling)
- 👥 **Identity Framework** (Authentication & Authorization)
- 📊 **DataTables.js**
- 🎨 **Bootstrap 5**

---

## 📡 API Endpoints

### `/Users`
- `GET /Users` – List users  
- `POST /Users/Create` – Add user  
- `POST /Users/Edit` – Update user  
- `POST /Users/ResetPassword` – Reset password  
- `POST /Users/ToggleStatus/{id}` – Activate/deactivate user  
- `POST /Users/Unlock/{id}` – Unlock account  
- `GET /Users/AllowUserName` – Validate username  
- `GET /Users/AllowEmail` – Validate email  

### `/Books`
- `GET /Books` – List books (DataTable)  
- `POST /Books/Create` – Add book  
- `POST /Books/Edit` – Edit book  
- `POST /Books/ToggleStatus/{id}` – Change book status  
- `GET /Books/AllowItem` – Validate book title  

### `/Authors`
- Full CRUD operations  
- Uniqueness validation  
- Status toggling

### `/BookCopies`
- Add, edit, and toggle status for book copies

### `/Categories`
- Full CRUD & validation  
- Status toggling

---

## 🧱 Project Structure

