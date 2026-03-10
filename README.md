Inventory Management System
-------------------------------------

A full-featured Windows desktop inventory management software built for 
AL-Madina Electric & Solar House.

Powered by ASP.NET Core 9, Electron.NET, Bootstrap 5, and SQLite for 
fast, reliable and offline-capable performance.


Screenshots
-----------
<img width="1303" height="619" alt="image" src="https://github.com/user-attachments/assets/94a3c29a-68fa-41b3-a5fb-f3d9a5ddf8b9" />
<img width="1215" height="598" alt="image" src="https://github.com/user-attachments/assets/d19b5215-a796-47e7-8392-033dc0757872" />
<img width="1263" height="639" alt="image" src="https://github.com/user-attachments/assets/d169c1db-98b1-4a7b-922f-7d14694e051b" />
<img width="1248" height="625" alt="image" src="https://github.com/user-attachments/assets/7513b152-7733-4f9d-97dd-a9cdbba6d7d5" />
<img width="1108" height="635" alt="image" src="https://github.com/user-attachments/assets/3a586d04-a31b-4076-8370-6c74d2c9d05d" />
<img width="1074" height="632" alt="image" src="https://github.com/user-attachments/assets/a83c8883-c334-4ca1-bf26-5ecb75f6f3f4" />
<img width="1251" height="588" alt="image" src="https://github.com/user-attachments/assets/34ca5e3a-1a5a-48db-b318-ee58e4e5d322" />


Features
--------

1. Inventory Management
   - Add, edit, and delete inventory items
   - Each item: Code, Name, Base Price, Stock Quantity
   - Real-time search with instant filtering
   - Smart pagination on all pages

2. Invoice Management
   - Create invoices with customer details
   - Dynamic product search and add to invoice
   - Supports multiple items with + / - quantity
   - Stock validation
   - Amount adjustment (add/deduct)
   - Payment received tracking
   - Auto subtotal & total calculation

3. Print & PDF
   - Thermal Printer Support (EPSON TM-T188IV – 72mm)
   - Multi-page invoice printing (29 items/page)
   - Live print progress indicator
   - PDF download via QuestPDF

4. Customer (Electrician) Management
   - Add and manage electricians
   - Fields: Name, Phone, Address
   - Delete with confirmation
   - Real-time search

5. Search & Pagination
   - Live search in Inventory, Invoices, Customers
   - Smart pagination
   - AJAX-based partial loading

6. Keyboard Shortcuts
   F2       => Focus product search
   Ctrl + S => Save invoice
   Escape   => Clear search


Tech Stack
----------

Framework       : ASP.NET Core 9 (Razor Pages)
Desktop         : Electron.NET v23.6.2
Database        : SQLite + EF Core 9
PDF Generator   : QuestPDF 2025.7.1
UI Framework    : Bootstrap 5 + Icons
Frontend JS     : jQuery 3.6
ORM             : Entity Framework Core


Project Structure
-----------------

FinalInventerySystem/
│
├── Models/
│   ├── Inventory.cs
│   ├── Invoice.cs
│   ├── InvoiceItem.cs
│   └── Customer.cs
│
├── Pages/
│   ├── Inventories/
│   ├── Invoices/
│   └── Customer/
│
├── Services/
│   └── ApplicationDBcontext.cs
│
├── Helpers/
│   └── PaginatedList.cs
│
├── wwwroot/
├── main.js
├── Program.cs
└── appsettings.json


Getting Started
---------------

Prerequisites:
- .NET 9 SDK
- Node.js
- Git

Installation:

# Clone repository
git clone https://github.com/Hasaankhokhar-dev/Inventery_management_system.git

# Go to project folder
cd Inventery_management_system/FinalInventerySystem

# Restore .NET packages
dotnet restore

# Install Node packages
npm install

# Run as web
dotnet run

# Run as desktop app (Electron)
dotnet electronize start


Build Desktop App (Windows Installer)
-------------------------------------

# Publish .NET project
dotnet publish -c Release -o published

# Build Electron installer
npm run build

The installer will be created inside:  dist/


Database
--------

SQLite database path:

C:\ProgramData\FinalInventorySystem\inventory.db

- Auto-created on first run
- Existing inventory.db in project root is migrated automatically


Key Pages
---------

/Inventories/Index         => View items with search & pagination
/Inventories/Create        => Add item
/Invoices/Index            => View invoices
/Invoices/Create           => Create invoice
/Invoices/Details/{id}     => View/Print/PDF invoice
/Invoices/Edit/{id}        => Edit invoice
/Customer/Index            => Manage electricians/customers

License
-------
This project is PRIVATE and built for internal business use only. If you want to use this project, you must purchase it.

Developer
---------
Muhammad Hasaan Khokhar
