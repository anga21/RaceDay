IF DB_ID('RaceDay')  IS NOT NULL
   Begin 

    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
    END 
    GO 

 CREATE  DATABASE RaceDay ;
  GO
 
USE RaceDay;
GO
 
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results ;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF  OBJECT_ID('dbo.Categories','U'  )IS NOT NULL DROP TABLE dbo.Categories ;
IF  OBJECT_ID('dbo.Route', 'U') IS NOT NULL DROP TABLE dbo.Route ;
 IF OBJECT_ID(' dbo.WeatherForecast', 'U')IS NOT NULL DROP TABLE dbo.WeatherForecast;
IF OBJECT_ID(' dbo.Event','U')IS NOT NULL DROP TABLE dbo.Event ;
IF OBJECT_ID ('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
 GO

CREATE TABLE  dbo.[User] (
    UserID    INT IDENTITY(1 ,1) PRIMARY KEY,
    FirstName  NVARCHAR(50)  NOT NULL,
    LastName  NVARCHAR(50) NOT  NULL ,
    PhoneNumber   NVARCHAR(20) NULL,
    Email   NVARCHAR(100 ) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(255) NOT  NULL,
    Role  NVARCHAR(20)  NOT NULL
    CONSTRAINT CK_User_Role CHECK (Role IN ('Organiser','Participant')),
    CreatedAt  DATETIME  NOT NULL DEFAULT  GETDATE()
   );
GO
 

CREATE TABLE dbo.Event (
    EventID   INT IDENTITY( 1,1) PRIMARY KEY,
    UserID   INT   NOT NULL,
   EventName  NVARCHAR(100)   NOT NULL,
   EventDate  DATE  NOT NULL,
    StartTime  TIME   NOT NULL,
    EndTime TIME  NULL,
    Location   NVARCHAR(150)   NOT NULL,
    Description   NVARCHAR(500)   NULL,
    CONSTRAINT FK_Event_User FOREIGN KEY (UserID)
        REFERENCES dbo.[User](UserID));
GO

 

CREATE TABLE dbo.Route (
   RouteID   INT IDENTITY(1,1) PRIMARY KEY,
  EventID  INT   NOT NULL,
    RouteName   NVARCHAR(100) NOT NULL,
   StartPoint   NVARCHAR(150) NOT NULL,
   EndPoint NVARCHAR(150) NOT NULL,
     DistanceKm DECIMAL(6,2) NOT NULL,
     Description  NVARCHAR(500)  NULL,
   CONSTRAINT   FK_Route_Event FOREIGN KEY (EventID)
      REFERENCES dbo.Event(EventID));
GO
 
 
 
 
 CREATE TABLE dbo.WeatherForecast (
    WeatherForecastID  INT IDENTITY(1,1 ) PRIMARY KEY,
     EventID  INT  NOT  NULL,
    ForecastDate   DATE NOT NULL,
    Conditions  NVARCHAR(100) NULL,
   Temperature  DECIMAL(4,1) NULL,
    WindSpeed   DECIMAL(4,1) NULL ,
    CONSTRAINT FK_Weather_Event FOREIGN KEY (EventID)
        REFERENCES dbo.Event(EventID)
);
 GO
 

CREATE TABLE  dbo.Categories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
   EventID  INT  NOT  NULL,
    CategoryName NVARCHAR(50) NOT  NULL,
  Description  NVARCHAR(300) NULL,
    CONSTRAINT FK_Category_Event FOREIGN KEY (EventID)
   REFERENCES dbo.Event(EventID)
);
GO
 

CREATE  TABLE dbo.Enrolments (
    EnrolmentID  INT IDENTITY(1,1 ) PRIMARY KEY,
    EventID INT  NOT NULL,
    ParticipantID  INT   NOT NULL,
    CategoryID INT   NOT NULL,
   BibNumber   NVARCHAR(10)  NULL,
   EnrolmentDate   DATETIME   NOT NULL DEFAULT GETDATE(),
   Status NVARCHAR(20)   NOT NULL DEFAULT 'Active',

   CONSTRAINT CK_Enrolment_Status CHECK (Status IN ('Active', 'Cancelled')),
    CONSTRAINT  FK_Enrolment_Event FOREIGN KEY (EventID)
      REFERENCES  dbo.Event(EventID ),
    CONSTRAINT   FK_Enrolment_Participant  FOREIGN KEY( ParticipantID)
      REFERENCES  dbo.[User](UserID) ,
    CONSTRAINT FK_Enrolment_Category  FOREIGN KEY(CategoryID)
    REFERENCES  dbo.Categories(CategoryID),
    CONSTRAINT UQ_Enrolment_Participant_Event UNIQUE (EventID, ParticipantID, CategoryID)
);
GO
 

CREATE  TABLE dbo.Results (
    ResultsID  INT IDENTITY(1,1) PRIMARY KEY,
     EnrolmentID INT  NOT NULL UNIQUE,
    FinishTime TIME NULL,
    BibNumber  NVARCHAR(10)  NULL,
   Position  INT  NULL,
    CapturedAt  DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentID)
        REFERENCES dbo.Enrolments( EnrolmentID)
);
GO
 
 

INSERT INTO dbo.[User] (FirstName,  LastName, PhoneNumber,Email,PasswordHash, Role )
VALUES
('Thabo', 'Nkosi',  '0821234567', 'thabo.nkosi@raceday.co.za','HASHED_PW_1', 'Organiser ') ,
( 'Sarah','van der Merwe', '0823456789', '  sarah.vdm@raceday.co.za', 'HASHED_PW_2', 'Organiser'),
(' Lindiwe', 'Dube', '0731234567', 'lindiwe.dube@example.com', 'HASHED_PW_3', 'Participant ') ,
('James', 'Botha', '0739876543', 'james.botha@example.com', 'HASHED_PW_4', 'Participant ');
GO
 

INSERT INTO dbo.Event(UserID, EventName, EventDate, StartTime, EndTime, Location, Description)
VALUES
(1, 'Johannesburg City Marathon', '2026-10-18', '06:00', '11:00', 'Johannesburg CBD', 'Annual road marathon through the city centre.'),
(1 ,'Soweto Fun Run', '2026-11-01', '07:00', '10:00', 'Soweto', 'Family-friendly 5km/10km fun run.'),
(2, ' Cape Winelands Cycle Challenge', '2026-11-22', '06:30', '13:00', 'Stellenbosch', 'Road cycling event through the Winelands.');
GO
 

INSERT INTO dbo.Route (EventID, RouteName, StartPoint, EndPoint, DistanceKm, Description)
VALUES
(1, 'Full Mrathon Route' , 'Sandton', 'Ellis Park Stadium', 42.20, 'Full marathon route through Sandton and the CBD.'),
(1, 'Half Marathon Route', 'Rosebank ', 'Ells  Park Stadium ', 21.10,' Half marathon route'),
(2 , '5km Route', 'Soweto Theatre ', 'Soweto  Theatre', 5.00, 'Flat loop route'),
(2, '10km Route', 'Soweto Theatre', 'Vilakazi Street', 10.00, 'Loop through Vilakazi Street.'),
(3,' Winelands 80km Route', 'Stelenbosch Square', 'Franschhoek', 80.00, 'Scenic cycling route through the Winelands');
 GO

INSERT  INTO  dbo.WeatherForecast (EventID,ForecastDate, Conditions,Temperature, WindSpeed)
VALUES
(1, '2026-10-17', 'Partly Cloudy', 18.5,12.0),
(2,'2026-10-31', 'Sunny',22.0, 8.5 ),
(3,'2026-11-21 ','Clear', 24.0, 15.0);
GO



INSERT INTO dbo.Categories ( EventID, CategoryName, Description)
VALUES
(1, 'Full Marathon', '42.2km individual categorY'),
(1 , 'Half Marathon','21.1km individual category' ),
( 2, '5km Fun Run','Casual  5km category, all ages'),
(2, '10km Race', 'Compettive 10km category .'),
(3, ' 80km Road Cycle ','  Indvidual road cycling category.');
 GO


INSERT INTO dbo.Enrolments(EventID, ParticipantID, CategoryID,BibNumber,Status )
VALUES
(1, 3,  1, 'A101', 'Active'),  
(1, 4,2, 'A102', 'Active'),   
(2, 3, 3,' B201', 'Active') ,   
(3, 4, 5, 'C301','Active');  
 GO

INSERT   INTO dbo.Results (EnrolmentID,FinishTime,BibNumber, Position)
VALUES
( 1,'03:45:12', 'A101', 15),
(3, '00:24:30','B201' ,4);
GO
