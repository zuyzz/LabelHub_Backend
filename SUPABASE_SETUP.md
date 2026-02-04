# 🎯 SUPABASE SESSION POOLER - SETUP HOÀN CHỈNH

## ✅ ĐÃ THỰC HIỆN

### 1. Connection String (Session Pooler)
**Format chuẩn:**
```
User Id=postgres.<PROJECT_REF>;Password=YOUR_PASSWORD;Server=aws-1-ap-northeast-1.pooler.supabase.com;Port=5432;Database=postgres;Pooling=true;SSL Mode=Require;Trust Server Certificate=true
```

**Đặc điểm:**
- ✅ Dùng Session Pooler: `aws-1-<region>.pooler.supabase.com`
- ✅ Port: 5432 (Transaction Mode)
- ✅ Pooling=true
- ✅ SSL Mode=Require
- ✅ Trust Server Certificate=true
- ❌ KHÔNG dùng Direct Connection (`db.<project>.supabase.co`)

### 2. Package Versions (.NET 8)
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
```
✅ Đúng .NET 8 compatible (không dùng 10.x)

### 3. Program.cs Configuration
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Enable retry on failure
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        
        // Command timeout
        npgsqlOptions.CommandTimeout(30);
    });
    
    // Development logging
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

### 4. AuthService - Async/Await Pattern
- ✅ Inject AppDbContext (không dùng static field)
- ✅ Tất cả methods đều async
- ✅ Dùng AsNoTracking() cho read-only queries
- ✅ SaveChangesAsync() cho update operations
- ✅ FirstOrDefaultAsync / SingleOrDefaultAsync

### 5. Controllers - Async Actions
- ✅ AuthController: Login, ChangePassword → async
- ✅ UsersController: GetAll, Create, Update, Delete → async
- ✅ ProfileController: UpdateProfile → async
- ✅ RolesController: GetAll, Create, Update, Delete → async

### 6. Nghiệp Vụ Không Đổi
- ✅ Admin tạo user → DefaultPassword123
- ✅ IsFirstLogin = true → requirePasswordChange
- ✅ Bắt buộc đổi password trước khi dùng API khác
- ✅ 4 role cố định: Admin, Manager, Reviewer, Annotator
- ✅ Routes không đổi

---

## 🔧 CẤU HÌNH SUPABASE

### Bước 1: Lấy Connection String từ Supabase

1. Vào Supabase Dashboard → Project Settings → Database
2. Chọn tab **"Connection pooling"**
3. Copy **"Session pooler"** connection string
4. Format: `User Id=postgres.xxx;Password=xxx;Server=aws-1-<region>.pooler.supabase.com;Port=5432;Database=postgres`

### Bước 2: Cập nhật appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=postgres.<YOUR_PROJECT_REF>;Password=<YOUR_PASSWORD>;Server=aws-1-<region>.pooler.supabase.com;Port=5432;Database=postgres;Pooling=true;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Jwt": {
    "Key": "LOCAL_DEV_SECRET_KEY_AT_LEAST_32_CHARS_123456789",
    "Issuer": "DataLabelAPI",
    "Audience": "DataLabelClient",
    "ExpireMinutes": "1440"
  },
  "DefaultPassword": "DefaultPassword123"
}
```

### Bước 3: Seed Data trong Database (SQL)

Chạy trong Supabase SQL Editor:

```sql
-- Insert 4 fixed roles
INSERT INTO public."Role" ("RoleId", "RoleName")
VALUES 
  ('11111111-1111-1111-1111-111111111111', 'Admin'),
  ('22222222-2222-2222-2222-222222222222', 'Manager'),
  ('33333333-3333-3333-3333-333333333333', 'Reviewer'),
  ('44444444-4444-4444-4444-444444444444', 'Annotator')
ON CONFLICT ("RoleId") DO NOTHING;

-- Insert admin user (password hash for "Admin@123")
INSERT INTO public."User" (
  "UserId", 
  "Username", 
  "PasswordHash", 
  "DisplayName", 
  "Email", 
  "RoleId", 
  "IsActive", 
  "IsFirstLogin", 
  "CreatedAt"
)
VALUES (
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  'admin',
  'iyT8vFZvpCDMM9jLZZXLPfWZMc2IJ8dPaX9y/lj5x4s=',
  'System Administrator',
  'admin@datalabel.com',
  '11111111-1111-1111-1111-111111111111',
  true,
  false,
  NOW()
)
ON CONFLICT ("UserId") DO NOTHING;
```

---

## 🧪 TEST SCENARIOS

### Test 1: Kiểm tra Connection
```bash
dotnet run
```

**Kỳ vọng:** Không có lỗi:
- ❌ SocketException
- ❌ Tenant or user not found
- ❌ Failed to resolve host

### Test 2: Login Admin (Swagger)

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "usernameOrEmail": "admin",
  "password": "Admin@123"
}
```

**Kỳ vọng:** 200 OK
```json
{
  "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "username": "admin",
  "roleName": "Admin",
  "token": "eyJhbG...",
  "expiresAt": "2026-02-01T...",
  "message": "Login successful",
  "requirePasswordChange": false
}
```

### Test 3: Tạo User Mới

**Endpoint:** `POST /api/users` (Authorize với admin token)

**Request:**
```json
{
  "username": "testuser",
  "displayName": "Test User",
  "email": "test@example.com",
  "phoneNumber": "0123456789",
  "roleId": "33333333-3333-3333-3333-333333333333"
}
```

**Kỳ vọng:** 201 Created
- Password tự động = "DefaultPassword123"
- IsFirstLogin = true

### Test 4: Login User Mới

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "usernameOrEmail": "testuser",
  "password": "DefaultPassword123"
}
```

**Kỳ vọng:** 200 OK
```json
{
  "requirePasswordChange": true,
  "message": "Login successful. You must change your password before accessing other features."
}
```

### Test 5: Đổi Password

**Endpoint:** `POST /api/auth/change-password` (Authorize với testuser token)

**Request:**
```json
{
  "oldPassword": "DefaultPassword123",
  "newPassword": "NewPassword123"
}
```

**Kỳ vọng:** 200 OK
- IsFirstLogin → false
- Có thể truy cập API khác

---

## ⚠️ TROUBLESHOOTING

### Lỗi: "Failed to resolve host"
**Nguyên nhân:** Dùng sai host
**Giải pháp:** Kiểm tra lại connection string, phải dùng `aws-1-<region>.pooler.supabase.com`

### Lỗi: "Tenant or user not found"
**Nguyên nhân:** Sai User Id hoặc Password
**Giải pháp:** Copy đúng credentials từ Supabase Dashboard

### Lỗi: "Connection refused on port 5432"
**Nguyên nhân:** Network/Firewall
**Giải pháp:** Kiểm tra firewall, hoặc thử port 6543 (Session Mode)

### Lỗi: "Role not found"
**Nguyên nhân:** Chưa seed role trong DB
**Giải pháp:** Chạy SQL insert roles ở Bước 3

---

## 📊 PERFORMANCE TIPS

1. **AsNoTracking() cho Read Operations**
   - GetAllUsers(), GetAllRoles(), GetById() đều dùng AsNoTracking()
   - Giảm memory overhead, tăng speed

2. **Connection Pooling**
   - Session Pooler tự quản lý pool
   - MaxPoolSize mặc định: 15 connections

3. **Retry Policy**
   - Tự động retry 3 lần khi transient error
   - Delay tối đa 5 giây

4. **Command Timeout**
   - 30 giây cho mỗi command
   - Đủ cho queries phức tạp

---

## ✅ CHECKLIST HOÀN TẤT

- [x] Connection string đúng format Session Pooler
- [x] Port 5432, Pooling=true
- [x] Package Npgsql 8.0.4 (không dùng 10.x)
- [x] Program.cs: EnableRetryOnFailure
- [x] AuthService: async/await + AsNoTracking()
- [x] Controllers: tất cả async Task<IActionResult>
- [x] DefaultPassword: "DefaultPassword123"
- [x] Nghiệp vụ không đổi
- [x] Routes không đổi
- [x] Build succeeded
- [ ] Test login admin thành công
- [ ] Test create user thành công
- [ ] Test login user mới thành công

**PROJECT SẴN SÀNG CONNECT VỚI SUPABASE!** 🚀
