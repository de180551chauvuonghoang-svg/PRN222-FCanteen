# FCanteen.KitchenServer — May chu bep (YC2)

Console App dong vai tro may chu bep, lang nghe ket noi TCP tu cac quay thu ngan.

---

## Chuc nang

- Lang nghe TCP tren **cong 9500**
- Xu ly nhieu quay dong thoi (moi ket noi mot Task rieng)
- Nhan phieu order dang JSON, luu vao database trong Transaction
- **Tu tinh lai tong tien** (khong tin so lieu tu client)
- Gui xac nhan ve quay (ma phieu + tong tien server tinh)
- Ghi `DeviceLog` moi ket noi vao/ra
- Hien thi danh sach phieu dang cho che bien

---

## Cau hinh

File `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "FCanteenDb": "Server=DESKTOP-IJV2BTH\\SQLEXPRESS;Database=PRN222_Lab_FCanteen;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Chay project

### Trong Visual Studio
1. Chuot phai `FCanteen.KitchenServer` → **Set as Startup Project**
2. Nhan **F5** hoac **Ctrl+F5**

### Trong Terminal
```powershell
cd C:\Users\RinHeo\Desktop\PRN222-FCanteen\FCanteen
dotnet run --project FCanteen.KitchenServer
```

### Chay lan thu hai tro di (khong build lai)
```powershell
dotnet run --project FCanteen.KitchenServer --no-build
```

---

## Giao thuc tuyen doi

| Huong | Dinh dang | Vi du |
|-------|-----------|-------|
| Client → Server | JSON `OrderDto` | `{"StationName":"QUAY01","Lines":[...]}` |
| Server → Client | JSON `OrderConfirmDto` | `{"OrderTicketId":1,"TotalAmount":70000,"Message":"..."}` |

---

## Output mau

```
[SERVER] Kitchen Server dang lang nghe tren cong 9500...
============================================
     KITCHEN SERVER — CONG 9500
============================================
  >> Phieu #1 | QUAY01 | 70,000 VND
  >> Phieu #2 | QUAY02 | 45,000 VND
============================================
[+] Ket noi tu: 127.0.0.1:54321
[<] Nhan du lieu: {"StationName":"QUAY01",...}
[OK] Luu phieu #3 thanh cong. Tong: 35,000 VND
[>] Gui xac nhan: {"OrderTicketId":3,...}
[-] Dong ket noi: 127.0.0.1:54321
```

---

## Luu y

> Server phai chay TRUOC khi khoi dong bat ky PosClient nao.