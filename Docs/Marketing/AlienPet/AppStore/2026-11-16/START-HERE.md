# Alien Pet App Store handoff

Bộ bàn giao ngày 16/11/2026 dành cho máy Mac có Apple Developer account. Đích phát hành: **Alien Pet: Escape Puzzle**, iPhone và iPad, tiếng Anh. Gameplay và đồ họa giữ nguyên bản NewGraphic; không thay thiết kế level trong công việc này.

**Trạng thái: đã chuẩn bị kế hoạch, metadata, icon và bố cục screenshot. Chưa đủ điều kiện submit App Store.** Ads/Analytics chỉ boot trên Android trong code hiện tại; iOS chưa có cấu hình dịch vụ thật và chưa xác minh IAP. Đây không phải IPA hay bản phát hành đã ký.

## Nhận bộ bàn giao

1. Giải nén gói `AlienPet-AppStore-Handoff-2026-11-16.zip`; mở `review.html` để xem hình.
2. Chạy `python3 tools/verify-package.py` để đối chiếu SHA-256 và kích thước tài nguyên.
3. Lấy source game riêng từ repo, không có source game đầy đủ trong ZIP:

```sh
git clone --branch NewGraphic git@github.com:aurora-teamplayer01/GravityBox.git
cd GravityBox
git checkout dec14e929897b4268421553ac51ab02abd297d09
```

Commit trên là mốc đã audit, không phải yêu cầu bỏ các sửa đổi mới hơn. Nếu dùng HEAD mới, đọc diff và chạy lại preflight/QA; giữ bản ghi commit đã phát hành. Máy mới cần quyền truy cập repo riêng tư. Không copy Library/Builds/certificate từ máy cũ.

## Thứ tự thực hiện

| Thứ tự | Tài liệu hoặc thư mục | Công việc |
|---|---|---|
| 1 | `technical/IOS-GAPS.md` | Xử lý các blocker iOS thực tế, không chỉ đổi signing |
| 2 | `RELEASE-PLAN.md` | Tạo app, cấu hình dịch vụ, Archive và TestFlight |
| 3 | `SCREENSHOTS.md` | Thay ảnh tham khảo bằng capture iOS, xuất bộ upload |
| 4 | `metadata/en-US/` | Copy nội dung vào App Store Connect |
| 5 | `PRIVACY-AND-REVIEW.md` | Hoàn tất privacy, rating, review notes theo binary thực tế |
| 6 | `QA-CHECKLIST.csv` | Ghi PASS/FAIL, thiết bị, build và bằng chứng |
| 7 | `RELEASE-PLAN.md` phần phát hành | Validate, submit, phát hành thủ công sau khi được duyệt |

## Có sẵn

- Icon C 1024 × 1024 RGB, không alpha; kèm `AppIcon.appiconset` single-size cho Xcode. Không thêm viền/bo góc vào ảnh.
- 6 screenshot iPhone 1320 × 2868, 4 screenshot iPad 2064 × 2752: **bản bố cục có watermark nháp**, dùng nguồn gameplay thật Android/Unity. Không upload thư mục `screenshots/draft`.
- File nguồn ảnh, font, JSON bố cục, renderer có kiểm tra đè chữ và khóa xuất final khi chưa thay nguồn iOS.
- Description, subtitle, keywords, promotional text, review-note template và ma trận QA.
- Preflight đọc repo, trình xác minh ZIP và hướng dẫn build portable. Không có mật khẩu, private key, `.p12`, provisioning profile hay Apple API key.

## Cần máy Apple account làm

Apple Developer Program/team có quyền phân phối; xác nhận Bundle ID thuộc team; cấu hình App Store Connect và các dịch vụ iOS; build/test bản phân phối; capture iOS; ký, upload và submit. Những khai báo pháp lý, rating và export compliance phải dựa trên nội dung/SDK thật, không copy nguyên Google Play.

Các trường chưa chốt có trong `technical/account-inputs.template.json`. Quốc gia mặc định đề xuất là **Mỹ trước**, free download, manual release; đây là cấu hình đề xuất cho iOS, chưa được áp dụng trên tài khoản.
