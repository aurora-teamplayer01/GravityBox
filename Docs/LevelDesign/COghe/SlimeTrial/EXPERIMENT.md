# Slime Trial — mục tiêu và phép thử

Ngày: 19/11/2026. Trạng thái: kế hoạch thử nghiệm; **chưa có kết quả chơi thử**.

## Phạm vi được giao

Nguồn: Mrk, Buzz Venom, event `ceb399f5394290e62a329f2c458b0a738054130352172f3f3d8a5fcdbb1b8763`, thread `0a0d672f32aa9d68952a1873cf02ec107a7392d60c5fc665ac2b3000e8dad5e0`: tạo branch mới, ba màn thử đề xuất publisher, chú ý mục tiêu và gửi clip gameplay lên Buzz. Phân công Claude tại event `ebccfb7af4e7759e380cbd7cbbd38a39c4e2f18e54cde23f227dbc225c7a4b47`: Claude code/scenes/tests/capture; Codex tài liệu này và review độc lập.

Branch khai báo: `claude/slime-physics-trial`, base `b8b69f03`. Mỗi báo cáo phải ghi HEAD thực tế và diff lúc build; base không thay cho commit được kiểm chứng.

Thử một động từ: chạm slime, kéo lùi để tích lực/co giãn, thả để bật theo chiều đối diện. Cùng luật lực và phản hồi trong cả ba phòng. Năng lực cấp sẵn. Giới hạn chính xác lực, điểm neo và trạng thái được phép kéo cần ghi theo implementation cuối.

Đây là ngoại lệ có chủ đích với luật giao việc bằng chạm của campaign, được yêu cầu thử trong branch riêng. Không mặc nhiên đổi luật campaign, lưu tiến trình hoặc thương hiệu. Chưa kiểm tra toàn bộ đề xuất năm năng lực, DNA, chapter progression hay mô phỏng sandbox mở. Bố cục cuối được ghi trong ba hồ sơ level theo [mẫu hiện hành](../LEVEL_TEMPLATE.md); tài liệu này không tự chứng nhận scene tồn tại.

## Giả thuyết và ba vai trò của màn

| Màn / vai trò đề xuất | Câu hỏi cần trả lời | Bằng chứng cần có |
| --- | --- | --- |
| 01 — Glass Prison / học kéo-thả | Người mới hiểu chiều bật và mối liên hệ độ kéo–kết quả không? | Lần đầu tự thao tác, một cú hụt có thể sửa, sau đó thoát bằng thao tác hợp lệ. |
| 02 — vận dụng với môi trường | Cùng thao tác có tạo quyết định mới thay vì chỉ căn lực mạnh hơn không? | Một vật thể hoặc bề mặt có tác dụng nhìn thấy; người chơi thay điểm tác động/hướng để tiến triển. Không thêm nút kỹ năng. |
| 03 — chọn cách thoát | Người chơi có thể dùng cùng động từ theo ít nhất hai cách khác biệt không? | Hai đường giải từ reset, khác tương tác hoặc đường đi có ý nghĩa; đổi lực rất nhỏ trên cùng đường không tính là hai lời giải. |

Tên và bố cục 02–03 do implementation chốt. Tránh ba phòng chỉ đổi vị trí cửa. Màn 01 không cần chứa mọi cách giải publisher nêu. Nếu màn 03 chưa có hai cách giải, báo mục tiêu đó chưa đạt thay vì gọi cả bộ là đã chứng minh đa lời giải.

Giả thuyết H1: thao tác trực tiếp có thể hiểu và sửa sai. H2: thao tác tạo quyết định về môi trường. H3: nhiều cách giải mang lại lựa chọn thực. H4: người chơi muốn tiếp tục thử. H1–H4 cần người chưa biết lời giải; script replay chỉ chứng minh tính khả thi kỹ thuật.

## Điều kiện kỹ thuật trước khi gửi clip

- Tải/chọn/reset đủ ba màn; một đường giải mỗi màn, hai đường ở màn 03 nếu tuyên bố đa lời giải. Mọi đường qua input/controller người chơi thực sự có; không đặt vị trí slime, mở cửa, sửa win flag hoặc phát lực bí mật riêng cho replay.
- Ghi input kéo bắt đầu, kéo kết thúc, thời gian giữ và kết quả; quay được thân co giãn, chiều bật, tiếp xúc và thắng. Nếu clip dùng public API thay input pixel, ghi nhãn replay tự động và kiểm tra riêng một lượt input pixel.
- Thử hụt, va cạnh, kéo tối thiểu/tối đa, kéo ngoài slime, thả ngoài vùng chơi, hủy input/pause lúc đang kéo, retry sau thất bại. Không để lực kéo cũ tồn tại sau retry/chuyển màn.
- Kiểm tra hạt không xuyên thành, văng khỏi vùng chứa, NaN hoặc tách thành mảnh không thể kết thúc. Nếu có mắc kẹt, retry phải phục hồi đúng trạng thái.
- Chạy ở 30 và 60 FPS với cùng physics timestep; ghi kết quả từng cấu hình. Không cần kết quả bit-identical, nhưng cùng luật phải còn khả giải và lực không đổi theo framerate.
- Chạy các suite đầy đủ áp dụng cho assembly/package thay đổi và báo số test/failure. Có lỗi baseline phải phân biệt bằng bằng chứng, không suy đoán.
- Kiểm tra portrait 720×1280 và 720×1612: slime, cửa, vùng kéo và UI không che nhau; thao tác vào UI không phát lực.

Đạt kỹ thuật: đủ bằng chứng trên hoặc liệt kê rõ ca chưa kiểm. Blocker: không giải được, input không tương ứng chuyển động, lỗi vật lý/softlock không phục hồi, mất tiến trình campaign. Sửa blocker trước khi gọi là bản chơi được. Bản Mac không chứng minh touch/hiệu năng trên điện thoại.

## Phiên thử với người chưa xem demo

Đợt đầu đề xuất 5 người thuộc nhóm người chơi dự kiến, chưa xem clip hay đọc đường giải. Đây là kiểm tra định tính nhỏ để tìm lỗi, không phải ước lượng retention thị trường. Nếu ít người hơn, báo đúng mẫu và giữ kết luận tạm thời.

Đưa build chơi được trước khi đưa clip. Chỉ giao nhiệm vụ: “Tìm cách đưa slime ra khỏi phòng.” Cho tối đa 2 phút mỗi phòng trước khi hỏi có muốn gợi ý; có thể dừng sớm khi khó chịu. Không giải thích chiều kéo trước. Nếu runtime có hướng dẫn, giữ cùng hướng dẫn cho tất cả và ghi phiên bản. Ghi mọi lần người quan sát giúp.

Ba câu hỏi, hỏi sau lượt chơi để tránh dẫn dắt:

1. Khi kéo rồi thả, mày nghĩ slime sẽ đi thế nào? Sau khi thử, điều gì khác với dự đoán?
2. Lần vừa rồi chưa thoát được vì sao? Nếu thử lại, mày sẽ đổi điều gì?
3. Mày có muốn chơi thêm hoặc thử một cách khác không? Vì sao?

Quan sát tách khỏi lời trả lời: thời gian tới cú bật có chủ đích đầu tiên; số kéo nhầm chiều/chạm hụt; số lần thử/reset; thời gian và cách thoát; có cần trợ giúp; cách giải khác tự tìm được; lựa chọn chơi tiếp thực tế. Không xem lời khen là bằng chứng muốn chơi tiếp.

## Ngưỡng quyết định thử nghiệm — đề xuất nội bộ, không phải benchmark ngành

- H1 tạm đạt nếu ít nhất 4/5 tự thực hiện cú bật đúng ý trong 60 giây và giải thích được chiều bật sau màn 01, không được người quan sát chỉ thao tác. Nếu chưa đạt, sửa điều khiển/phản hồi rồi thử lại trước khi thêm năng lực.
- H2 tạm đạt nếu ít nhất 3/5 tự thay cách tương tác với môi trường ở màn 02, không chỉ lặp cùng cú bật; ghi rõ hành vi nào được tính theo layout cuối trước phiên thử.
- H3 kỹ thuật đạt khi hai đường giải khác biệt đều tái hiện được từ reset. H3 về khám phá chỉ tạm được hỗ trợ nếu nhóm tự tìm ra cả hai cách ở màn 03; không bắt từng người phải tìm đủ hai cách. Nếu chỉ tìm một cách, chưa kết luận đường thứ hai hấp dẫn.
- H4 chỉ ghi tín hiệu ban đầu nếu ít nhất 3/5 tự chọn chơi thêm hoặc thử cách khác khi được quyền dừng. Không chuyển tín hiệu này thành kết luận D1/D7.
- Có thể qua gate kỹ thuật nhưng chưa qua gate trải nghiệm. Ngưỡng này không cấp phép pivot; cần thêm thử trên điện thoại, so sánh bản cũ và xem khả năng làm nội dung tiếp.

## So với bản hiện tại

So ở cùng thiết bị, chất lượng hình, thời lượng, mức hướng dẫn và kinh nghiệm người chơi. Chọn đoạn đầu bản cũ có nhiệm vụ thoát phòng tương ứng; ghi chính xác build/level, không so prototype ba phòng dễ với boss cũ. Đảo thứ tự thử A/B giữa người tham gia để giảm lợi thế học hoặc mệt. Ghi nhận khác biệt layout/art; đây không phải phép cô lập hoàn hảo chỉ riêng input.

So khả năng tự hiểu, lỗi thao tác, giải thích thất bại, nhu cầu trợ giúp và lựa chọn chơi tiếp. Số cú kéo và số tap không phải cùng đơn vị chất lượng. Không yêu cầu prototype thắng về tốc độ thoát: khám phá có thể làm thời gian dài hơn. Kết quả hỗn hợp dẫn tới sửa đúng điểm yếu, không chọn riêng chỉ số đẹp.

Creative test là phép thử riêng sau khi playable ổn: dùng gameplay thật, cùng điều kiện phân phối/ngân sách/đối tượng khi so sánh. CTR/CPI không thay thế khả năng chơi, retention hay hiệu quả thương mại. Không có dữ liệu thì để chưa đo.

## Clip giao Mrk và provenance

Mỗi màn một clip, có thể thêm clip tổng hợp. Giữ tốc độ gameplay thật; hiện được bắt đầu kéo, thả, kết quả và thắng. Có ít nhất một đoạn hụt rồi phục hồi trong bộ clip; màn 03 quay cả hai cách nếu đã làm. Phụ đề tiếng Việt ngắn nêu câu hỏi thử nghiệm, không che gameplay. Không dùng clip giải mẫu trước phiên thử mù.

Ghi cùng clip: commit + diff/build hash; Unity 6000.3.19f1; máy/platform; resolution; input tay hay tự động; timestep/game-time; đường dẫn log/test; lỗi còn lại. Nếu encode từ frame sequence, khai báo đó là capture mô phỏng, không dùng tốc độ encode để tuyên bố FPS máy. Camera.Render có thể thiếu HUD IMGUI; kiểm tra hình cuối hoặc dùng framebuffer capture để chứng minh giao diện/input.

Nguồn capture đã đọc: `Assets/_Game/Tests/PlayMode/COgheChapterClipRecorder.cs` tạo PNG 30fps từ simulation tick và camera.Render; `Assets/_Game/Venom/Runtime/COgheMacPlaytest.cs` là telemetry, không phải video recorder; `Assets/_Game/Scripts/App/LocalPlaytestRecorder.cs` là event CSV. Máy đã phát hiện ffmpeg/ffprobe tại `/opt/homebrew/bin` và đúng Unity tại `/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app` ngày 19/11/2026. Đây là kiểm tra khả dụng, chưa chứng minh capture bản Slime Trial thành công.

## Bảng kết quả — điền sau khi chạy

| Mục | Trạng thái hiện tại | Bằng chứng |
| --- | --- | --- |
| Build/compile và suite | Chưa kiểm | — |
| 3 màn, solve/reset/recovery | Chưa kiểm | — |
| 2 đường màn 03 | Chưa kiểm | — |
| Input pixel, HUD, portrait | Chưa kiểm | — |
| 30/60 FPS và thiết bị mobile | Chưa kiểm | — |
| Clip đã xem/đã gửi Buzz | Chưa có | — |
| Người mới H1–H4 / so bản cũ | Chưa chạy | — |

Kết luận hiện tại: kế hoạch kiểm chứng. Chưa có căn cứ kết luận hướng mới vui hơn hoặc nên pivot.
