# Shopping Cart Backend API (.NET Core & MongoDB)

## เทคโนโลยีที่ใช้ (Tech Stack)

- Framework: .NET 10 (ASP.NET Core Web API)
- Database: MongoDB 7.0 (NoSQL)
- Database Driver: `MongoDB.Driver` v3.12.0
- API Documentation: Swagger / OpenAPI
- Architecture: Controller-Service-Repository Pattern พร้อม DTOs และ Response Wrapper



## การตั้งค่าฐานข้อมูล MongoDB (`appsettings.json`)
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://satoshiro56:Ouccw8556@localhost:27017/?authSource=admin",
    "DatabaseName": "ShoppingCartDb",
    "ProductsCollectionName": "Products",
    "CartsCollectionName": "Carts"
  }
}

# วิธีการรันโปรเจกต์ (Getting Started)

dotnet run 
  
# เอกสารและทดสอบ API ผ่าน Swagger UI:
# http://localhost:5131/swagger

---

## รายละเอียด API Endpoints

### 1. Products API

`GET`  `/api/Products`  ดึงรายการสินค้าทั้งหมด 

`POST`  `/api/Products/seed` เตรียมข้อมูลสินค้าตัวอย่าง (MacBook, iPhone, Sony Headphones ฯลฯ) 

### 2. Cart API

`GET`  `/api/Cart`  ดูข้อมูลตะกร้าสินค้า (สร้างให้อัตโนมัติหากยังไม่มี) 

`POST`  `/api/Cart/items`  เพิ่มสินค้าลงในตะกร้า (ตรวจสอบสต็อกและรวมจำนวนอัตโนมัติ) 

`DELETE`  `/api/Cart/items/{productId}`  ลบสินค้าชิ้นนั้นออกจากตะกร้า 

`DELETE`  `/api/Cart` | ล้างสินค้าทั้งหมดออกจากตะกร้า (Clear Cart) 

`POST`  `/api/Cart/checkout`  เช็คเอาท์และสั่งซื้อ (ตรวจสอบสต็อก, ตัดสต็อกจริงใน MongoDB, ล้างตะกร้า) 

##  ตัวอย่างการเรียกใช้งาน API (Example Usage)

## 1. เพิ่มสินค้าลงตะกร้า (`POST /api/Cart/items`)
# Request Body:
```json
{
  "productId": "6ad6ddaec84d53564572ba05",
  "quantity": 1
}
```

# Response:
```json
{
  "success": true,
  "message": "เพิ่มสินค้าลงในตะกร้าสำเร็จ",
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
# Response:
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

