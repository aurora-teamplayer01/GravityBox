# Slime Trial — 3 màn thử điều khiển trực tiếp (kéo-thả)

Nhánh `claude/slime-physics-trial` (tách từ `NewGraphic` b8b69f03). Yêu cầu: Mrk 19/11/2026, thread Buzz `0a0d672f…`.
Mục tiêu và tiêu chí thử nghiệm: [EXPERIMENT.md](EXPERIMENT.md) (Codex viết, file của Codex). Hồ sơ từng màn:
[S1](S1.md), [S2](S2.md), [S3](S3.md).

Ba màn này **nằm ngoài** campaign 65 màn: không có trong `SpatialOrder`, không ghi save/tiến trình, không DNA,
không mở khóa, không Personality/Intelligence/ProductUI/quảng cáo (definition dùng `ProgressKey = coghe.slime.trial`).

## Điều khiển (một thao tác cho cả 3 màn)

| Mục | Giá trị (runtime `COgheSlimeControl`) |
| --- | --- |
| Thao tác | Chạm bất kỳ đâu trong vùng chơi (không cần chạm vào slime), kéo lùi, thả: slime bật theo hướng ngược với hướng kéo. |
| Vùng chơi | Cả màn hình trừ dải tiêu đề trên và dải nút dưới (`Layout().Play`). Chạm vào nút THỬ LẠI/DỪNG không bao giờ thành cú kéo. |
| Độ kéo → lực | Đo trong mặt phẳng làn chơi (đơn vị mét của phòng 60×80 cm). Kéo < 2,5 cm = huỷ. Lực = (kéo − 2,5)/(20 − 2,5 cm), kẹp 0–100 %. Tốc độ bật = 1,0 → 3,4 m/s tuyến tính theo lực. Kéo quá 20 cm vẫn là 100 %. |
| Khi nào được bật | Chỉ khi thân đang bám (≥ 2 hạt có grip: sàn, kính trần, thùng). Thả khi đang bay: từ chối, thân rung nhẹ, thông báo “Đang bay: chạm vào bề mặt rồi mới bật được”. |
| Bám (Stick) | Dùng nguyên cơ chế bám có sẵn của Motion: chạm kính trần/sàn là dính, kể cả tường và trần; lớp phủ tím (trơn) không bám. Khi chạm đất thêm bộ giảm chấn 0,2 s để lún rồi đứng, không trượt. |
| Phản hồi trước khi thả | Dây kéo tới ngón tay, mũi tên hướng bật (dày theo lực), số %, chuỗi chấm quỹ đạo theo trọng lực thật dừng ở vật cản đầu tiên, thân slime kéo giãn về phía ngón tay. Quỹ đạo **không** vẽ gió của quạt (S3). |
| Huỷ | Thả ngoài vùng chơi, DỪNG, mất focus/app pause, THỬ LẠI, đổi màn, touch bị huỷ: xoá sạch trạng thái kéo, không bật. |
| Không phụ thuộc FPS | Input chỉ ghi vector kéo; cú bật áp vào bước vật lý cố định kế tiếp (120 Hz). Test: vận tốc bật ở 60 và 30 fps trùng tới 1e-4 m/s. |
| Thất bại | Chạm laser (S2), văng ra ngoài hộp/xuyên kính, NaN → thông báo + THỬ LẠI. THỬ LẠI khôi phục thùng, nút, laser, quạt, cửa chớp, thân và trạng thái kéo. |

## Cách chạy

- Chơi trong Editor: mở `Assets/_Game/Venom/SlimeTrial/COgheSlimeS1.unity` (S2, S3 tương tự), bấm Play. Chuột: nhấn-kéo-thả.
  Phím: `R` thử lại, `P` dừng, `1`/`2`/`3` đổi màn. Mac build chưa làm.
- Sinh lại scene (đã commit sẵn, chỉ cần khi sửa builder):
  `Unity -batchmode -nographics -projectPath "/Volumes/Phone SSD/GravityBox-slime" -executeMethod GravityBox.Editor.VenomCampaignBuilder.GenerateSlimeTrial -quit`
  Lưu ý: bước art của builder ghi thêm keyword `_EMISSION` vào `Art/DayLab/Quiet mint light.mat` (vật liệu dùng chung); revert file đó sau khi sinh.
- Test (không gồm Explicit):
  `Unity -batchmode -projectPath "/Volumes/Phone SSD/GravityBox-slime" -runTests -testPlatform PlayMode -testFilter "GravityBox.Tests.COgheSlimeTrialTests"`
- Tìm cú bật (Explicit): `…COgheSlimeTrialTests.SlimeSearchS1` / `S2` / `S3` → `Artifacts/SlimeTrial/search-*.txt`.
- Quay clip (Explicit): `…SlimeClipS1` / `S2` / `S3` → `Artifacts/SlimeTrial/clips/*/f####.png` + `captions.tsv`;
  encode bằng `~/.buzz/.scratch/slime-trial/build_clip.py` (Pillow + ffmpeg, rồi remux bỏ metadata).

## File

| Loại | Đường dẫn |
| --- | --- |
| Điều khiển, HUD, phản hồi kéo | `Assets/_Game/Venom/Runtime/Slime/COgheSlimeControl.cs` |
| Cơ quan | `Runtime/Slime/COgheSlimePad.cs` (thùng trên ray đè nút), `COgheSlimeLaser.cs`, `COgheSlimeFan.cs` (gió theo vùng, cửa chớp) |
| Móc vào runtime chung | `VenomCampaign.cs` (chuyển pointer cho slime, bỏ ghi save khi thắng, giữ khung camera), `VenomCampaignInput.cs` (huỷ kéo khi reset/touch), `VenomCampaignMotion.cs` (`Detach`), `VenomCampaignCamera.cs` (khung phòng). Các nhánh chỉ chạy khi scene có `COgheSlimeControl`. |
| Builder | `Assets/_Game/Editor/COgheSlimeTrialLevels.cs`; `COgheGlassPreviewBuilder.SpatialArtKey` thêm nhánh `Slime…` (trước đó lần sinh đầu đã ghi đè mesh kính Spatial Level01–03; đã revert, không commit). |
| Scene / definition / mesh | `Assets/_Game/Venom/SlimeTrial/`, `Art/DayLab/Meshes/SlimeTrial/`, `Art/DayLab/Meshes/SpatialGlass/SlimeS1–S3/`; 3 scene thêm cuối `EditorBuildSettings`. |
| Test | `Assets/_Game/Tests/PlayMode/COgheSlimeTrialTests.cs` (harness), `…ProofTests.cs`, `…Search.cs`, `…Clips.cs` |
| Bằng chứng | `Artifacts/SlimeTrial/` (không commit: search, solves, wander, layout, clips) và các file trong thư mục này |
