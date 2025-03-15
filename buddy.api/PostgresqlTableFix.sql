CREATE EXTENSION IF NOT EXISTS postgis;
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE EXTENSION IF NOT EXISTS postgis;

CREATE TABLE "Users" (
    "UserId" uuid NOT NULL,
    "Auth0Id" character varying(128) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "IsVerified" boolean NOT NULL,
    "IsAdmin" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastLoginAt" timestamp with time zone,
    "Active" boolean NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("UserId")
);

CREATE TABLE "Locations" (
    "LocationId" uuid NOT NULL,
    "Name" character varying(255) NOT NULL,
    "Address" text NOT NULL,
    "City" character varying(100) NOT NULL,
    "State" character varying(100) NOT NULL,
    "PostalCode" character varying(20) NOT NULL,
    "Country" character varying(100) NOT NULL,
    "Coordinates" geometry NOT NULL,
    "IsVerified" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_Locations" PRIMARY KEY ("LocationId"),
    CONSTRAINT "FK_Locations_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("UserId")
);

CREATE TABLE "Sports" (
    "SportId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" text NOT NULL,
    "IconUrl" character varying(255) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid,
    CONSTRAINT "PK_Sports" PRIMARY KEY ("SportId"),
    CONSTRAINT "FK_Sports_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("UserId")
);

CREATE TABLE "UserProfiles" (
    "ProfileId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Bio" text NOT NULL,
    "ProfilePictureUrl" character varying(255) NOT NULL,
    "PreferredLocation" geometry NOT NULL,
    "MaxTravelDistance" integer NOT NULL,
    "PreferredDays" text[] NOT NULL,
    "PreferredTimes" text[] NOT NULL,
    "VerificationStatus" character varying(50) NOT NULL,
    "VerificationCompletedAt" timestamp with time zone,
    "PublicProfile" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserProfiles" PRIMARY KEY ("ProfileId"),
    CONSTRAINT "FK_UserProfiles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "UserReports" (
    "ReportId" uuid NOT NULL,
    "ReportingUserId" uuid NOT NULL,
    "ReportedUserId" uuid NOT NULL,
    "Reason" text NOT NULL,
    "Status" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ReviewedAt" timestamp with time zone,
    "ReviewedByUserId" uuid,
    CONSTRAINT "PK_UserReports" PRIMARY KEY ("ReportId"),
    CONSTRAINT "FK_UserReports_Users_ReportedUserId" FOREIGN KEY ("ReportedUserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserReports_Users_ReportingUserId" FOREIGN KEY ("ReportingUserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserReports_Users_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users" ("UserId")
);

CREATE TABLE "Verifications" (
    "VerificationId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "VerificationType" character varying(50) NOT NULL,
    "VerificationProvider" character varying(100) NOT NULL,
    "ProviderReferenceId" character varying(255) NOT NULL,
    "Status" character varying(50) NOT NULL,
    "InitiatedAt" timestamp with time zone NOT NULL,
    "CompletedAt" timestamp with time zone,
    "ExpiresAt" timestamp with time zone,
    "VerificationData" text NOT NULL,
    CONSTRAINT "PK_Verifications" PRIMARY KEY ("VerificationId"),
    CONSTRAINT "FK_Verifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "Activities" (
    "ActivityId" uuid NOT NULL,
    "SportId" uuid NOT NULL,
    "LocationId" uuid,
    "Name" character varying(255) NOT NULL,
    "Description" text NOT NULL,
    "RecurringSchedule" character varying(255) NOT NULL,
    "DifficultyLevel" character varying(50) NOT NULL,
    "MaxParticipants" integer,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid,
    CONSTRAINT "PK_Activities" PRIMARY KEY ("ActivityId"),
    CONSTRAINT "FK_Activities_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES "Locations" ("LocationId"),
    CONSTRAINT "FK_Activities_Sports_SportId" FOREIGN KEY ("SportId") REFERENCES "Sports" ("SportId") ON DELETE CASCADE,
    CONSTRAINT "FK_Activities_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("UserId")
);

CREATE TABLE "Matches" (
    "MatchId" uuid NOT NULL,
    "RequesterId" uuid NOT NULL,
    "RecipientId" uuid NOT NULL,
    "SportId" uuid,
    "Status" character varying(50) NOT NULL,
    "RequestedAt" timestamp with time zone NOT NULL,
    "RespondedAt" timestamp with time zone,
    CONSTRAINT "PK_Matches" PRIMARY KEY ("MatchId"),
    CONSTRAINT "FK_Matches_Sports_SportId" FOREIGN KEY ("SportId") REFERENCES "Sports" ("SportId"),
    CONSTRAINT "FK_Matches_Users_RecipientId" FOREIGN KEY ("RecipientId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Matches_Users_RequesterId" FOREIGN KEY ("RequesterId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
);

CREATE TABLE "UserSports" (
    "UserSportId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "SportId" uuid NOT NULL,
    "SkillLevel" character varying(50) NOT NULL,
    "YearsExperience" integer,
    "Notes" text NOT NULL,
    "IsPublic" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserSports" PRIMARY KEY ("UserSportId"),
    CONSTRAINT "FK_UserSports_Sports_SportId" FOREIGN KEY ("SportId") REFERENCES "Sports" ("SportId") ON DELETE CASCADE,
    CONSTRAINT "FK_UserSports_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

CREATE TABLE "Conversations" (
    "ConversationId" uuid NOT NULL,
    "MatchId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastMessageAt" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_Conversations" PRIMARY KEY ("ConversationId"),
    CONSTRAINT "FK_Conversations_Matches_MatchId" FOREIGN KEY ("MatchId") REFERENCES "Matches" ("MatchId")
);

CREATE TABLE "Messages" (
    "MessageId" uuid NOT NULL,
    "ConversationId" uuid NOT NULL,
    "SenderId" uuid NOT NULL,
    "Content" text NOT NULL,
    "SentAt" timestamp with time zone NOT NULL,
    "ReadAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("MessageId"),
    CONSTRAINT "FK_Messages_Conversations_ConversationId" FOREIGN KEY ("ConversationId") REFERENCES "Conversations" ("ConversationId") ON DELETE CASCADE,
    CONSTRAINT "FK_Messages_Users_SenderId" FOREIGN KEY ("SenderId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('3a98a529-8ea9-46e1-8acf-1c4558fd8bf4', TIMESTAMPTZ '-infinity', NULL, 'A team sport played with a ball and hoop', '/images/sports/basketball.svg', TRUE, 'Basketball');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('8056afb6-6f5a-4799-a69c-f8e5c2de53ad', TIMESTAMPTZ '-infinity', NULL, 'A racket sport played on a rectangular court', '/images/sports/tennis.svg', TRUE, 'Tennis');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('8731bdcf-cda2-48d5-b92f-2cf9b4797c61', TIMESTAMPTZ '-infinity', NULL, 'A team sport played with a ball over a net', '/images/sports/volleyball.svg', TRUE, 'Volleyball');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('9544f52b-8a96-457a-a481-c7b4f0dd78fd', TIMESTAMPTZ '-infinity', NULL, 'A sport involving riding bicycles', '/images/sports/cycling.svg', TRUE, 'Cycling');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('a0e58388-ccba-4cdf-9af2-25ee470adb68', TIMESTAMPTZ '-infinity', NULL, 'A water-based sport', '/images/sports/swimming.svg', TRUE, 'Swimming');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('a20ed55e-13d3-4b92-8abc-c3e7c8ae9550', TIMESTAMPTZ '-infinity', NULL, 'A physical, mental and spiritual practice', '/images/sports/yoga.svg', TRUE, 'Yoga');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('b03df04f-405a-46e4-a424-a0351f67ff98', TIMESTAMPTZ '-infinity', NULL, 'A team sport played with a ball', '/images/sports/soccer.svg', TRUE, 'Soccer');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('c9ea073c-f6fb-4a29-ad69-b5c2060ead7a', TIMESTAMPTZ '-infinity', NULL, 'Walking in natural environments', '/images/sports/hiking.svg', TRUE, 'Hiking');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('caeb79ef-c17e-443e-914b-b27fb9a8c1cc', TIMESTAMPTZ '-infinity', NULL, 'A club-and-ball sport', '/images/sports/golf.svg', TRUE, 'Golf');
INSERT INTO "Sports" ("SportId", "CreatedAt", "CreatedByUserId", "Description", "IconUrl", "IsActive", "Name")
VALUES ('cb5b097c-ab7b-4b28-8c7f-4fef91d8a9e7', TIMESTAMPTZ '-infinity', NULL, 'An individual sport involving running distances', '/images/sports/running.svg', TRUE, 'Running');

CREATE INDEX "IX_Activities_CreatedByUserId" ON "Activities" ("CreatedByUserId");

CREATE INDEX "IX_Activities_LocationId" ON "Activities" ("LocationId");

CREATE INDEX "IX_Activities_SportId" ON "Activities" ("SportId");

CREATE INDEX "IX_Conversations_MatchId" ON "Conversations" ("MatchId");

CREATE INDEX "IX_Locations_CreatedByUserId" ON "Locations" ("CreatedByUserId");

CREATE INDEX "IX_Matches_RecipientId" ON "Matches" ("RecipientId");

CREATE UNIQUE INDEX "IX_Matches_RequesterId_RecipientId_SportId" ON "Matches" ("RequesterId", "RecipientId", "SportId");

CREATE INDEX "IX_Matches_SportId" ON "Matches" ("SportId");

CREATE INDEX "IX_Messages_ConversationId" ON "Messages" ("ConversationId");

CREATE INDEX "IX_Messages_SenderId" ON "Messages" ("SenderId");

CREATE INDEX "IX_Sports_CreatedByUserId" ON "Sports" ("CreatedByUserId");

CREATE UNIQUE INDEX "IX_UserProfiles_UserId" ON "UserProfiles" ("UserId");

CREATE INDEX "IX_UserReports_ReportedUserId" ON "UserReports" ("ReportedUserId");

CREATE INDEX "IX_UserReports_ReportingUserId" ON "UserReports" ("ReportingUserId");

CREATE INDEX "IX_UserReports_ReviewedByUserId" ON "UserReports" ("ReviewedByUserId");

CREATE INDEX "IX_UserSports_SportId" ON "UserSports" ("SportId");

CREATE UNIQUE INDEX "IX_UserSports_UserId_SportId" ON "UserSports" ("UserId", "SportId");

CREATE INDEX "IX_Verifications_UserId" ON "Verifications" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250314222648_FixPostgresqlTableNames', '9.0.3');

COMMIT;

