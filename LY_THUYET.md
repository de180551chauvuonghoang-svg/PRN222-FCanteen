# Cau hoi Ly Thuyet — Lab 01

**Mon hoc**: PRN222 — Networking Programming  
**Ho ten**: Chau Vuong Hoang | **Ma SV**: DE180551

---

## Cau 1: Tai sao gui phieu order phai dung TCP, thong bao het mon co the dung UDP?

### Tra loi

#### TCP — Dung cho gui phieu order (OrderTicket)

TCP (Transmission Control Protocol) dam bao **truyen du lieu tin cay** nho co che:
- **Ket noi 3 buoc** (3-way handshake): client va server bac tay truoc khi truyen
- **Xac nhan tung goi tin** (ACK): neu mat goi → tu dong gui lai
- **Dam bao thu tu**: du lieu den dung thu tu da gui
- **Kiem tra loi**: phat hien va xu ly truong hop mat du lieu

**Vi sao phieu order PHAI dung TCP?**

| Tinh huong | Hau qua neu dung UDP |
|-----------|---------------------|
| Goi tin bi mat tren mang | Phieu order bi mat → khach hang khong nhan duoc mon |
| Du lieu den sai thu tu | Server xu ly sai phieu → tinh tien sai |
| Khong co xac nhan | Quay khong biet phieu da den bep hay chua |
| Ket noi bi ngat giua chung | Phieu order luu mot nua vao DB → du lieu hong |

**Ket luan**: Phieu order chua thong tin tai chinh (tong tien, so luong) → sai sot gay thiet hai truc tiep → **bat buoc phai dung TCP**.

---

#### UDP — Dung cho thong bao het mon

UDP (User Datagram Protocol) gui du lieu **nhanh, khong ket noi**, khong dam bao den noi:
- Khong co 3-way handshake → gui ngay lap tuc
- Khong xac nhan → khong biet client co nhan duoc khong
- Ho tro **Broadcast**: 1 goi tin phat toi toan mang cung luc

**Vi sao thong bao het mon CO THE dung UDP?**

| Ly do | Giai thich |
|-------|-----------|
| **Hau qua khong nghiem trong** | Neu PosClient khong nhan duoc → lan sau load menu tu DB van biet mon het hang |
| **Toc do quan trong** | Thong bao can phat nhanh toi tat ca quay dong thoi |
| **Broadcast hieu qua** | 1 goi UDP phat toi 10 quay cung luc, TCP phai ket noi 10 lan rieng |
| **Du lieu nho, don gian** | Chi can: "MonID:TenMon het hang" — khong can toan ven phuc tap |
| **Co co che du phong** | PosClient reload menu tu DB khi can → tu dong cap nhat trang thai |

**Ket luan**: Thong bao het mon chi co tinh chat "thong tin nhanh" → mat mot goi cung khong gay sai so nghiem trong → UDP phu hop vi nhanh va ho tro Broadcast.

---

### So sanh tong quat

| Tieu chi | TCP | UDP |
|---------|-----|-----|
| Do tin cay | ✅ Cao (co ACK, gui lai) | ❌ Khong dam bao |
| Toc do | Cham hon | ✅ Nhanh hon |
| Ket noi | Phai ket noi truoc | Khong can |
| Broadcast | ❌ Khong ho tro | ✅ Ho tro |
| Dung cho | Phieu order, giao dich | Thong bao, stream video, game |

---

## Cau 2: May chu tu tinh tong tien thay vi tin client — tinh huong tan cong cu the?

### Tra loi

He thong FCanteen **khong tin tong tien tu PosClient** — server luon tu doc gia tu DB va tinh lai.

**Vi sao?** Vi client co the bi gia mao (tampered) hoac lap trinh lai.

---

### Tinh huong tan cong cu the

#### Tan cong 1: Man-in-the-Middle (MITM) — Sua goi tin tren duong truyen

```
PosClient                  Ke tan cong                  KitchenServer
    |  {"TotalAmount":70000}  |                               |
    |─────────────────────────► sua thanh 7000 ──────────────► |
    |                          (giam 10 lan!)                  |
```

**Kich ban**: Ke tan cong chen vao giua mang, bat goi TCP va sua `TotalAmount` tu 70,000 xuong 7,000 truoc khi gui den server.

**Neu server tin client**: Luu phieu voi tong tien 7,000 → mat tien  
**He thong FCanteen**: Server tu lay gia tu DB × so luong → tinh ra 70,000 → phieu dung

---

#### Tan cong 2: Gian lan tu nguoi dung (Client Tampering)

```csharp
// Ke gian lan sua code PosClient:
var order = new OrderDto {
    StationName = "QUAY01",
    Lines = new() {
        new OrderLineDto { MenuItemId = 1, Quantity = 10, UnitPrice = 1 } // gia 1 VND!
    },
    TotalAmount = 10 // tu tinh sai
};
```

**Kich ban**: Nhan vien sua code PosClient, tu dat `UnitPrice = 1` de mua 10 mon chi ton 10 VND.

**Neu server tin client**: Luu phieu 10 VND → thiet hai  
**He thong FCanteen**: Server doc `menuItem.Price = 35000` tu DB → tinh `35000 × 10 = 350,000` → dung

---

#### Tan cong 3: Replay Attack — Gui lai phieu cu

**Kich ban**: Chup lai goi tin TCP cua 1 phieu thanh toan thanh cong, gui lai nhieu lan voi tong tien thap.

**Neu server tin client**: Chap nhan nhieu phieu trung lap voi gia cu  
**He thong FCanteen**: Moi phieu server tu tinh lai tu DB → gia luon la gia hien tai, khong the dung gia cu

---

### Nguyen tac bao mat: "Never Trust the Client"

> **"Khong bao gio tin du lieu tu phia client doi voi cac thong tin co gia tri kinh te."**

He thong FCanteen ap dung:
- Client chi gui: `MenuItemId` + `Quantity` + `Note` (khong co gia)  
- Server tu lay: `Price` tu database (nguon duy nhat dang tin cay)
- Server tu tinh: `TotalAmount = SUM(Price × Quantity)` — khong chap nhan so client gui

**Ngoai ra nen bo sung** (de hoan thien bao mat):
- Xac thuc JWT token cho moi ket noi TCP
- Ghi log moi phieu voi thong tin quay + IP nguon
- Canh bao neu so luong trong 1 phieu bat thuong (> 50 mon/loai)

---

*Tra loi duoc trich xuat tu thiet ke thuc te cua he thong FCanteen - Lab 01 PRN222*