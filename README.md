# Shopping Cart Backend API (.NET Core & MongoDB)

ระบบ Backend API สำหรับจัดการแคตตาล็อกสินค้าและระบบตระกร้าสินค้า (Shopping Cart System) พัฒนาด้วย **ASP.NET Core (.NET 10) C#** พร้อมเชื่อมต่อฐานข้อมูล **MongoDB** แบบ NoSQL

---

## 🛠️ เทคโนโลยีที่ใช้ (Tech Stack)

- **Framework**: .NET 10 (ASP.NET Core Web API)
- **Database**: MongoDB 7.0 (NoSQL)
- **Database Driver**: `MongoDB.Driver` v3.12.0
- **API Documentation**: Swagger / OpenAPI (`Swashbuckle.AspNetCore` v10.3.1)
- **Architecture**: Controller-Service-Repository Pattern พร้อม DTOs และ Response Wrapper

---

## 📂 โครงสร้างโปรเจกต์ (Project Structure)

```text
backendShoppingcart/
├── Controllers/
│   ├── ProductsController.cs       # API จัดการสินค้า (CRUD, Seed data)
│   └── CartController.cs           # API จัดการตระกร้าสินค้า (Add, Update, Remove, Clear, Checkout)
├── Services/
│   ├── IProductService.cs & ProductService.cs   # Business Logic สินค้าและสต็อก
│   └── ICartService.cs & CartService.cs         # Business Logic ตระกร้าและ Checkout
├── Models/
│   ├── Product.cs                  # Entity สินค้าใน MongoDB
│   ├── Cart.cs                     # Entity ตระกร้าสินค้า
│   ├── CartItem.cs                 # Entity รายการสินค้าในตระกร้า
│   └── MongoDbSettings.cs          # Model คอนฟิก MongoDB
├── DTOs/
│   ├── ProductDtos.cs              # CreateProductDto, UpdateProductDto
│   ├── CartDtos.cs                 # AddToCartDto, UpdateCartItemDto, CheckoutResultDto
│   └── ApiResponse.cs              # Wrapper รูปแบบ Response มาตรฐาน
├── appsettings.json                # Connection string และชื่อคอลเลกชัน MongoDB
└── Program.cs                      # การลงทะเบียน DI, CORS, Swagger, MongoDB
```

---

## 🗄️ การตั้งค่าฐานข้อมูล MongoDB (`appsettings.json`)

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://satoshiro56:Ouccw8556@localhost:27017/?authSource=admin",
    "DatabaseName": "ShoppingCartDb",
    "ProductsCollectionName": "Products",
    "CartsCollectionName": "Carts"
  }
}
```

> **หมายเหตุ**: หากรัน MongoDB ผ่าน Docker ในเครื่อง สามารถสตาร์ทคอนเทนเนอร์ด้วยคำสั่ง:
> ```bash
> docker start local-mongodb local-mongo-express
> ```
> และสามารถเข้าดูข้อมูลใน MongoDB ผ่านเว็บ GUI (Mongo Express) ได้ที่: `http://localhost:8081`

---

## 🚀 วิธีการรันโปรเจกต์ (Getting Started)

1. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

2. **Build โปรเจกต์**:
   ```bash
   dotnet build
   ```

3. **รันโปรเจกต์**:
   ```bash
   dotnet run --launch-profile http
   ```

4. **เปิดดูเอกสารและทดสอบ API ผ่าน Swagger UI**:
   - เปิดบราวเซอร์ไปที่: [http://localhost:5131/swagger](http://localhost:5131/swagger) หรือ [http://localhost:5131](http://localhost:5131)

---

## 📡 รายละเอียด API Endpoints

### 1. หมวดหมู่สินค้า (Products API)

| Method | Endpoint | รายละเอียด |
| :--- | :--- | :--- |
| `GET` | `/api/Products` | ดึงรายการสินค้าทั้งหมด (รองรับ Query `?category=...`) |
| `GET` | `/api/Products/{id}` | ดึงข้อมูลสินค้าตาม ID |
| `POST` | `/api/Products` | เพิ่มสินค้าใหม่ |
| `PUT` | `/api/Products/{id}` | แก้ไขข้อมูลสินค้า |
| `DELETE` | `/api/Products/{id}` | ลบสินค้า |
| `POST` | `/api/Products/seed` | สุ่ม/เตรียมข้อมูลสินค้าตัวอย่าง (MacBook, iPhone, Sony Headphones ฯลฯ) |

### 2. หมวดหมู่ตระกร้าสินค้า (Cart API)

> **หมายเหตุ**: ไม่จำเป็นต้องระบุ `userId` ในการเรียกใช้งานระบบตระกร้า หากไม่ระบุระบบจะใช้ค่าเริ่มต้น (`default-user`) ให้อัตโนมัติ (และยังคงรองรับการส่ง `userId` ผ่าน Route หรือ Query Parameter เช่น `?userId=...` ได้เช่นกัน) ทั้งยังรองรับทั้ง `/api/Cart/...` และ `/Cart/...`

| Method | Endpoint | รายละเอียด |
| :--- | :--- | :--- |
| `GET` | `/api/Cart` | ดูข้อมูลตระกร้าสินค้า (สร้างให้อัตโนมัติหากยังไม่มี) |
| `POST` | `/api/Cart/items` | เพิ่มสินค้าลงในตระกร้า (ตรวจสอบสต็อกและรวมจำนวนอัตโนมัติ) |
| `PUT` / `PATCH` | `/api/Cart/items/{productId}` | ปรับปรุงจำนวนสินค้า (ถ้าใส่ `0` จะเป็นการลบออก) |
| `DELETE` | `/api/Cart/items/{productId}` | ลบสินค้าชิ้นนั้นออกจากตระกร้า |
| `DELETE` | `/api/Cart` | ล้างสินค้าทั้งหมดออกจากตระกร้า (Clear Cart) |
| `POST` | `/api/Cart/checkout` | เช็คเอาท์และสั่งซื้อ (ตรวจสอบสต็อก, ตัดสต็อกจริงใน MongoDB, ล้างตระกร้า) |

---

## 💡 ตัวอย่างการเรียกใช้งาน API (Example Usage)

### 1. เพิ่มสินค้าลงตระกร้า (`POST /api/Cart/items`)
**Request Body**:
```json
{
  "productId": "6ad6ddaec84d53564572ba05",
  "quantity": 2
}
```

**Response**:
```json
{
  "success": true,
  "message": "เพิ่มสินค้าลงในตระกร้าสำเร็จ",
  "data": {
    "id": "6ad6ddb4c84d53564572ba09",
    "userId": "default-user",
    "items": [
      {
        "productId": "6ad6ddaec84d53564572ba05",
        "productName": "iPhone 16 Pro Max 256GB",
        "unitPrice": 48900,
        "quantity": 2,
        "subTotal": 97800,
        "imageUrl": "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=500"
      }
    ],
    "totalItems": 2,
    "totalPrice": 97800,
    "updatedAt": "2026-10-20T03:19:22.335Z"
  },
  "errors": null
}
```

### 2. เช็คเอาท์ (`POST /api/Cart/checkout`)
**Response**:
```json
{
  "success": true,
  "message": "การเช็คเอาท์สำเร็จ",
  "data": {
    "orderId": "ORD-25691020031941-333250",
    "userId": "user123",
    "items": [
      {
        "productId": "6ad6ddaec84d53564572ba05",
        "productName": "iPhone 16 Pro Max 256GB",
        "unitPrice": 48900,
        "quantity": 2,
        "subTotal": 97800
      }
    ],
    "totalItems": 2,
    "totalPrice": 97800,
    "checkoutAt": "2026-10-20T03:19:41.984Z",
    "status": "Completed",
    "message": "ทำการสั่งซื้อและตัดสต็อกสินค้าเรียบร้อยแล้ว"
  },
  "errors": null
}
```
"# Test_Backend_ASP.NetCore" 
