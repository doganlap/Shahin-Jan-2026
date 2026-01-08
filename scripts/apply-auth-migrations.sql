-- Apply GrcAuthDb migrations manually
-- This creates the Identity tables

-- Create __EFMigrationsHistory table if it doesn't exist
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Check if migration already applied
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106183534_InitialAuthSchema') THEN
        
        -- Create AspNetRoles table
        CREATE TABLE IF NOT EXISTS "AspNetRoles" (
            "Id" TEXT NOT NULL,
            "Name" VARCHAR(256),
            "NormalizedName" VARCHAR(256),
            "ConcurrencyStamp" TEXT,
            CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
        );

        -- Create RoleProfile table
        CREATE TABLE IF NOT EXISTS "RoleProfile" (
            "Id" UUID NOT NULL,
            "RoleCode" TEXT NOT NULL,
            "RoleName" TEXT NOT NULL,
            "Layer" TEXT NOT NULL,
            "Department" TEXT NOT NULL,
            "Description" TEXT NOT NULL,
            "Scope" TEXT NOT NULL,
            "Responsibilities" TEXT NOT NULL,
            "ApprovalLevel" INTEGER NOT NULL,
            "ApprovalAuthority" NUMERIC,
            "CanEscalate" BOOLEAN NOT NULL,
            "CanApprove" BOOLEAN NOT NULL,
            "CanReject" BOOLEAN NOT NULL,
            "CanReassign" BOOLEAN NOT NULL,
            "ParticipatingWorkflows" TEXT,
            "IsActive" BOOLEAN NOT NULL,
            "TenantId" UUID,
            "DisplayOrder" INTEGER NOT NULL,
            "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL,
            "ModifiedDate" TIMESTAMP WITH TIME ZONE,
            "CreatedBy" TEXT,
            "ModifiedBy" TEXT,
            "IsDeleted" BOOLEAN NOT NULL,
            "DeletedAt" TIMESTAMP WITH TIME ZONE,
            "RowVersion" BYTEA,
            CONSTRAINT "PK_RoleProfile" PRIMARY KEY ("Id")
        );

        -- Create AspNetUsers table
        CREATE TABLE IF NOT EXISTS "AspNetUsers" (
            "Id" TEXT NOT NULL,
            "FirstName" TEXT NOT NULL,
            "LastName" TEXT NOT NULL,
            "Department" TEXT NOT NULL,
            "JobTitle" TEXT NOT NULL,
            "RoleProfileId" UUID,
            "KsaCompetencyLevel" INTEGER NOT NULL,
            "KnowledgeAreas" TEXT,
            "Skills" TEXT,
            "Abilities" TEXT,
            "AssignedScope" TEXT,
            "IsActive" BOOLEAN NOT NULL,
            "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL,
            "LastLoginDate" TIMESTAMP WITH TIME ZONE,
            "RefreshToken" TEXT,
            "RefreshTokenExpiry" TIMESTAMP WITH TIME ZONE,
            "UserName" VARCHAR(256),
            "NormalizedUserName" VARCHAR(256),
            "Email" VARCHAR(256),
            "NormalizedEmail" VARCHAR(256),
            "EmailConfirmed" BOOLEAN NOT NULL,
            "PasswordHash" TEXT,
            "SecurityStamp" TEXT,
            "ConcurrencyStamp" TEXT,
            "PhoneNumber" TEXT,
            "PhoneNumberConfirmed" BOOLEAN NOT NULL,
            "TwoFactorEnabled" BOOLEAN NOT NULL,
            "LockoutEnd" TIMESTAMP WITH TIME ZONE,
            "LockoutEnabled" BOOLEAN NOT NULL,
            "AccessFailedCount" INTEGER NOT NULL,
            "MustChangePassword" BOOLEAN NOT NULL,
            "LastPasswordChangedAt" TIMESTAMP WITH TIME ZONE,
            CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id"),
            CONSTRAINT "FK_AspNetUsers_RoleProfile_RoleProfileId" FOREIGN KEY ("RoleProfileId") REFERENCES "RoleProfile" ("Id")
        );

        -- Create remaining Identity tables
        CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
            "Id" SERIAL PRIMARY KEY,
            "RoleId" TEXT NOT NULL,
            "ClaimType" TEXT,
            "ClaimValue" TEXT,
            CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
            "Id" SERIAL PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "ClaimType" TEXT,
            "ClaimValue" TEXT,
            CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
            "LoginProvider" TEXT NOT NULL,
            "ProviderKey" TEXT NOT NULL,
            "ProviderDisplayName" TEXT,
            "UserId" TEXT NOT NULL,
            CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
            CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
            "UserId" TEXT NOT NULL,
            "RoleId" TEXT NOT NULL,
            CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
            CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
            CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
            "UserId" TEXT NOT NULL,
            "LoginProvider" TEXT NOT NULL,
            "Name" TEXT NOT NULL,
            "Value" TEXT,
            CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
            CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
        );

        -- Create indexes
        CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
        CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
        CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
        CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
        CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
        CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
        CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_Email" ON "AspNetUsers" ("Email");
        CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedEmail" ON "AspNetUsers" ("NormalizedEmail");
        CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

        -- Record migration as applied
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260106183534_InitialAuthSchema', '8.0.8');

        RAISE NOTICE 'Migration 20260106183534_InitialAuthSchema applied successfully';
    ELSE
        RAISE NOTICE 'Migration 20260106183534_InitialAuthSchema already applied';
    END IF;
END $$;

-- Apply second migration: AddMustChangePasswordToUser
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260106191724_AddMustChangePasswordToUser') THEN
        
        -- Add MustChangePassword column if it doesn't exist
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'MustChangePassword') THEN
            ALTER TABLE "AspNetUsers" ADD COLUMN "MustChangePassword" BOOLEAN NOT NULL DEFAULT FALSE;
        END IF;

        -- Add LastPasswordChangedAt column if it doesn't exist
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'LastPasswordChangedAt') THEN
            ALTER TABLE "AspNetUsers" ADD COLUMN "LastPasswordChangedAt" TIMESTAMP WITH TIME ZONE;
        END IF;

        -- Record migration as applied
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260106191724_AddMustChangePasswordToUser', '8.0.8');

        RAISE NOTICE 'Migration 20260106191724_AddMustChangePasswordToUser applied successfully';
    ELSE
        RAISE NOTICE 'Migration 20260106191724_AddMustChangePasswordToUser already applied';
    END IF;
END $$;

SELECT 'Migrations applied successfully!' AS status;
