# ShareMeal — Food Redistribution & Surplus Management Platform
**Final Year Project (FYP) Specification & Documentation**  
**Developer:** Muhammad Khulfan  
**Technology Stack:** ASP.NET Core (.NET 8) MVC, Entity Framework Core, SQLite, TailwindCSS, Bootstrap 5  

---

## 1. Executive Summary & Problem Statement
In the contemporary hospitality sector, substantial quantities of high-quality surplus food are discarded daily due to the lack of dynamic, real-time coordination between food businesses and charitable relief organizations. 

**ShareMeal** is an enterprise-grade web application architected to eliminate commercial food wastage and alleviate community hunger. It establishes an automated digital bridge enabling restaurants and food donors to catalog surplus items and registered charities/NGOs to claim and distribute them to vulnerable populations.

---

## 2. System Architecture & Role-Based Access Control (RBAC)

The application implements strict Role-Based Access Control (RBAC) leveraging ASP.NET Core Identity:

### A. Restaurant / Donor Module
- **Registration & Verification:** Secure self-service onboarding for restaurants and culinary donors.
- **Donation Cataloging:** Dynamic listing of surplus batches with quantitative specs (servings, expiry window, dietary tags, pickup instructions).
- **Logistics & Status Tracking:** Real-time visibility into claim requests, pickup schedules, and historical donation analytics.

### B. Charity / NGO Module
- **Live Food Marketplace:** Centralized searchable dashboard displaying active, real-time surplus batches within geographic proximity.
- **Claim & Allocation Protocol:** One-click reservation system preventing double-allocation of critical food supplies.
- **Dispatch Details:** Automatic generation of pickup credentials, donor contact telemetry, and route coordination.

### C. System Administration & Oversight Module
- **Centralized Command Dashboard:** Comprehensive visibility into overall platform metrics, volume of diverted food waste, and active organizations.
- **Entity Governance:** Approval, auditing, and moderation of participating donor and recipient accounts.
- **Audit Trails & Reporting:** Comprehensive transaction history and reporting exports for compliance and performance assessment.

---

## 3. Technical Implementation & Design Highlights
- **Model-View-Controller (MVC) Pattern:** Strict separation of business logic, database transactions, and presentation layer.
- **Data Persistence:** Entity Framework Core (Code-First / Migrations) paired with an optimized SQLite relational database engine.
- **Asynchronous Operations:** Fully asynchronous repository and controller pipelines for high-concurrency throughput.
- **Responsive & Accessible UI:** Modern glassmorphic dark theme engineered with TailwindCSS and custom CSS design tokens.
- **Security & Integrity:** Anti-forgery token validation (CSRF), parameterized SQL queries (SQL injection prevention), and encrypted credential hashing.

---

## 4. Key Performance Objectives
1. **Zero Food Wastage:** Accelerate the time-to-claim cycle for perishable food surpluses.
2. **Operational Efficiency:** Eliminate manual, ad-hoc phone coordination through automated digital dispatching.
3. **Data Transparency:** Provide verifiable impact metrics regarding meals served and landfill diversion.

