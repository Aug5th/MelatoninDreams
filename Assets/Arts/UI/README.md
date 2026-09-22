# UI Skin của GameKit

Toàn bộ art UI mà GameKit dùng nằm trong `Resources/GameKitUI/`.

**Đổi giao diện = ghi đè file cùng tên.** Không cần sửa code, không cần mở Sharp Editor để chỉnh 9-slice — `UIAssetPostprocessor` tự đặt lại import setting và border mỗi lần import.

> Thư mục phải giữ tên `Resources` thì Unity mới nạp được lúc chạy. Cấp `GameKitUI` bên trong để tên file không đụng với Resources khác trong project.

## Bảng slot

| File | Dùng ở đâu | Nguồn Kenney | 9-slice (L,B,R,T) |
|---|---|---|---|
| `panel.png` | Nền dialog: Settings, Pause, Game Over | `Grey/button_rectangle_depth_border` | 10, 14, 10, 10 |
| `panel_flat.png` | Khung mỏng cho ô nội dung | `Extra/input_rectangle` | 6, 6, 6, 6 |
| `button.png` | Nút thường | `Grey/button_rectangle_depth_gradient` | 6, 10, 6, 10 |
| `button_primary.png` | Nút chính (PLAY, RESUME, RETRY) | `Blue/button_rectangle_depth_gradient` | 6, 10, 6, 10 |
| `button_square.png` | Nút vuông chỉ có icon | `Grey/button_square_depth_gradient` | 6, 10, 6, 10 |
| `slider_track.png` | Rãnh slider âm lượng | `Grey/button_rectangle_flat` | 6, 6, 6, 6 |
| `slider_fill.png` | Phần đã lấp của slider | `Blue/button_rectangle_flat` | 6, 6, 6, 6 |
| `slider_handle.png` | Núm kéo | `Grey/button_round_depth_border` | Simple |
| `toggle_off.png` | Ô tick khi tắt | `Grey/check_square_grey` | 6, 6, 6, 6 |
| `toggle_on.png` | Dấu tick khi bật | `Blue/check_square_color_checkmark` | Simple |
| `progress_track.png` | Rãnh thanh loading | `Grey/button_rectangle_flat` | 6, 6, 6, 6 |
| `progress_fill.png` | Phần đã chạy của thanh loading | `Blue/button_rectangle_flat` | 6, 6, 6, 6 |
| `icon_play.png` | (để dành cho nút play dạng icon) | `Extra/icon_play_light` | Simple |
| `icon_pause.png` | Nút pause trên HUD | **tự sinh** — xem ghi chú | Simple |
| `icon_restart.png` | (để dành cho nút chơi lại) | `Extra/icon_repeat_light` | Simple |
| `icon_close.png` | (để dành cho nút đóng) | `Grey/icon_cross` | Simple |
| `icon_check.png` | (để dành cho xác nhận) | `Grey/icon_checkmark` | Simple |
| `arrow_left.png` | Nút ◀ ở dòng Quality | `Grey/arrow_basic_w` | Simple |
| `arrow_right.png` | Nút ▶ ở dòng Quality | `Grey/arrow_basic_e` | Simple |
| `divider.png` | Đường kẻ ngăn nhóm | `Extra/divider` | 8, 0, 8, 0 |
| `star.png` | Ngôi sao cạnh điểm BEST | `Grey/star` | Simple |
| `font_ui.ttf` | Font toàn bộ UI | `Font/Kenney Future Narrow.ttf` | — |

**Vì sao slider không dùng `slide_horizontal_*` của Kenney:** bộ đó vẽ sẵn núm tròn ở hai đầu thanh, nên khi 9-slice kéo dài ra thì hai đầu phình lên trông như quả tạ. Tôi thay bằng `button_rectangle_flat` — rounded rect mỏng, kéo dài bao nhiêu cũng giữ đúng hình. Núm kéo tách riêng thành `slider_handle.png`.

**Ghi chú về `icon_pause.png`:** Kenney UI Pack không có icon pause (chỉ có play, repeat, arrow up/down). File này tôi vẽ bằng code — hai thanh bo góc trắng 20×18 để khớp phong cách các icon còn lại. Nếu bạn có bộ icon khác, ghi đè nó như mọi file khác.

## Cách màu hoạt động

Sprite Kenney có màu sẵn, nên khi skin được nạp GameKit **không tint đè** lên nút — chỉ làm sáng/tối nhẹ lúc hover và nhấn. Bảng màu nằm trong `Theme` (file `GameKit/Runtime/Core/GameKitConfig.cs`):

- `SkinTint` / `SkinHover` / `SkinPress` — trạng thái nút khi có skin
- `TextOnLight` — màu chữ trên nút sáng màu
- `Panel`, `TrackColor` — vẫn tint đè lên `panel.png` và rãnh slider để giữ nền tối

Muốn theo tông Kenney gốc (nền sáng): đổi `Theme.Background` và `Theme.Panel` sang màu sáng, `Theme.Text` sang màu tối.

## Xoá bớt asset thì sao?

Không sao. `UISkin` nạp từng file một; file nào thiếu thì `UIFactory` tự quay về sprite bo góc sinh bằng code cho đúng chỗ đó. Xoá sạch thư mục `Resources/GameKitUI` thì kit chạy lại y hệt lúc chưa có art.

## License

Kenney UI Pack là CC0 — dùng thương mại thoải mái, không bắt buộc ghi nguồn (nhưng nên ghi). Xem `Assets/Arts/kenney_ui-pack/License.txt`.
