# DataLabel Backend  
_Data Labeling Support System – Backend Service_

---

## 📌 Tổng quan
**DataLabel Backend** là hệ thống backend phục vụ cho nền tảng gán nhãn dữ liệu (Data Labeling Platform).  
Backend chịu trách nhiệm xử lý nghiệp vụ, quản lý dữ liệu, phân quyền người dùng và cung cấp API cho Frontend thông qua RESTful APIs.

Dự án được xây dựng cho **mục đích nội bộ**, tập trung vào việc chuẩn hóa luồng nghiệp vụ gán nhãn dữ liệu và sẵn sàng mở rộng trong các giai đoạn tiếp theo.

---

## 🎯 Mục tiêu
- Cung cấp API cho hệ thống gán nhãn dữ liệu
- Quản lý người dùng và phân quyền theo vai trò
- Quản lý dự án gán nhãn, bộ dữ liệu và bộ nhãn
- Xử lý luồng gán nhãn – kiểm duyệt – phê duyệt
- Hỗ trợ xuất dữ liệu đã được duyệt
- Chuẩn bị nền tảng cho AI hỗ trợ gán nhãn trong tương lai

---

## 👥 Vai trò hệ thống
- **Admin**: Quản lý người dùng, cấu hình hệ thống, nhật ký hoạt động  
- **Manager**: Quản lý dự án gán nhãn, bộ dữ liệu, phân công công việc  
- **Annotator**: Thực hiện gán nhãn dữ liệu theo nhiệm vụ được giao  
- **Reviewer**: Kiểm duyệt, phê duyệt hoặc trả về kết quả gán nhãn  

---

## 🧩 Kiến trúc tổng quan
