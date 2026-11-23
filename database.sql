/* ============================================================
   Объектный программный модуль
   "Стипендия и прочие выплаты студентам ПолесГУ"
   Скрипт создания базы данных SQL Server
   ------------------------------------------------------------
   Запуск: sqlcmd -S localhost -E -C -i database.sql
   либо открыть в SSMS и выполнить целиком.
   ============================================================ */

USE master;
GO

IF DB_ID(N'ScholarshipPaym') IS NULL
    CREATE DATABASE ScholarshipPaym;
GO

USE ScholarshipPaym;
GO

/* ---------- Удаление старых таблиц (порядок важен из-за FK) ---------- */
DROP TABLE IF EXISTS dbo.PaymentAssignments;
DROP TABLE IF EXISTS dbo.ScholarshipRules;
DROP TABLE IF EXISTS dbo.Users;
DROP TABLE IF EXISTS dbo.PaymentTypes;
DROP TABLE IF EXISTS dbo.Students;
GO

/* ---------- Студенты ---------- */
CREATE TABLE dbo.Students
(
    Id                INT IDENTITY(1,1) NOT NULL,
    StudentCardNumber NVARCHAR(30)  NOT NULL,
    FullName          NVARCHAR(150) NOT NULL,
    Faculty           NVARCHAR(100) NOT NULL CONSTRAINT DF_Students_Faculty DEFAULT (N''),
    Speciality        NVARCHAR(100) NOT NULL CONSTRAINT DF_Students_Speciality DEFAULT (N''),
    StudyGroup        NVARCHAR(30)  NOT NULL,
    Course            INT           NOT NULL CONSTRAINT DF_Students_Course DEFAULT (1),
    StudyForm         NVARCHAR(30)  NOT NULL,                     -- Бюджетная / Платная
    FundingType       NVARCHAR(60)  NOT NULL CONSTRAINT DF_Students_Funding DEFAULT (N''),
    AverageGrade      DECIMAL(4,2)  NOT NULL,

    CONSTRAINT PK_Students PRIMARY KEY (Id),
    CONSTRAINT UQ_Students_Card UNIQUE (StudentCardNumber),
    CONSTRAINT CK_Students_Grade  CHECK (AverageGrade >= 0 AND AverageGrade <= 10),
    CONSTRAINT CK_Students_Course CHECK (Course >= 1 AND Course <= 6)
);
GO

/* ---------- Справочник видов выплат ----------
   Category:     Scholarship (стипендия) | MaterialAid (материальная помощь) | Other (прочие)
   Periodicity:  Monthly (ежемесячно)    | Once (разово)
   Служебные коды хранятся в БД, а на экране показываются
   на выбранном языке (русский / беларуский / английский).            */
CREATE TABLE dbo.PaymentTypes
(
    Id             INT IDENTITY(1,1) NOT NULL,
    Name           NVARCHAR(100) NOT NULL,
    Category       NVARCHAR(30)  NOT NULL,
    Periodicity    NVARCHAR(30)  NOT NULL,
    FundingSource  NVARCHAR(100) NOT NULL CONSTRAINT DF_PaymentTypes_Funding DEFAULT (N''),
    Amount         DECIMAL(10,2) NOT NULL,
    Description    NVARCHAR(300) NULL,
    IsActive       BIT           NOT NULL CONSTRAINT DF_PaymentTypes_Active DEFAULT (1),

    CONSTRAINT PK_PaymentTypes PRIMARY KEY (Id),
    CONSTRAINT UQ_PaymentTypes_Name UNIQUE (Name),
    CONSTRAINT CK_PaymentTypes_Amount CHECK (Amount >= 0),
    CONSTRAINT CK_PaymentTypes_Category CHECK (Category IN (N'Scholarship', N'MaterialAid', N'Other')),
    CONSTRAINT CK_PaymentTypes_Periodicity CHECK (Periodicity IN (N'Monthly', N'Once'))
);
GO

/* ---------- Правила расчёта стипендий ----------
   Speciality = N'' означает "любая специальность" (правило по умолчанию).
   Итоговая сумма = Amount (размер) * Coefficient * число месяцев.      */
CREATE TABLE dbo.ScholarshipRules
(
    Id              INT IDENTITY(1,1) NOT NULL,
    PaymentTypeId   INT           NOT NULL,
    Speciality      NVARCHAR(100) NOT NULL CONSTRAINT DF_Rules_Speciality DEFAULT (N''),
    MinAverageGrade DECIMAL(4,2)  NOT NULL,
    MaxAverageGrade DECIMAL(4,2)  NOT NULL,
    Coefficient     DECIMAL(6,3)  NOT NULL,
    ValidFrom       DATE          NOT NULL,
    ValidTo         DATE          NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Rules_Active DEFAULT (1),

    CONSTRAINT PK_ScholarshipRules PRIMARY KEY (Id),
    CONSTRAINT FK_Rules_PaymentType FOREIGN KEY (PaymentTypeId)
        REFERENCES dbo.PaymentTypes (Id),
    CONSTRAINT CK_Rules_Grade CHECK
        (MinAverageGrade >= 0 AND MaxAverageGrade <= 10
         AND MinAverageGrade <= MaxAverageGrade),
    CONSTRAINT CK_Rules_Coefficient CHECK (Coefficient > 0),
    CONSTRAINT CK_Rules_Dates CHECK (ValidTo IS NULL OR ValidTo >= ValidFrom)
);
GO

/* ---------- Назначенные выплаты ----------
   Status: Assigned (назначена) | Paid (выплачена) | Cancelled (отменена)  */
CREATE TABLE dbo.PaymentAssignments
(
    Id             INT IDENTITY(1,1) NOT NULL,
    StudentId      INT           NOT NULL,
    PaymentTypeId  INT           NOT NULL,
    DocumentNumber NVARCHAR(50)  NULL,
    DocumentDate   DATE          NULL,
    PeriodStart    DATE          NOT NULL,
    PeriodEnd      DATE          NOT NULL,
    AverageGrade   DECIMAL(4,2)  NULL,
    BaseAmount     DECIMAL(10,2) NOT NULL,
    Coefficient    DECIMAL(6,3)  NOT NULL,
    Amount         DECIMAL(12,2) NOT NULL,
    AssignmentDate DATE          NOT NULL,
    PaymentDate    DATE          NULL,
    Status         NVARCHAR(30)  NOT NULL,
    Reason         NVARCHAR(300) NULL,

    CONSTRAINT PK_PaymentAssignments PRIMARY KEY (Id),
    CONSTRAINT FK_Assignments_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Students (Id),
    CONSTRAINT FK_Assignments_PaymentType FOREIGN KEY (PaymentTypeId)
        REFERENCES dbo.PaymentTypes (Id),
    CONSTRAINT CK_Assignments_Period CHECK (PeriodStart <= PeriodEnd),
    CONSTRAINT CK_Assignments_Amount CHECK (Amount >= 0),
    CONSTRAINT CK_Assignments_Coefficient CHECK (Coefficient > 0),
    CONSTRAINT CK_Assignments_Status CHECK (Status IN (N'Assigned', N'Paid', N'Cancelled'))
);
GO

/* ---------- Пользователи системы ----------
   Пароль хранится в виде SHA-256 в шестнадцатеричном виде.
   Учётная запись по умолчанию: admin / admin                        */
CREATE TABLE dbo.Users
(
    Id           INT IDENTITY(1,1) NOT NULL,
    Login        NVARCHAR(50)  NOT NULL,
    PasswordHash NVARCHAR(64)  NOT NULL,
    DisplayName  NVARCHAR(100) NOT NULL,
    Role         NVARCHAR(30)  NOT NULL CONSTRAINT DF_Users_Role DEFAULT (N'Employee'),
    IsActive     BIT           NOT NULL CONSTRAINT DF_Users_Active DEFAULT (1),

    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Login UNIQUE (Login),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Employee'))
);
GO

/* ==================== Тестовые данные ==================== */

INSERT INTO dbo.Users (Login, PasswordHash, DisplayName, Role, IsActive)
VALUES
    (N'admin',
     LOWER(CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', N'admin'), 2)),
     N'Администратор системы', N'Admin', 1);
GO

INSERT INTO dbo.Students
    (StudentCardNumber, FullName, Faculty, Speciality, StudyGroup, Course, StudyForm, FundingType, AverageGrade)
VALUES
    (N'25001', N'Радкевич Максим Иванович', N'Инженерный факультет',
     N'Информационные системы и технологии', N'25ИТ-3', 2, N'Budget', N'Республиканский бюджет', 8.50),
    (N'25002', N'Иванова Анна Сергеевна', N'Инженерный факультет',
     N'Информационные системы и технологии', N'25ИТ-2', 2, N'Budget', N'Республиканский бюджет', 7.20),
    (N'25003', N'Ковальчук Дмитрий Петрович', N'Экономический факультет',
     N'Финансы и кредит', N'25ФК-1', 1, N'Paid', N'Собственные средства', 6.40),
    (N'25004', N'Смирнова Ольга Викторовна', N'Экономический факультет',
     N'Бухгалтерский учёт', N'24БУ-1', 3, N'Budget', N'Республиканский бюджет', 9.10);
GO

INSERT INTO dbo.PaymentTypes (Name, Category, Periodicity, FundingSource, Amount, Description, IsActive)
VALUES
    (N'Учебная стипендия',        N'Scholarship', N'Monthly', N'Республиканский бюджет', 150.00,
     N'Ежемесячная стипендия за успешную учёбу', 1),
    (N'Социальная стипендия',     N'Scholarship', N'Monthly', N'Республиканский бюджет', 200.00,
     N'Ежемесячная социальная стипендия', 1),
    (N'Стипендия Президента',     N'Scholarship', N'Monthly', N'Республиканский бюджет', 500.00,
     N'Стипендия Президента Республики Беларусь', 1),
    (N'Материальная помощь',      N'MaterialAid', N'Once',    N'Профсоюзный бюджет',     300.00,
     N'Разовая материальная помощь нуждающимся студентам', 1),
    (N'Компенсация на проезд',    N'Other',       N'Once',    N'Профсоюзный бюджет',      80.00,
     N'Компенсация стоимости проезда', 1),
    (N'Разовая премия',           N'Other',       N'Once',    N'Средства университета',  100.00,
     N'Поощрение за участие в мероприятиях', 1);
GO

INSERT INTO dbo.ScholarshipRules
    (PaymentTypeId, Speciality, MinAverageGrade, MaxAverageGrade, Coefficient, ValidFrom, ValidTo, IsActive)
VALUES
    -- Учебная стипендия: правила для ИСиТ
    (1, N'Информационные системы и технологии', 8.00, 10.00, 1.400, '2026-09-01', NULL, 1),
    (1, N'Информационные системы и технологии', 6.00,  7.99, 1.000, '2026-09-01', NULL, 1),
    -- Учебная стипендия: правила по умолчанию для любой специальности
    (1, N'',                                    8.00, 10.00, 1.200, '2026-09-01', NULL, 1),
    (1, N'',                                    6.00,  7.99, 0.900, '2026-09-01', NULL, 1),
    -- Социальная стипендия не зависит от балла
    (2, N'',                                    0.00, 10.00, 1.000, '2026-09-01', NULL, 1),
    -- Стипендия Президента
    (3, N'',                                    9.00, 10.00, 1.000, '2026-09-01', NULL, 1);
GO

INSERT INTO dbo.PaymentAssignments
    (StudentId, PaymentTypeId, DocumentNumber, DocumentDate, PeriodStart, PeriodEnd,
     AverageGrade, BaseAmount, Coefficient, Amount, AssignmentDate, PaymentDate, Status, Reason)
VALUES
    (1, 1, N'Приказ №101-ст', '2026-09-01', '2026-09-01', '2027-01-31',
     8.50, 150.00, 1.400, 1050.00, '2026-09-01', NULL, N'Assigned',
     N'Назначение учебной стипендии на семестр'),
    (2, 1, N'Приказ №101-ст', '2026-09-01', '2026-09-01', '2026-12-31',
     7.20, 150.00, 1.000, 600.00, '2026-09-01', '2026-10-05', N'Paid',
     N'Выплата за сентябрь–декабрь'),
    (4, 4, N'Заявление №12', '2026-10-10', '2026-10-15', '2026-10-15',
     NULL, 300.00, 1.000, 300.00, '2026-10-12', '2026-10-15', N'Paid',
     N'Материальная помощь'),
    (3, 5, N'Заявление №15', '2026-11-01', '2026-11-01', '2026-11-30',
     NULL, 80.00, 1.000, 80.00, '2026-11-01', NULL, N'Assigned',
     N'Компенсация проезда');
GO

PRINT N'База ScholarshipPaym успешно создана.';
GO

