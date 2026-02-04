-- ================================================
-- DATABASE SCHEMA - DATA LABELING PROJECT
-- ================================================
-- Run this FIRST in Supabase SQL Editor
-- Then run seed_data.sql
-- ================================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ================================================
-- 1. ROLE TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Role" (
    "RoleId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "RoleName" VARCHAR NOT NULL UNIQUE
);

-- ================================================
-- 2. USER TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."User" (
    "UserId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Username" VARCHAR NOT NULL UNIQUE,
    "PasswordHash" VARCHAR NOT NULL,
    "DisplayName" VARCHAR,
    "Email" VARCHAR UNIQUE,
    "PhoneNumber" VARCHAR UNIQUE,
    "RoleId" UUID NOT NULL REFERENCES public."Role"("RoleId"),
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "IsFirstLogin" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for User table
CREATE INDEX IF NOT EXISTS "User_Email_idx" ON public."User"("Email");
CREATE INDEX IF NOT EXISTS "User_Username_idx" ON public."User"("Username");
CREATE INDEX IF NOT EXISTS "User_RoleId_idx" ON public."User"("RoleId");

-- ================================================
-- 3. PROJECT TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Project" (
    "ProjectId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectName" VARCHAR NOT NULL,
    "Description" TEXT,
    "ProjectType" VARCHAR,
    "Status" VARCHAR DEFAULT 'Active',
    "CreatedBy" UUID REFERENCES public."User"("UserId"),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 4. DATASET TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Dataset" (
    "DatasetId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectId" UUID NOT NULL REFERENCES public."Project"("ProjectId") ON DELETE CASCADE,
    "DatasetName" VARCHAR NOT NULL,
    "Description" TEXT,
    "CreatedBy" UUID REFERENCES public."User"("UserId"),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 5. LABEL TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Label" (
    "LabelId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectId" UUID NOT NULL REFERENCES public."Project"("ProjectId") ON DELETE CASCADE,
    "LabelName" VARCHAR NOT NULL,
    "Description" TEXT,
    "Color" VARCHAR,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 6. CATEGORY TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Category" (
    "CategoryId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectId" UUID NOT NULL REFERENCES public."Project"("ProjectId") ON DELETE CASCADE,
    "CategoryName" VARCHAR NOT NULL,
    "Description" TEXT,
    "CreatedBy" UUID REFERENCES public."User"("UserId"),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 7. LABELSET TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."LabelSet" (
    "LabelSetId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectId" UUID NOT NULL REFERENCES public."Project"("ProjectId") ON DELETE CASCADE,
    "LabelSetName" VARCHAR NOT NULL,
    "Description" TEXT,
    "CreatedBy" UUID REFERENCES public."User"("UserId"),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 8. GUIDELINE TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Guideline" (
    "GuidelineId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectId" UUID NOT NULL REFERENCES public."Project"("ProjectId") ON DELETE CASCADE,
    "GuidelineText" TEXT NOT NULL,
    "Version" INTEGER DEFAULT 1,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 9. ANNOTATIONTASK TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."AnnotationTask" (
    "TaskId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "DatasetId" UUID NOT NULL REFERENCES public."Dataset"("DatasetId") ON DELETE CASCADE,
    "TaskName" VARCHAR NOT NULL,
    "Status" VARCHAR DEFAULT 'Pending',
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 10. ASSIGNMENT TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Assignment" (
    "AssignmentId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TaskId" UUID NOT NULL REFERENCES public."AnnotationTask"("TaskId") ON DELETE CASCADE,
    "UserId" UUID NOT NULL REFERENCES public."User"("UserId"),
    "AssignedBy" UUID REFERENCES public."User"("UserId"),
    "AssignedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "DueDate" TIMESTAMP,
    "Status" VARCHAR DEFAULT 'Assigned'
);

-- ================================================
-- 11. ANNOTATION TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Annotation" (
    "AnnotationId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "TaskId" UUID NOT NULL REFERENCES public."AnnotationTask"("TaskId") ON DELETE CASCADE,
    "UserId" UUID NOT NULL REFERENCES public."User"("UserId"),
    "LabelId" UUID REFERENCES public."Label"("LabelId"),
    "AnnotationData" JSONB,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP
);

-- ================================================
-- 12. REVIEW TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."Review" (
    "ReviewId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "AnnotationId" UUID NOT NULL REFERENCES public."Annotation"("AnnotationId") ON DELETE CASCADE,
    "ReviewerId" UUID NOT NULL REFERENCES public."User"("UserId"),
    "Status" VARCHAR DEFAULT 'Pending',
    "Comments" TEXT,
    "ReviewedAt" TIMESTAMP,
    "ApprovedBy" UUID REFERENCES public."User"("UserId")
);

-- ================================================
-- 13. EXPORTJOB TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."ExportJob" (
    "ExportJobId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProjectId" UUID NOT NULL REFERENCES public."Project"("ProjectId") ON DELETE CASCADE,
    "Format" VARCHAR NOT NULL,
    "Status" VARCHAR DEFAULT 'Pending',
    "FilePath" VARCHAR,
    "RequestedBy" UUID REFERENCES public."User"("UserId"),
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CompletedAt" TIMESTAMP
);

-- ================================================
-- 14. ACTIVITYLOG TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."ActivityLog" (
    "LogId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID REFERENCES public."User"("UserId"),
    "Action" VARCHAR NOT NULL,
    "EntityType" VARCHAR,
    "EntityId" UUID,
    "Details" JSONB,
    "Timestamp" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- 15. SYSTEMCONFIG TABLE
-- ================================================
CREATE TABLE IF NOT EXISTS public."SystemConfig" (
    "ConfigId" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ConfigKey" VARCHAR NOT NULL UNIQUE,
    "ConfigValue" TEXT,
    "Description" TEXT,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ================================================
-- CREATE INDEXES FOR PERFORMANCE
-- ================================================
CREATE INDEX IF NOT EXISTS "Project_CreatedBy_idx" ON public."Project"("CreatedBy");
CREATE INDEX IF NOT EXISTS "Dataset_ProjectId_idx" ON public."Dataset"("ProjectId");
CREATE INDEX IF NOT EXISTS "Label_ProjectId_idx" ON public."Label"("ProjectId");
CREATE INDEX IF NOT EXISTS "Annotation_TaskId_idx" ON public."Annotation"("TaskId");
CREATE INDEX IF NOT EXISTS "Annotation_UserId_idx" ON public."Annotation"("UserId");
CREATE INDEX IF NOT EXISTS "Assignment_UserId_idx" ON public."Assignment"("UserId");
CREATE INDEX IF NOT EXISTS "Review_AnnotationId_idx" ON public."Review"("AnnotationId");
CREATE INDEX IF NOT EXISTS "ActivityLog_UserId_idx" ON public."ActivityLog"("UserId");

-- ================================================
-- SUCCESS MESSAGE
-- ================================================
DO $$
BEGIN
    RAISE NOTICE '✅ Database schema created successfully!';
    RAISE NOTICE '📝 Next step: Run seed_data.sql to insert roles and admin user';
END $$;
