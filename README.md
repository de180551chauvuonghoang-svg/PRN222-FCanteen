# PRN222 - FCanteen
**He thong quan ly cang tin FPT University Da Nang**
> Mon hoc: PRN222 — Networking Programming | FPT University Da Nang

---

## Thong tin sinh vien
| | |
|---|---|
| **Ho ten** | Chau Vuong Hoang |
| **Ma SV** | DE180551 |
| **Mon hoc** | PRN222 — Networking Programming |

---

## Cong nghe su dung
- **Framework**: .NET 8
- **ORM**: Entity Framework Core 8 (Code First)
- **Database**: SQL Server Express (`PRN222_Lab_FCanteen`)
- **IDE**: Visual Studio 2022

---

## Quy dinh chung (Lab 00)
- [x] Su dung .NET 8, SQL Server, EF Core Code First
- [x] Moi thay doi schema qua `Add-Migration` va `Update-Database`
- [x] Connection string dat trong `appsettings.json`
- [x] Ma nguon tren GitHub, moi lab mot branch (`lab01`, `lab02`, ...)
- [x] Toi thieu 3 commits moi lab

---

## Lab 01 — Networking Programming

### Tien do
| YC | Mo ta | Diem | Trang thai |
|----|-------|------|-----------|
| **YC1** | Tang du lieu Code First | 1.5 | ✅ Hoan thanh |
| **YC2** | May chu bep bang TcpListener | 2.5 | ✅ Hoan thanh |
| **YC3** | Quay thu ngan bang TcpClient | 2.5 | ✅ Hoan thanh |
| **YC4** | UDP + HttpClient dong bo bang gia | 2.5 | ⬜ Chua lam |
| **LT** | Cau hoi ly thuyet | 1.0 | ⬜ Chua lam |
| | **Tong** | **10** | |

---

## Cau truc Solution

```
FCanteen/
├── FCanteen.Data/              # Class Library - Tang du lieu (YC1)
│   ├── Entities/
│   │   ├── MenuItem.cs
│   │   ├── OrderTicket.cs
│   │   ├── TicketLine.cs
│   │   └── DeviceLog.cs
│   ├── Migrations/
│   ├── FCanteenContext.cs
│   ├── FCanteenContextFactory.cs
│   ├── appsettings.json
│   └── README.md
├── FCanteen.KitchenServer/     # Console App - May chu bep (YC2)
│   ├── Models/OrderDto.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── README.md
└── FCanteen.PosClient/         # Console App - Quay thu ngan (YC3)
    ├── Models/OrderDto.cs
    ├── Program.cs
    ├── appsettings.json
    └── README.md
```

---

## Huong dan chay nhanh

### Yeu cau he thong
- .NET 8 SDK
- SQL Server Express (`DESKTOP-IJV2BTH\SQLEXPRESS`)
- Visual Studio 2022

### Thu tu chay
```
1. Chay FCanteen.KitchenServer (lang nghe cong 9500)
2. Chay FCanteen.PosClient (co the chay nhieu instance)
```

Chi tiet xem README trong tung project.

---

## Lich su commit

| Hash | Mo ta |
|------|-------|
| `b6b2ff3` | lab01: init - add lab documents |
| `b01a561` | lab01: YC1 - EF Core Code First, migration done |
| `03695d7` | lab01: YC2 - KitchenServer TcpListener completed |
| *(sap toi)* | lab01: YC3 - PosClient TcpClient completed |