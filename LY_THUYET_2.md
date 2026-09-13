# Lý Thuyết Lab 01 — Trả lời đơn giản

**Môn**: PRN222 | **Họ tên**: Châu Vương Hoàng | **Mã SV**: DE180551

---

## Câu 1: Tại sao gửi phiếu order dùng TCP, thông báo hết món dùng UDP?

### Dùng TCP để gửi phiếu order vì:

TCP giống như gửi bưu phẩm có xác nhận — người gửi biết chắc người nhận đã nhận được.

Phiếu order chứa thông tin quan trọng như tên món, số lượng, tiền. Nếu gói tin bị mất mà không gửi lại → bếp không nhận được → khách không có món → thiệt hại thực tế. Vì vậy cần TCP để đảm bảo phiếu **luôn đến nơi và đúng thứ tự**.

### Dùng UDP để thông báo hết món vì:

UDP giống như phát loa thông báo — nhanh, ai nghe được thì nghe, ai không nghe cũng không sao lắm.

Thông báo hết món cần gửi **đồng thời đến tất cả các quầy** (broadcast). Nếu 1-2 quầy không nhận được thì lần sau họ load lại menu từ database vẫn biết được. Không gây thiệt hại nghiêm trọng. UDP nhanh hơn và hỗ trợ broadcast nên dùng UDP là hợp lý.

---

## Câu 2: Tại sao server phải tự tính tiền, không tin số tiền từ client gửi lên?

### Trả lời:

Vì client có thể bị **hack hoặc sửa code** để gửi số tiền giả lên server.

**Ví dụ đơn giản**: Một nhân viên quầy sửa code PosClient để tự đặt giá mỗi món = 1 đồng. Nếu server tin vào số đó → lưu phiếu 10 đồng cho 10 món → công ty mất tiền.

**Ví dụ khác**: Kẻ tấn công chặn gói tin TCP trên mạng, sửa tổng tiền từ 70.000đ xuống 7.000đ rồi mới gửi đến server. Nếu server tin → thiệt hại.

**Cách FCanteen xử lý**: Server không quan tâm đến số tiền client gửi lên. Server tự lấy giá từ database, tự nhân với số lượng, tự ra tổng tiền. Client chỉ được gửi: **món nào + bao nhiêu cái + ghi chú**. Tiền là việc của server.

> Nguyên tắc: **Không bao giờ tin số tiền từ phía người dùng gửi lên.**
