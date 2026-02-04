# 🏷️ DataLabel – Data Labeling Support System

Hệ thống hỗ trợ gán nhãn dữ liệu phục vụ huấn luyện mô hình Machine Learning / AI.  
Dự án được phát triển trong khuôn khổ môn học **SWP391 – Software Project**.

---

## 📌 Tổng quan đề tài
**DataLabel** là một nền tảng web full-stack giúp tổ chức, quản lý và thực hiện quy trình **gán nhãn dữ liệu** một cách có kiểm soát, minh bạch và dễ mở rộng.

Hệ thống hỗ trợ nhiều vai trò người dùng, cho phép:
- Phân công công việc gán nhãn
- Kiểm duyệt chất lượng nhãn
- Quản lý dataset, project, label
- Xuất dữ liệu theo định dạng phục vụ huấn luyện mô hình AI

---

## 🎓 Thông tin môn học
- **Môn học:** SWP391 – Software Project
- **Học kỳ:** FALL 2025
- **Loại dự án:** Full-stack Web Application

---

## 👥 Vai trò người dùng

### 🔐 Admin
- Quản lý người dùng
- Tạo và quản lý vai trò (Role)
- Gán vai trò cho tài khoản
- Cấu hình hệ thống
- Theo dõi nhật ký hoạt động

### 📋 Manager
- Tạo và quản lý Project
- Quản lý Dataset
- Tạo Category, Label Set, Guideline
- Phân công task gán nhãn cho Annotator
- Theo dõi tiến độ và chất lượng gán nhãn
- Xuất dữ liệu đã được duyệt

### ✍️ Annotator
- Nhận task gán nhãn
- Xem guideline và label set
- Thực hiện gán nhãn dữ liệu
- Lưu nháp hoặc gửi kết quả để kiểm duyệt
- Chỉnh sửa nhãn theo phản hồi

### 🔍 Reviewer
- Nhận các annotation cần kiểm duyệt
- Đánh giá chất lượng nhãn
- Phê duyệt hoặc trả về làm lại
- Ghi nhận lỗi theo danh mục

---

## ✨ Chức năng chính của hệ thống

### 👤 Quản lý người dùng & phân quyền
- CRUD User
- CRUD Role
- Gán role cho tài khoản
- Kiểm soát truy cập theo vai trò

### 📁 Quản lý Project & Dataset
- Tạo và quản lý Project
- Phân loại Project theo Category
- Quản lý Dataset theo từng Project
- Gắn Label Set cho Dataset

### 🏷️ Gán nhãn dữ liệu
- Tạo Label Set & Label
- Quản lý Guideline gán nhãn
- Phân công task gán nhãn
- Lưu nháp và nộp kết quả

### ✅ Kiểm duyệt & đánh giá
- Reviewer kiểm tra annotation
- Phê duyệt hoặc yêu cầu chỉnh sửa
- Theo dõi mức độ đồng thuận (consensus)
- Ghi nhận lỗi gán nhãn


### 🤖 Hỗ trợ AI (định hướng)
- Gợi ý nhãn ban đầu cho Annotator
- Hỗ trợ tăng tốc quá trình gán nhãn

---

## 🏗️ Kiến trúc tổng thể

- **Frontend:** Web UI cho từng vai trò (Admin / Manager / Annotator / Reviewer)
- **Backend:** RESTful API xử lý nghiệp vụ & phân quyền
- **Database:** Lưu trữ người dùng, project, dataset, annotation
- **Swagger:** Tài liệu API
- **AI Module (future):** Gợi ý nhãn

---

## 🛠️ Công nghệ sử dụng (dự kiến)

### Backend
- ASP.NET Core Web API (C#)
- Swagger (OpenAPI)
- Entity Framework Core
- PostgreSQL / SQL Server

---

## ⚙️ Configuration & Setup

### Local Development
1. Clone repository
2. Ensure `appsettings.Development.json` exists (already gitignored)
3. The Development file contains the real JWT secret key
4. Run the project - it will automatically use Development configuration

### Important Files
- **appsettings.json**: Template configuration (safe for GitHub, contains placeholder values)
- **appsettings.Development.json**: Local secrets (gitignored, contains real JWT key)
- **.gitignore**: Already configured to exclude Development config

### Configuration Flow
ASP.NET Core automatically merges configurations in this order:
1. `appsettings.json` (base template)
2. `appsettings.Development.json` (overrides for Development environment)
3. Environment variables (if needed)

The real JWT key in Development config overrides the placeholder in base config.

--- 

## 🔜 Hướng phát triển
- Áp dụng JWT Authentication & Authorization
- Tích hợp database đầy đủ
- Thống kê & báo cáo chất lượng gán nhãn
- Hoàn thiện AI hỗ trợ gán nhãn
- Triển khai hệ thống hoàn chỉnh
