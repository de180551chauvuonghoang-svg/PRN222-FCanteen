# FCanteen.PosClient — Quay thu ngan (YC3)

Console App dong vai tro quay thu ngan, cho phep nhap order va gui len may chu bep.

---

## Chuc nang

- Doc thuc don tu database, hien thi dang bang co danh so
- Nhap nhieu mon kem so luong va ghi chu
- Hien thi tam tinh truoc khi gui
- Gui phieu len `FCanteen.KitchenServer` qua TCP
- Nhan va hien thi xac nhan tu server (ma phieu + tong tien server tinh)
- Ho tro chay **nhieu instance cung luc** (mo phong nhieu quay)
- Ten quay truyen qua tham so dong lenh

---

## Cau hinh

File `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "FCanteenDb": "Server=DESKTOP-IJV2BTH\\SQLEXPRESS;Database=PRN222_Lab_FCanteen;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "KitchenServer": {
    "Host": "127.0.0.1",
    "Port": 9500
  }
}
```

---

## Chay project

### Dieu kien tien quyet
> **FCanteen.KitchenServer phai dang chay** truoc khi khoi dong PosClient

### Chay mot quay (trong Visual Studio)
1. Chuot phai `FCanteen.PosClient` → **Set as Startup Project**
2. Nhan **F5**

### Chay mot quay (Terminal)
```powershell
cd C:\Users\RinHeo\Desktop\PRN222-FCanteen\FCanteen
dotnet run --project FCanteen.PosClient -- QUAY01
```

### Chay 3 quay cung luc (mo 3 cua so rieng)

**Buoc 1**: Build truoc
```powershell
dotnet build FCanteen.PosClient
```

**Buoc 2**: Chay 3 instance cung luc
```powershell
Start-Process "FCanteen.PosClient\bin\Debug\net8.0\FCanteen.PosClient.exe" -ArgumentList "QUAY01"
Start-Process "FCanteen.PosClient\bin\Debug\net8.0\FCanteen.PosClient.exe" -ArgumentList "QUAY02"
Start-Process "FCanteen.PosClient\bin\Debug\net8.0\FCanteen.PosClient.exe" -ArgumentList "QUAY03"
```

> Su dung `--no-build` de tranh loi file lock khi chay nhieu instance:
> ```powershell
> dotnet run --project FCanteen.PosClient --no-build -- QUAY02
> ```

---

## Huong dan su dung

```
1. Xem danh sach mon an hien thi tren man hinh
2. Nhap so thu tu mon muon chon
3. Nhap so luong
4. Nhap ghi chu (hoac Enter de bo qua)
5. Lap lai de them nhieu mon
6. Go "xong" de ket thuc nhap
7. Xac nhan gui len bep (Y/n)
8. Cho xac nhan tu server
```

---

## Output mau

```
╔══════════════════════════════════════════╗
║     FCANTEEN — QUAY THU NGAN             ║
║     Ten quay: QUAY01                     ║
╚══════════════════════════════════════════╝

  THUC DON
  STT   Ten mon                   Don gia      Don vi
  -------------------------------------------------------
  1     Com ga                     35,000  phan
  2     Bun bo                     40,000  to
  ...

  > So thu tu mon: 1
  > So luong: 2
  > Ghi chu: it cay
  [+] Da them: Com ga x2 = 70,000 VND

  > So thu tu mon: xong

  ===== XAC NHAN TU BEP =====
  Ma phieu : #5
  Tong tien: 70,000 VND
  Thong bao: Phieu #5 da duoc tiep nhan
  ============================
```