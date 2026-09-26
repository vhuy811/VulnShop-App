# VulnShop

Ứng dụng ASP.NET Core **cố ý có lỗ hổng**, dùng làm mục tiêu kiểm thử cho pipeline DevSecOps ở [vhuy811/VulnShop-DevSecOps](https://github.com/vhuy811/VulnShop-DevSecOps).

**Chỉ chạy trên localhost. Không bao giờ triển khai.**

## Có gì trong này

`ground_truth.csv` liệt kê từng trường hợp: có lỗ hổng thật hay đã khử độc. Đó là đáp án để đo pipeline — không sửa các trường hợp này, sửa là phá bộ dữ liệu đối chứng.

Pipeline gắn qua `.github/workflows/bao-mat.yml`. Nó gọi workflow dùng chung, không chứa công cụ nào ở đây. Bốn lỗ hổng có sẵn là **nợ cũ**: được báo cáo, không chặn PR mới.

## Chạy tại chỗ

```
dotnet run --urls http://0.0.0.0:5000
```

Mở http://localhost:5000/Product/List. Quét bằng dashboard của bộ công cụ với URL `http://host.docker.internal:5000`.

## Cho đồng đội

Đọc `HUONG_DAN_DONG_DOI.md` trong repo bộ công cụ. Tóm tắt: không push lên `main`, mỗi việc một nhánh, mở PR, đọc check `security / scan`.
