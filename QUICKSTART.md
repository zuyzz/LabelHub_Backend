# 🚀 QUICK START - 5 BƯỚC SETUP SUPABASE

## Bước 1: Lấy Connection String

1. Vào **Supabase Dashboard** → Project Settings → Database
2. Chọn tab **"Connection pooling"**
3. Mode: **"Session"**
4. Copy connection string dạng:
   ```
   postgresql://postgres.xxx:[YOUR-PASSWORD]@aws-1-<region>.pooler.supabase.com:5432/postgres
   ```

## Bước 2: Chuyển sang Format .NET

Chuyển từ:
```
postgresql://postgres.xxx:[PASSWORD]@aws-1-ap-northeast-1.pooler.supabase.com:5432/postgres
```

Sang:
```
User Id=postgres.xxx;Password=[PASSWORD];Server=aws-1-ap-northeast-1.pooler.supabase.com;Port=5432;Database=postgres;Pooling=true;SSL Mode=Require;Trust Server Certificate=true
```

## Bước 3: Cập nhật appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=postgres.xxx;Password=YOUR_PASSWORD;Server=aws-1-ap-northeast-1.pooler.supabase.com;Port=5432;Database=postgres;Pooling=true;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

## Bước 4: Seed Data vào Supabase

1. Vào Supabase → **SQL Editor**
2. Copy nội dung file `seed_data.sql`
3. Click **Run**
4. Verify: Có 4 roles và 1-2 users

## Bước 5: Run & Test

```bash
dotnet run
```

Mở Swagger: `https://localhost:7xxx/swagger`

**Test Login:**
```json
POST /api/auth/login
{
  "usernameOrEmail": "admin",
  "password": "Admin@123"
}
```

**Kết quả:** 200 OK + JWT token ✅

---

## ✅ CHECKLIST

- [ ] Copy connection string từ Supabase (Session Pooler)
- [ ] Update appsettings.Development.json
- [ ] Run seed_data.sql trong Supabase SQL Editor
- [ ] dotnet run không lỗi
- [ ] Login admin thành công
- [ ] Tạo user mới thành công

**DONE!** 🎉
