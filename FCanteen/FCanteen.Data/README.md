# FCanteen.Data — Tang du lieu (YC1)

Class Library chua toan bo Entity, DbContext va Migration cho he thong FCanteen.

---

## Cac Entity

| Entity | Mo ta |
|--------|-------|
| `MenuItem` | Mon an: ma, ten, gia, don vi, trang thai con ban |
| `OrderTicket` | Phieu order: ma quay, tong tien, thoi gian, trang thai |
| `TicketLine` | Dong phieu: FK toi OrderTicket & MenuItem, so luong, gia luc ban, ghi chu |
| `DeviceLog` | Log thiet bi: giao thuc, IP nguon, noi dung, thoi gian |

> **Luu y**: `TicketLine.UnitPrice` luu gia tai thoi diem ban, KHONG doc lai tu MenuItem

---

## Seed Data

15 mon an duoc seed san bang `HasData` trong `OnModelCreating`.

---

## Cau hinh ket noi

File `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "FCanteenDb": "Server=DESKTOP-IJV2BTH\\SQLEXPRESS;Database=PRN222_Lab_FCanteen;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Chay Migration

Mo **Package Manager Console** trong Visual Studio:

```powershell
# Tao migration moi
Add-Migration <TenMigration>

# Cap nhat database
Update-Database
```

> Default project phai chon la `FCanteen.Data`

---

## Ket noi Database

```
Server   : DESKTOP-IJV2BTH\SQLEXPRESS
Database : PRN222_Lab_FCanteen
Auth     : Windows Authentication
```