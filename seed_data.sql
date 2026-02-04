-- ================================================
-- SUPABASE SEED DATA - DATA LABELING PROJECT
-- ================================================
-- Run this in Supabase SQL Editor to initialize database
-- ================================================

-- 1. Insert 4 Fixed Roles
-- ================================================
INSERT INTO public."Role" ("RoleId", "RoleName")
VALUES 
  ('11111111-1111-1111-1111-111111111111', 'Admin'),
  ('22222222-2222-2222-2222-222222222222', 'Manager'),
  ('33333333-3333-3333-3333-333333333333', 'Reviewer'),
  ('44444444-4444-4444-4444-444444444444', 'Annotator')
ON CONFLICT ("RoleId") DO NOTHING;

-- 2. Insert Admin User
-- ================================================
-- Username: admin
-- Password: password12345
-- PasswordHash: /VJslblzkvBvOUTZDRRwDlqAiTuLqKc4mVy0rZAY6Y8= (SHA256 Base64)
-- IsFirstLogin: false (admin không cần đổi password)
-- ================================================
INSERT INTO public."User" (
  "UserId", 
  "Username", 
  "PasswordHash", 
  "DisplayName", 
  "Email", 
  "PhoneNumber",
  "RoleId", 
  "IsActive", 
  "IsFirstLogin", 
  "CreatedAt"
)
VALUES (
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  'admin',
  '/VJslblzkvBvOUTZDRRwDlqAiTuLqKc4mVy0rZAY6Y8=',
  'System Administrator',
  'admin@datalabel.com',
  NULL,
  '11111111-1111-1111-1111-111111111111',
  true,
  false,
  NOW()
)
ON CONFLICT ("UserId") DO NOTHING;

-- 3. Insert Demo Reviewer User (Optional)
-- ================================================
-- Username: reviewer_demo
-- Password: DefaultPassword123
-- PasswordHash: wcSP+bLu9CpS33hnSxxOeoOXFzfBVdwcja42jcwb7tg= (SHA256 Base64)
-- IsFirstLogin: true (phải đổi password lần đầu)
-- ================================================
INSERT INTO public."User" (
  "UserId", 
  "Username", 
  "PasswordHash", 
  "DisplayName", 
  "Email", 
  "PhoneNumber",
  "RoleId", 
  "IsActive", 
  "IsFirstLogin", 
  "CreatedAt"
)
VALUES (
  'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  'reviewer_demo',
  'wcSP+bLu9CpS33hnSxxOeoOXFzfBVdwcja42jcwb7tg=',
  'Demo Reviewer',
  'reviewer@datalabel.com',
  NULL,
  '33333333-3333-3333-3333-333333333333',
  true,
  true,
  NOW()
)
ON CONFLICT ("UserId") DO NOTHING;

-- ================================================
-- VERIFICATION QUERIES
-- ================================================

-- Check roles
SELECT * FROM public."Role" ORDER BY "RoleName";

-- Check users
SELECT 
  "UserId",
  "Username",
  "DisplayName",
  "Email",
  "IsActive",
  "IsFirstLogin",
  r."RoleName"
FROM public."User" u
LEFT JOIN public."Role" r ON u."RoleId" = r."RoleId"
ORDER BY "CreatedAt";

-- ================================================
-- PASSWORD HASH REFERENCE (SHA256 Base64)
-- ================================================
-- Admin@123         → iyT8vFZvpCDMM9jLZZXLPfWZMc2IJ8dPaX9y/lj5x4s=
-- DefaultPassword123 → vBOF3jLi3C6uJXxBs5dQKI0XRb4Y8mKb8Qz7dQQMLDY=
-- ================================================
