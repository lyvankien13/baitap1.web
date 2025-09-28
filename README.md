# Lý Văn Kiên - K225480106101
# K58ktp - Môn: Phát triển ứng dụng trên nền web
# BÀI TẬP VỀ NHÀ 01:
## TẠO SOLUTION GỒM CÁC PROJECT SAU:
1. DLL đa năng, keyword: c# window library -> Class Library (.NET Framework) bắt buộc sử dụng .NET Framework 2.0: giải bài toán bất kỳ, độc lạ càng tốt, phải có dấu ấn cá nhân trong kết quả, biên dịch ra DLL. DLL độc lập vì nó ko nhập, ko xuất, nó nhận input truyền vào thuộc tính của nó, và trả về dữ liệu thông qua thuộc tính khác, hoặc thông qua giá trị trả về của hàm. Nó độc lập thì sẽ sử dụng được trên app dạng console (giao diện dòng lệnh - đen sì), cũng sử dụng được trên app desktop (dạng cửa sổ), và cũng sử dụng được trên web form (web chạy qua iis).

2. Console app, bắt buộc sử dụng .NET Framework 2.0, sử dụng được DLL trên: nhập được input, gọi DLL, hiển thị kết quả, phải có dấu án cá nhân. keyword: c# window Console => Console App (.NET Framework), biên dịch ra EXE

3. Windows Form Application, bắt buộc sử dụng .NET Framework 2.0**, sử dụng được DLL đa năng trên, kéo các control vào để có thể lấy đc input, gọi DLL truyền input để lấy đc kq, hiển thị kq ra window form, phải có dấu án cá nhân; keyword: c# window Desktop => Windows Form Application (.NET Framework), biên dịch ra EXE
  
4. Web đơn giản, bắt buộc sử dụng .NET Framework 2.0, sử dụng web server là IIS, dùng file hosts để tự tạo domain, gắn domain này vào iis, file index.html có sử dụng html css js để xây dựng giao diện nhập được các input cho bài toán, dùng mã js để tiền xử lý dữ liệu, js để gửi lên backend. backend là api.aspx, trong code của api.aspx.cs thì lấy được các input mà js gửi lên, rồi sử dụng được DLL đa năng trên. kết quả gửi lại json cho client, js phía client sẽ nhận được json này hậu xử lý để thay đổi giao diện theo dữ liệu nhận dược, phải có dấu án cá nhân. keyword: c# window web => ASP.NET Web Application (.NET Framework) + tham khảo link chatgpt thầy gửi. project web này biên dịch ra DLL, phải kết hợp với IIS mới chạy được.


**Các bước thực hiện:**
<br>
**Bước 1:Mở VS studio**
 
+ tạo 1 project mới<br>
  <img width="558" height="206" alt="image" src="https://github.com/user-attachments/assets/c0292ff9-22ea-4ec5-81cc-3fce4031e35b" />

+ tạo sẵn 1 tệp tên BI-A<br>
+ <img width="898" height="105" alt="image" src="https://github.com/user-attachments/assets/fbf4d907-0423-48ce-a71a-91909b69de7c" />


**Bước 2:Tạo Solution và 4 Project trong Visual Studio**

- Mở Visual Studio → File → New → Project... → tạo Solution.
- Trong Solution Explorer → click phải Solution → Add → New Project… →
- thêm lần lượt:

- Class Library (.NET Framework).

- Console Application (.NET Framework).

- Windows Forms Application (.NET Framework).

- ASP.NET Web Application (.NET Framework).<br>
 + <img width="580" height="431" alt="image" src="https://github.com/user-attachments/assets/65418576-ea56-4123-9105-42b7b0b4c675" />

 
- Trong Solution, tạo 4 project (lần lượt):

  + Class Library → Đặt tên ClassLibrary1→chọn tệp BI-A → Target framework: .NET Framework 2.0 → chọn creat.<br>
  + <img width="1261" height="762" alt="image" src="https://github.com/user-attachments/assets/b7a27888-b046-4006-a5ac-d70f1115dd97" />


  + Console Application → Đặt tên ConsoleApp1 → chọn tệp BI-A → Target .NET 2.0. → chọn creat.<br>
  + <img width="1272" height="838" alt="image" src="https://github.com/user-attachments/assets/337e040b-f6bb-45f4-95e8-af93c8e97617" />


  + Windows Forms Application → Đặt tên WindowsformApp1 → chọn tệp BI-A → Target .NET 2.0 → chọn creat.<br>
  + <img width="1252" height="806" alt="image" src="https://github.com/user-attachments/assets/e1254823-78de-4678-a49c-84716a6f1a49" />


  + ASP.NET Web Application → Đặt tên WebApplicaton1 → chọn tệp BI-A → Target .NET 2.0 → chọn creat.<br><br>
  <img width="1325" height="823" alt="image" src="https://github.com/user-attachments/assets/17b12435-cb93-4140-a02b-7fa9ce10c2bc" />
  
- Thêm tham chiếu (Add Reference)

+ Trong ConsoleApp1, WindowsFormApp1, WebApplication1 → Add Reference… → chọn Project → tích vào Class Library1 → Ok.
+ <img width="1301" height="907" alt="image" src="https://github.com/user-attachments/assets/9efa74aa-e20a-4f03-a90e-1fdf99d4a621" /><img width="971" height="695" alt="image" src="https://github.com/user-attachments/assets/fdd1032d-b717-4efa-8d0f-0e6da235e2f2" />



== Như vậy 3 project này sẽ dùng được DLL đa năng
  
**Bước 3:**
+ web form tạo 1 file api.aspx.cs và index.html<br>
+ <img width="420" height="123" alt="image" src="https://github.com/user-attachments/assets/a1a60f7e-0b15-4ef4-a7fb-6f40e72a37b2" />

  
+ code xử lí api cho api.aspx.cs và code game cho file index.html<br>
+ <img width="1633" height="579" alt="image" src="https://github.com/user-attachments/assets/7659d7f0-bff3-430d-bfb4-3f7642f84290" />
<br>
<img width="1695" height="536" alt="image" src="https://github.com/user-attachments/assets/ce298a3e-c672-412b-be08-5e3cd02ea59a" />

  
+ tạo 1 tệp mới và publish webform vào tệp đó<br>
  
+ <img width="1221" height="611" alt="image" src="https://github.com/user-attachments/assets/76589b4c-bf0d-43cc-ab1d-e8731d9ea30d" />


**Bước 4:**
+ vào iis tạo 1 website mới với tên miền là pool.com và chọn vị trí tệp vừa publish webform vào
  
+ <img width="1296" height="871" alt="image" src="https://github.com/user-attachments/assets/204e51dc-b007-4be1-b063-6f6dab4c0201" />

  
+ vào nodepad chạy quyền admin, mở file hosts theo đường dẫn:C:\Windows\System32\drivers\etc\hosts
  
+ thêm dòng: 127.0.0.1   pool.com vào cuối trang
  
+ lưu lại và lên trình duyệt máy gõ domain pool.com ta được kết quả
+ <img width="826" height="492" alt="image" src="https://github.com/user-attachments/assets/eb06be85-ab63-4075-8ab5-0f1efc37b741" />

- Kết quả chạy console app:
 <img width="1551" height="801" alt="image" src="https://github.com/user-attachments/assets/4807137a-50e5-4ed7-9f5e-17a6d89096eb" />
- Kết quả chạy WebFormsApp:
  <img width="1536" height="949" alt="image" src="https://github.com/user-attachments/assets/ca5ffa37-a9fd-4cba-a8d1-3608500e8f1b" />
 - Kết quả chạy Web Application:
 - <img width="1859" height="962" alt="image" src="https://github.com/user-attachments/assets/e7fd55b6-77ed-4890-81e3-1be7fde284c2" />





 

