# Kế hoạch phát hành Alien Pet trên App Store

## Mục tiêu và phạm vi

Giữ nhận diện Alien Pet, COghe là tên sinh vật; iPhone + iPad portrait; Glass C; main campaign 65 màn, không quảng bá 2 prototype như màn hoàn chỉnh. Phát hành iOS không được đổi bundle/save keys hoặc làm yếu age gate để chạy quảng cáo.

Bộ này là hồ sơ bàn giao. Không có App Store Connect mutation, build phân phối hay kết quả TestFlight mới được thực hiện trong gói này.

## Giai đoạn 1 Chuẩn bị tài khoản và môi trường

Người vận hành trên máy mới:

- Apple Developer Program còn hiệu lực, team và quyền Certificates/Identifiers/Profiles + App Store Connect phù hợp. Personal Team dùng test không thay thế team phân phối.
- Unity **6000.3.19f1**, iOS Build Support; Xcode và iOS SDK đáp ứng yêu cầu upload đang có hiệu lực. Kiểm tra lại trang Apple tại thời điểm nộp; không suy luận từ việc Xcode cũ build được.
- Chạy `python3 Tools/setup-google-sdks.py` trong repo và resolver iOS/CocoaPods. Ghi phiên bản Pod thực tế/Podfile.lock, SDK và Xcode trong hồ sơ release. Không dùng script `run-ios-simulator.sh` nguyên trạng: nó build prototype cũ.
- Xác nhận team sở hữu/đăng ký được Bundle ID **com.gravityboxlab.venom**. Nếu đã thuộc team khác, dừng giải quyết ownership; không tự đổi identifier vì ảnh hưởng dịch vụ/save.
- Tạo iOS app trong App Store Connect: tên Alien Pet: Escape Puzzle; primary language English (US); SKU nội bộ đề xuất `alienpet-ios`; chọn đúng Bundle ID. Tên studio hiển thị phụ thuộc tư cách Apple account, không đảm bảo nhập tùy ý thành YoloStudioGlobal.
- Marketing version đề xuất **1.0.0**, build **1**, chỉ khi chưa dùng trên App Store Connect. Version Android không quyết định build number iOS. Kiểm tra số đã có trước khi chọn.

Đầu ra: điền account-inputs riêng trên máy mới (không commit bí mật), source commit và môi trường build.

## Giai đoạn 2 Hoàn thiện bản iOS

Làm theo `technical/IOS-GAPS.md`. Đây là phần kỹ thuật thật còn thiếu, không thể bỏ qua bằng điền App Store Connect.

Hướng khuyến nghị: iOS có chức năng tương đương Android với cùng nguyên tắc tuổi/consent. Nếu muốn ra trước bằng bản không Ads/Analytics/IAP, cần chủ sản phẩm chọn phương án đó và sửa các nút/offer/UI cho đúng; không tự phát hành bản có nút mua/xem ads nhưng provider không hoạt động.

- Tạo Firebase iOS app đúng bundle, lấy GoogleService-Info.plist. Tách stream/platform, dùng chung project nếu phù hợp. Không copy google-services.json Android sang iOS.
- Tạo AdMob **iOS app và iOS units**; cấu hình app ID, rewarded/interstitial/banner, UMP riêng đúng app. Android IDs không dùng thay.
- Tích hợp boot iOS có kiểm soát tuổi; audit auto-init native Firebase trước C# và analytics defaults trong Info.plist. Tất cả SDK/adapters/Remote Config phải tôn trọng lựa chọn.
- Hoàn thiện thanh toán Apple nếu phát hành các gói mua. Xác minh sản phẩm thật, localized price, cancel/pending/fail, xác thực giao dịch, restore, refund/revocation. Không dùng mock/simulated fulfillment. Product IDs cần đối chiếu runtime; `coghe_plus` và `coghe_no_ads` trong code là khóa entitlement, chưa chứng minh đã là SKU App Store.
- Kiểm tra ATT/tracking theo hành vi SDK thực tế; UMP không thay thế ATT. Không bắt người chơi đồng ý tracking để chơi/nhận quyền đã mua. Không thêm prompt tracking cho trẻ em.
- Kiểm tra privacy manifests, required-reason APIs, SDK signatures, launch screen, icon, safe areas và iPad.

Đầu ra: code/config iOS được review; tất cả blocker P0 đóng bằng bằng chứng.

## Giai đoạn 3 Export và ký

Đóng Unity đang mở repo. Sau khi Giai đoạn 2 đạt:

```sh
export DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer
unset COGHE_IOS_TEAM COGHE_IOS_PROFILE COGHE_IOS_DEVICE COGHE_BENCHMARK COGHE_PROFILE
COGHE_STORE=1 bash Tools/build-venom-ios.sh --spatial
```

Lệnh này export current Spatial catalog. `COGHE_STORE=1` bỏ test tools nhưng **không tự xác nhận mọi điều kiện App Store**. Hiện nó sẽ dừng nếu iOS AdMob app ID trống; không xóa guard để vượt lỗi.

Mở `Builds/SpatialLab/iOS/Unity-iPhone.xcworkspace` khi có CocoaPods, nếu không mới dùng xcodeproj. Kiểm tra Unity-iPhone app target và UnityFramework:

1. Chọn team của máy mới, automatic signing phù hợp; bỏ profile/device development cố định từ lần export cũ.
2. Bundle ID `com.gravityboxlab.venom`, display name Alien Pet: Escape Puzzle; version/build theo App Store Connect; thiết bị iPhone và iPad; portrait; deployment target tương thích SDK/thiết bị đã test.
3. Áp dụng icon C trong Unity, kiểm tra asset catalog được export. `assets/icon/AppIcon.appiconset` là nguồn thay thế nếu thiếu marketing icon; không xóa các slot Unity sinh mà chưa kiểm tra.
4. Chọn Generic iOS Device / Any iOS Device, Release, **Product → Archive**.
5. Organizer → Validate App; sửa mọi error, kiểm tra privacy/symbol warnings. Distribute App → App Store Connect → Upload. Đây khác `xcodebuild build` cài USB.
6. Giữ exact `.xcarchive`, dSYM, Podfile.lock, source commit và upload validation ở chỗ riêng an toàn. Không đưa signing credentials vào gói chia sẻ.

Không tạo ExportOptions có team giả. Có thể dùng `technical/ExportOptions.template.plist` sau khi điền team; Xcode Organizer là luồng chính được khuyến nghị. Bản đã cài USB không chứng minh TestFlight hoạt động.

## Giai đoạn 4 TestFlight và tài nguyên cuối

- Đợi processing, giải quyết export compliance bằng câu trả lời đúng SDK mã hóa thực tế. Không tự khai miễn trừ chỉ vì là game.
- Internal TestFlight trước; external testing có thể cần Beta App Review. Test trên ít nhất một iPhone và một iPad vật lý; kiểm tra bản cài từ TestFlight, không chỉ Xcode.
- Điền `QA-CHECKLIST.csv`. Chạy ma trận tuổi, reward một lần, consent, cold launch, update giữ save, restore IAP, background/resume và 20–30 phút gameplay. Đo FPS/thermal trên thiết bị; không đưa mục tiêu FPS vào marketing như kết quả đo.
- Chụp đúng binary TestFlight hoặc simulator cùng commit/config hiển thị. Thay ảnh nguồn theo `SCREENSHOTS.md`; kiểm tra final trên iPhone/iPad trước khi upload.
- Copy metadata. Không quảng cáo IAP/khả năng chưa hoạt động; bản mô tả hiện không hứa offline, không quảng cáo, 60 FPS hay mọi item miễn phí.

## Giai đoạn 5 Hồ sơ và submit

- App Information: primary Games, Puzzle, secondary Adventure nếu Console phù hợp; quyền nội dung do chủ app xác nhận. Không chọn Kids Category tự động chỉ vì audience 9–15; quyết định category và giới hạn SDK phải được đối chiếu yêu cầu thực tế.
- Pricing/Availability: đề xuất Free và US trước; điền thuế/ngân hàng/hợp đồng nếu commerce yêu cầu. Các trường pháp nhân/điện thoại/người liên hệ do account owner nhập.
- Version: screenshot final, description, keyword, support/privacy URLs, review notes và build đúng version.
- App Privacy + age rating theo `PRIVACY-AND-REVIEW.md`; rating do câu trả lời nội dung tạo ra, không sao chép ESRB/PEGI thành rating Apple.
- App Review access: không cần login nếu vẫn giữ gameplay hiện tại; ghi cách tiếp cận Home, coloring, ads/IAP thật và mô tả monster animation trung thực.
- Chọn manual release; submit khi checklist P0/P1 qua. Sau khi duyệt kiểm tra store listing, cài từ App Store, dịch vụ và bản cập nhật. Giữ kill switch cho ads theo thiết kế hiện có, không đổi gameplay để lách review.

## Nguồn và thời điểm kiểm tra

Tra ngày 16/11/2026; máy gửi cần kiểm tra lại khi upload.

- Screenshot sizes: https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/
- Metadata fields: https://developer.apple.com/help/app-store-connect/reference/app-information/platform-version-information/
- Submission requirements: https://developer.apple.com/app-store/submitting/
- Review guidelines: https://developer.apple.com/app-store/review/guidelines/
- App privacy: https://developer.apple.com/app-store/app-privacy-details/
- Google Unity iOS privacy: https://developers.google.com/admob/unity/privacy/strategies
- Firebase Unity setup: https://firebase.google.com/docs/unity/setup

- Icon asset catalog: https://developer.apple.com/documentation/xcode/configuring-your-app-icon
