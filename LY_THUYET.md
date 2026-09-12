# Câu hỏi Lý Thuyết — Lab 01

**Môn học**: PRN222 — Networking Programming
**Họ tên**: Châu Vương Hoàng | **Mã SV**: DE180551

---

## Câu 1: Tại sao gửi phiếu order phải dùng TCP, thông báo hết món có thể dùng UDP?

### Trả lời

#### TCP — Dùng cho gửi phiếu order

TCP (Transmission Control Protocol) đảm bảo **truyền dữ liệu tin cậy** nhờ cơ chế:
- **Kết nối 3 bước** (3-way handshake): client và server bắt tay trước khi truyền
- **Xác nhận từng gói tin** (ACK): nếu mất gói → tự động gửi lại
- **Đảm bảo thứ tự**: dữ liệu đến đúng thứ tự đã gửi

**Tại sao phiếu order PHẢI dùng TCP?**

| Tình huống | Hậu quả nếu dùng UDP |
|-----------|---------------------|
| Gói tin bị mất | Phiếu order mất → khách hàng không nhận được món |
| Dữ liệu sai thứ tự | Server tính tiền sai |
| Không có xác nhận | Quầy không biết phiếu đã đến bếp hay chưa |
| Kết nối đứt giữa chừng | Lưu một nửa vào DB → dữ liệu hỏng |

**Kết luận**: Phiếu order chứa thông tin tài chính → sai sót gây thiệt hại trực tiếp → **bắt buộc dùng TCP**.

---

#### UDP — Dùng cho thông báo hết món

| Lý do | Giải thích |
|-------|-----------|
| Hậu quả không nghiêm trọng | Nếu PosClient không nhận được → load lại menu từ DB vẫn biết |
| Tốc độ quan trọng | Phát nhanh tới tất cả quầy đồng thời |
| Broadcast hiệu quả | 1 gói UDP → 10 quầy cùng lúc; TCP phải kết nối 10 lần riêng |
| Dữ liệu nhỏ, đơn giản | Chỉ cần: ID:Tên hết hàng — không cần phức tạp |

**Kết luận**: Thông báo hết món chỉ là thông tin nhanh → mất một gói không gây sai sót → UDP phù hợp vì nhanh và hỗ trợ Broadcast.

---

### So sánh tổng quát

| Tiêu chí | TCP | UDP |
|---------|-----|-----|
| Độ tin cậy | ✅ Cao (ACK, gửi lại) | ❌ Không đảm bảo |
| Tốc độ | Chậm hơn | ✅ Nhanh hơn |
| Broadcast | ❌ Không hỗ trợ | ✅ Hỗ trợ |
| Dùng cho | Phiếu order, giao dịch | Thông báo, stream, game |

---

## Câu 2: Máy chủ tự tính tổng tiền thay vì tin client — tình huống tấn công cụ thể?

### Trả lời

Hệ thống FCanteen **không tin tổng tiền từ PosClient** — server luôn tự đọc giá từ DB và tính lại.

---

### Tình huống tấn công cụ thể

#### Tấn công 1: Man-in-the-Middle (MITM) — Sửa gói tin trên đường truyền

`
PosClient ──── {TotalAmount: 70000} ────► [Kẻ tấn công sửa → 7000] ────► KitchenServer
`

**Kịch bản**: Kẻ tấn công chèn vào giữa mạng, bắt gói TCP và sửa TotalAmount từ 70.000 xuống 7.000.

- ❌ Nếu server tin client: Lưu phiếu 7.000 VND → mất tiền
- ✅ FCanteen: Server tự lấy giá từ DB × số lượng → luôn ra 70.000

---

#### Tấn công 2: Gian lận Client (Client Tampering)

`csharp
// Nhân viên sửa code PosClient:
new OrderLineDto { MenuItemId = 1, Quantity = 10, UnitPrice = 1 } // giá 1đ thay vì 35.000đ
`

**Kịch bản**: Nhân viên sửa code PosClient để đặt giá = 1 VND cho mọi món.

- ❌ Nếu server tin client: Lưu phiếu 10 VND (10 món × 1đ) → thiệt hại
- ✅ FCanteen: Server đọc Price = 35.000 từ DB → tính 35.000 × 10 = 350.000

---

#### Tấn công 3: Replay Attack — Gửi lại phiếu cũ

**Kịch bản**: Chụp lại gói TCP của phiếu cũ (giá thấp hơn), gửi lại nhiều lần.

- ❌ Nếu server tin client: Chấp nhận phiếu với giá cũ
- ✅ FCanteen: Server luôn tự tính từ giá hiện tại trong DB → không dùng giá cũ được

---

### Nguyên tắc bảo mật: Never Trust the Client

> **Không bao giờ tin dữ liệu từ client đối với thông tin có giá trị kinh tế.**

| Vai trò | Thông tin |
|--------|----------|
| Client chỉ gửi | MenuItemId + Quantity + Note *(không có giá)* |
| Server tự lấy | Price từ database *(nguồn duy nhất đáng tin)* |
| Server tự tính | TotalAmount = SUM(Price × Quantity) |

---

*Trả lời được trích xuất từ thiết kế thực tế của hệ thống FCanteen — Lab 01 PRN222*
