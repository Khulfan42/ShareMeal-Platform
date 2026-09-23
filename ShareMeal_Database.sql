CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "Organizations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Organizations" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Type" INTEGER NOT NULL,
    "ContactEmail" TEXT NOT NULL,
    "Phone" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "Description" TEXT NULL,
    "CreatedAt" TEXT NOT NULL
);

CREATE TABLE "Donations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Donations" PRIMARY KEY AUTOINCREMENT,
    "FoodItem" TEXT NOT NULL,
    "Category" INTEGER NOT NULL,
    "Quantity" TEXT NOT NULL,
    "ExpiryDate" TEXT NOT NULL,
    "PickupInstructions" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "DonorId" INTEGER NOT NULL,
    "RecipientId" INTEGER NULL,
    CONSTRAINT "FK_Donations_Organizations_DonorId" FOREIGN KEY ("DonorId") REFERENCES "Organizations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Donations_Organizations_RecipientId" FOREIGN KEY ("RecipientId") REFERENCES "Organizations" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Donations_DonorId" ON "Donations" ("DonorId");

CREATE INDEX "IX_Donations_RecipientId" ON "Donations" ("RecipientId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402145709_InitialCreate', '8.0.13');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);

CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402145852_IdentitySchema', '8.0.13');

COMMIT;

BEGIN TRANSACTION;

CREATE TABLE "ContactMessages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ContactMessages" PRIMARY KEY AUTOINCREMENT,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "IsResolved" INTEGER NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260402172215_AddContactMessage', '8.0.13');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "Organizations" ADD "IsSuspended" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Organizations" ADD "Status" INTEGER NOT NULL DEFAULT 0;

CREATE TABLE "ef_temp_Organizations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Organizations" PRIMARY KEY AUTOINCREMENT,
    "Address" TEXT NULL,
    "ContactEmail" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "Description" TEXT NULL,
    "IsSuspended" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "Phone" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Type" INTEGER NOT NULL
);

INSERT INTO "ef_temp_Organizations" ("Id", "Address", "ContactEmail", "CreatedAt", "Description", "IsSuspended", "Name", "Phone", "Status", "Type")
SELECT "Id", "Address", "ContactEmail", "CreatedAt", "Description", "IsSuspended", "Name", "Phone", "Status", "Type"
FROM "Organizations";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;

DROP TABLE "Organizations";

ALTER TABLE "ef_temp_Organizations" RENAME TO "Organizations";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260403143148_AddOrganizationStatus', '8.0.13');

COMMIT;

BEGIN TRANSACTION;

ALTER TABLE "Organizations" ADD "OwnerId" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260404083911_AddOrganizationOwner', '8.0.13');

COMMIT;

-- ========================================================
-- SEED DATA: Realistic Pakistani Restaurants & Charities
-- ========================================================
BEGIN TRANSACTION;

-- Organizations (Restaurants: Type 0, Charities: Type 1, NGOs: Type 2)
INSERT INTO "Organizations" ("Id", "Name", "Type", "ContactEmail", "Phone", "Address", "Description", "CreatedAt", "Status", "IsSuspended", "OwnerId") VALUES
(1, 'Savour Foods', 0, 'manager.isb@savourfoods.com', '+92-51-2345678', 'Plot 14, Block H, Blue Area, Islamabad', 'Renowned Pakistani food chain famous for traditional Chicken Pulao and roast chicken.', '2026-09-10 11:30:00', 1, 0, 'b6d98dbf-77b2-451f-9b77-badf75211f63'),
(2, 'The Monal Restaurant', 0, 'donations@themonal.com', '+92-51-2898044', 'Pir Sohawa Road, Margalla Hills, Islamabad', 'Luxury Pakistani fine dining specializing in traditional Barbecue, Karahi, and traditional cuisine.', '2026-09-11 14:15:00', 1, 0, 'b030b296-0369-46b1-8346-79e925c16eb5'),
(3, 'Bundu Khan Restaurant', 0, 'gulberg@bundukhan.pk', '+92-42-35754433', 'Main Boulevard, Gulberg III, Lahore', 'Legacy culinary house known for authentic kababs, curries, and tandoori breads.', '2026-09-12 09:20:00', 1, 0, 'c53d3e43-7f64-431a-92fb-035bbe8e3be9'),
(4, 'Student Biryani', 0, 'saddar@studentbiryani.com', '+92-21-111111978', 'Mansfield Street, Saddar, Karachi', 'Iconic Pakistani biryani pioneers serving fresh catering surplus for distribution.', '2026-09-13 16:45:00', 1, 0, '8aa38541-6af4-41a9-b72b-1c1bcdbe9150'),
(5, 'Cheezious Pakistan', 0, 'f7@cheezious.com', '+92-51-111446699', 'F-7 Markaz, Jinnah Super, Islamabad', 'Fast-growing food chain donating evening catering surplus, burgers, and packaged meals.', '2026-09-14 18:00:00', 1, 0, '53d1339f-c9b6-4a13-9efd-0f1433d3040f'),
(6, 'Saylani Welfare International Trust', 1, 'food.drive@saylaniwelfare.com', '+92-21-111729526', 'A-25, Bahadurabad Chowrangi, Karachi', 'Pakistan’s largest charitable network running daily Dastarkhwan and food relief drives.', '2026-09-08 10:00:00', 1, 0, '9be7ed21-d5e3-4927-b30e-78d7cecdea80'),
(7, 'Edhi Foundation Pakistan', 1, 'relief@edhi.org', '+92-21-32310066', 'Edhi Head Office, Bunder Road, Bolton Market, Karachi', 'Humanitarian relief foundation providing round-the-clock shelter meals and destitute welfare.', '2026-09-09 11:45:00', 1, 0, '04192e46-cff9-4f77-8b60-a3e18740e1cf'),
(8, 'Al-Khidmat Foundation Pakistan', 2, 'community@alkhidmat.org', '+92-42-35957260', '3-Km Khayaban-e-Jinnah, Johar Town, Lahore', 'Non-profit dedicated to disaster relief, hunger alleviation, and orphan care across Pakistan.', '2026-09-10 13:10:00', 1, 0, 'df2ae2e1-757a-486e-8d5a-f4cfacf644b9'),
(9, 'JDC Welfare Organization', 1, 'info@jdcwelfare.org', '+92-21-36341050', 'F.B. Area, Ancholi Block 20, Karachi', 'Grassroots relief organization known for free community meals and emergency disaster kitchen drives.', '2026-09-11 15:20:00', 1, 0, '3237a665-a4b4-4925-a870-7172bc1f37c8'),
(10, 'Habibi Restaurant', 0, 'contact@habibirestaurant.pk', '+92-51-4861234', 'I-8 Markaz, Islamabad', 'Specializes in traditional Peshawari Shinwari, Karahi, and Platters. Awaiting verification.', '2026-09-12 16:30:00', 0, 0, NULL),
(11, 'Kolachi Restaurant', 0, 'admin@kolachirestaurant.com', '+92-21-36123456', 'Do Darya, Phase VIII, DHA, Karachi', 'Seaside dining known for Karahi, Handi, and BBQ banquets. Newly submitted registration.', '2026-09-13 18:20:00', 0, 0, NULL),
(12, 'Salt''n Pepper Village', 0, 'lahore@saltnpepper.com.pk', '+92-42-35850900', '103/B-II, MM Alam Road, Gulberg, Lahore', 'Heritage restaurant chain. Currently under periodic compliance suspension.', '2026-09-10 12:00:00', 2, 1, NULL),
(13, 'Robin Hood Army Pakistan', 1, 'volunteers@robinhoodarmy.pk', '+92-300-8273641', 'Gulshan-e-Iqbal, Block 5, Karachi', 'Zero-funds volunteer organization taking surplus food from restaurants to orphanages.', '2026-09-14 14:10:00', 0, 0, NULL),
(14, 'Shaukat Khanum Nutrition Relief', 2, 'foodsupport@shaukatkhanum.org.pk', '+92-42-35905000', '7A, Block R-3, Johar Town, Lahore', 'Specialized dietary food distribution for impoverished patient families.', '2026-09-14 15:40:00', 0, 0, NULL),
(15, 'Ansar Burney Trust', 1, 'welfare@ansarburney.org', '+92-21-32623382', 'Hassan Manzil, Arambagh, Karachi', 'Human rights and relief trust distributing cooked meals to marginalized groups.', '2026-09-11 11:00:00', 1, 0, NULL);

-- Donations (Surplus food entries)
INSERT INTO "Donations" ("Id", "FoodItem", "Category", "Quantity", "ExpiryDate", "PickupInstructions", "Status", "CreatedAt", "DonorId", "RecipientId") VALUES
(1, 'Chicken Biryani (Catering Daig Boxes)', 0, '45 Meal Boxes (Approx 80 Servings)', datetime('now', '+18 hours'), 'Freshly packed from dinner banquet. Please collect from Kitchen Back Gate, Blue Area.', 0, datetime('now', '-2 hours'), 1, NULL),
(2, 'Special Chicken Pulao with Shami Kabab', 0, '50 Meal Boxes with Raita Packs', datetime('now', '+14 hours'), 'Packed in hygienic disposable food containers. Gate 2 delivery entrance.', 1, datetime('now', '-5 hours'), 1, 6),
(3, 'Mutton Qorma & Roghani Naan', 0, '30 Large Foil Containers', datetime('now', '+20 hours'), 'Hot and ready for pickup. Please bring insulated transport boxes.', 0, datetime('now', '-1 hours'), 3, NULL),
(4, 'Fresh Tandoori Roti & Sheermal', 2, '120 Fresh Breads in Paper Sacks', datetime('now', '+10 hours'), 'Baked fresh this evening. Best distributed before morning breakfast drive.', 2, datetime('now', '-8 hours'), 4, 7),
(5, 'Beef Nihari with Ginger-Chilli Tarka & Naan', 0, '25 Large Portions', datetime('now', '+16 hours'), 'Cooked in sealed food-grade containers. Pickup from Pir Sohawa collection point.', 0, datetime('now', '-3 hours'), 2, NULL),
(6, 'Pure Fresh Milk & Kheer Cups', 3, '35 Liters Milk & 40 Dessert Cups', datetime('now', '+36 hours'), 'Kept in chiller at 4°C. Insulated refrigerator van recommended for pickup.', 1, datetime('now', '-6 hours'), 5, 8),
(7, 'Chicken Karahi with Daal Makhni & Rice', 0, '35 Packaged Dinner Boxes', datetime('now', '+12 hours'), 'Includes disposable cutlery. Ready for distribution at community center.', 0, datetime('now', '-30 minutes'), 3, NULL),
(8, 'Crispy Zinger Burgers & Fried Chicken Packs', 0, '25 Snack Boxes with Fries', datetime('now', '+8 hours'), 'Surplus from corporate dinner event. Immediate pickup from F-7 counter.', 2, datetime('now', '-10 hours'), 5, 9);

-- Contact Messages
INSERT INTO "ContactMessages" ("Id", "FullName", "Email", "Message", "CreatedAt", "IsResolved") VALUES
(1, 'Hamza Tariq (Operations Manager, Bundu Khan)', 'hamza.tariq@bundukhan.pk', 'Assalam o Alaikum! We wish to register our 2 newly opened branches in Rawalpindi on ShareMeal for regular surplus food distribution. Kindly guide us through the verification checklist.', datetime('now', '-1 day'), 1),
(2, 'Dr. Ayesha Malik (Saylani Food Drive Coordinator)', 'ayesha.malik@saylaniwelfare.com', 'JazakAllah Khair to the ShareMeal team! The 50 meal boxes collected yesterday were successfully distributed at the G-9 community welfare center. Extremely smooth coordination.', datetime('now', '-12 hours'), 0),
(3, 'Usman Ghani (Youth Relief Volunteer, Lahore)', 'usman.ghani92@gmail.com', 'We have a team of 15 student volunteers with delivery bikes in Gulberg Lahore. How can we register as logistics volunteers to assist NGOs in picking up food faster?', datetime('now', '-4 hours'), 0);

COMMIT;

