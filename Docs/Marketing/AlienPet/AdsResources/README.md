> **Thư mục tài nguyên ads chính của game:** `Docs/Marketing/AlienPet/AdsResources/`
>
> - [Xem / chọn từng clip](index.html)
> - [Gói chỉ chứa 8 clip Alien Pet](Packages/AlienPet_Ads_8Clips_2026-10-29_scene2fix.zip)
> - [Bộ tài nguyên đầy đủ v2](Packages/AlienPet_Ads_ResourceKit_2026-10-29_scene2fix.zip)
> - [Bộ ASO Google Play 29/10: icon, feature graphic, phone/tablet screenshots và description](../GooglePlay/2026-10-29/screenshot-review.html) · [ZIP upload](../GooglePlay/2026-10-29/AlienPet_GooglePlay_UploadPack_2026-10-29.zip)
> - `Packages/Archive/` giữ ZIP v1. Các bản ZIP là gói xuất cục bộ; file video, thumbnail, dữ liệu và hướng dẫn nằm trực tiếp trong thư mục này.

# Alien Pet: Escape Puzzle — bộ tài nguyên quảng cáo

Ngày xuất: 28/10/2026. Mở **index.html** để xem và tải từng video.

## Dùng để chuẩn bị test quảng cáo

Thư mục **01_CURRENT_ALIEN_PET** có tám video mang nhận diện mới:

| ID | Hướng sáng tạo | Thời lượng |
|---|---|---|
| A | Escape Artist — giải đố, cơ quan, thoát khỏi hộp | 22 giây |
| B | Your New Alien Friend — gắn bó với pet + giải đố | 24 giây |
| C | Tiny Pet, Big Personality — nuôi pet vui nhộn, cho ăn, đổi màu, thay mũ | 24 giây |
| D | Puzzle — đổi từ OLD01 | 15 giây |
| E | Pet Play — đổi từ OLD02 | 17 giây |
| F | Game Showcase — đổi từ OLD03 | 30 giây |
| G | Cinematic Escape — đổi từ OLD04 | 30 giây |
| H | Gameplay Tour — đổi từ REF01 | 30 giây |

Cả tám: tiếng Anh, 1080×1920, 9:16, 30fps, H.264, stereo AAC 48kHz, fast-start.
Đây là các file để test creative, chưa có campaign được đăng. Hiệu quả CPI cần đo thực tế.

## Các bản cũ và nguồn tham khảo

- **02_ARCHIVE_COGHE**: 4 quảng cáo hoàn chỉnh mang tên COghe cũ (15/17/30/30 giây). Bản gốc để lưu trữ; các phiên bản đổi thương hiệu tương ứng là D/E/F/G.
- **03_REFERENCE_EXPORTS**: 2 bản xuất khác tìm thấy trong Downloads. `REF01` là showcase tên cũ, đã đổi thành bản H; `REF02` là gameplay không âm thanh, chưa có end card. Không dùng chúng như bản quảng cáo đã hoàn thiện.
- Các file trùng nội dung do sao chép, intermediate picture-lock và shot nguồn đơn lẻ không được tính là quảng cáo riêng. ActionCut cũ chỉ còn nguồn dựng, không tìm thấy bản final nên không gắn nhãn hoàn thành.

## Tài nguyên kèm theo

- **Thumbnails/**: ảnh xem nhanh từng video.
- **ad-copy.txt**: headline, primary text, CTA tiếng Anh cho A–H.
- **manifest.json**: thời lượng, kích thước, frame rate, âm thanh, nguồn, trạng thái và SHA-256.
- **Checks/**: thông số kiểm tra file mới, bảng shot, kết quả capture thật trong Unity.
- **Sources_C/**: hai cảnh Veo, reel gameplay gốc đã nén, mốc thao tác/âm thanh và script dựng clip C để dựng lại. File PNG trung gian không đóng gói.

## Nguồn hình và âm thanh

Các cảnh người thật + sinh vật 3D ở A/B/C được tạo trong Google Flow bằng Veo 3.1 Quality, dựa trên cùng bộ ảnh tham chiếu. C có hai cảnh mới: cho ăn và vuốt ve. Nguồn AI 720×1280 được upscale lên 1080×1920; không gọi đây là AI native 1080p.

Phần ACTUAL GAMEPLAY quay từ Unity hiện tại. C sử dụng thao tác thật trong Home/Style: ăn hai loại snack, phản ứng ăn cay, giữ ống tiêm Mint và thay ba mũ. Fixture mở khóa trong bộ nhớ phục vụ quay hình, không ghi tiến độ hay mua hàng. Các khung hình được crop để tập trung vào pet, không thay animation. Âm thanh game được đặt theo timestamp capture; nhạc nền mới được tổng hợp riêng bằng script, kết hợp foley do Veo tạo. Không dùng bài hát thương mại có sẵn.

Hai cảnh AI có nhãn AI CINEMATIC, cảnh game có nhãn ACTUAL GAMEPLAY. Không gắn logo nền tảng/stores hay đường dẫn chưa được cung cấp.

Toàn bộ 14 video xuất được decode kiểm tra. Tám bản Alien Pet được kiểm tra thời lượng, frame count, khung đen và âm lượng; ZIP được đọc lại để kiểm tra checksum. Lịch sử nguồn được giữ nguyên ngoài gói.

D–H preserve historical footage and audio. The baked native caption band and end cards are replaced; the cinematic edit uses the original unbranded picture-lock. Old level-count claims and PLAY FREE are removed. COghe remains when naming the creature in actual UI (for example Understand COghe). Historical footage is not a new capture. Original exports are untouched.


## Scene 2 correction — 29 October 2026

Cảnh 2 đã sửa 29/10: COghe bò ở mặt trong kính; giữ nguyên nhạc, lời thoại và các cảnh khác.

The replaced interval is frames [241, 331) at 30fps (8.033333–11.033333s). The 30s final retains its original AAC packets, including timestamps. Flow uses a corrected start frame and the same character/scientist references. Original exports and prior ZIPs are preserved.
