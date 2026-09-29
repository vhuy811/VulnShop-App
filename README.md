# VulnShop

Ứng dụng ASP.NET Core nhỏ, dùng làm mục tiêu kiểm thử cho pipeline DevSecOps ở [vhuy811/DevSecOps_VHNAT](https://github.com/vhuy811/DevSecOps_VHNAT).

**Chỉ chạy trên localhost. Không bao giờ triển khai.**

## Hai phiên bản, hai mục đích

| Ở đâu | Trạng thái | Dùng để |
|---|---|---|
| nhánh `main` | **sạch** — 0 lỗ hổng xác nhận được | điểm xuất phát cho kịch bản làm việc nhóm: cổng phải chặn code hỏng *trước khi* nó vào `main` |
| tag `ground-truth` | 4 lỗ hổng gieo cố ý + 2 case an toàn | đo độ chính xác của pipeline; `ground_truth.csv` mô tả bản này |

Kết quả đo trên tag `ground-truth`: pipeline gán đúng nhãn 6/6 case — 4 CONFIRMED, 2 FILTERED, 0 sai. Bằng chứng nằm ở lần chạy CI đầu tiên của repo này.

## Chạy tại chỗ

```
dotnet run --urls http://0.0.0.0:5000
```

Mở http://localhost:5000/Product/List. Quét bằng dashboard của bộ công cụ với URL `http://host.docker.internal:5000`.

## Xem lại bản có lỗ hổng

```
git checkout ground-truth
```

Đừng sửa gì ở tag đó — nó là bộ dữ liệu đối chứng.

## Cho đồng đội

Đọc `HUONG_DAN_DONG_DOI.md` trong repo bộ công cụ. Tóm tắt: không push lên `main`, mỗi việc một nhánh, mở PR, đọc check `security / scan`.
Chay ung dung: dotnet run
