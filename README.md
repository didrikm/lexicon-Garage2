# Lexicon Garage 3.0

This project is a parking management application built with ASP.NET Core and Entity Framework Core. It allows users to manage parking spots, vehicles, and user roles. The system supports both admin and regular user roles with different levels of access.

---

## Features
- **Admin functionalities:**
  - Add and manage vehicle types.
  - Assign users to roles.
  - View and manage all parked vehicles and their details.

- **User functionalities:**
  - Register and park vehicles.
  - Search and filter parked vehicles by registration number, type, or parking time.

---

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- SQL Server or an equivalent database
- Visual Studio or any IDE with C# support

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/lexicon-garage2.git
   ```
2. Navigate to the project directory:
   ```bash
   cd lexicon-garage2
   ```
3. Apply migrations to the database:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

---

## Seeded Accounts

When the application is started for the first time, it seeds the database with the following default roles and accounts:

### Admin Login
- **Email:** `admin@admin.com`
- **Password:** `Abcd_1234`

### User Login
- **Email:** `user@user.com`
- **Password:** `Abcd_1234`

---

## Logging In

### As an Admin
1. Navigate to the login page in your browser.
2. Use the following credentials:
   - Email: `admin@admin.com`
   - Password: `Abcd_1234`
3. After logging in, you can manage vehicle types, view all parked vehicles, and assign roles to users.

### As a Regular User
1. Navigate to the login page in your browser.
2. Use the following credentials:
   - Email: `user@user.com`
   - Password: `Abcd_1234`
3. After logging in, you can register vehicles, park vehicles, and view your parked vehicles.

---

## Roles and Permissions

### Admin
- Manage all users and roles.
- View and manage all parked vehicles.
- Add, update, or remove vehicle types.

### User
- Register and manage their own vehicles.
- Park vehicles in available spots.
- View and search their parked vehicles.

---

## Seed Data

The application initializes the following vehicle types and sizes in the database:
- **Car** (Size: 1)
- **Motorcycle** (Size: 1)
- **Truck** (Size: 2)
- **Plane** (Size: 5)

---

## Future Enhancements
- Add notifications for available parking spots.
- Integrate payment systems for parking fees.
- Support for multi-language localization.

---

