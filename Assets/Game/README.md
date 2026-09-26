# GameKit — Unity starter kit cho GameJam 1 tuần

Bộ khung "phần nào game cũng cần", viết sẵn để bạn chỉ phải làm gameplay:

- **Main Menu + Settings + Pause + Game Over** — dựng hoàn toàn bằng code
- **Audio Manager** — nhạc nền crossfade, pool SFX, âm lượng lưu lại
- **Save/Load** — JSON atomic vào `persistentDataPath`, tự chuyển sang PlayerPrefs khi build WebGL
- **Game State Machine + Scene Loader** — `Menu / Loading / Playing / Paused / GameOver`, load scene async có fade + thanh progress

Không cần TextMeshPro, không cần import asset, không cần kéo thả prefab. Font dùng font built-in của Unity, sprite bo góc và toàn bộ âm thanh được **sinh ra lúc chạy**.

---

## Cài trong 30 giây

1. Tạo project Unity mới (2021.3 LTS trở lên, kể cả Unity 6 — bản 2D hay 3D đều được).
2. Copy thư mục `Assets/GameKit` vào `Assets/` của project.
3. Bấm **Play**.

Bạn sẽ thấy Main Menu ngay, kể cả khi đang mở scene trống. Lý do: `GameKitRoot` tự khởi động qua `[RuntimeInitializeOnLoadMethod]`.

Nếu muốn có scene bootstrap thật: **Tools ▸ GameKit ▸ Create Bootstrap Scene** (tự tạo `Assets/Scenes/Bootstrap.unity` và đặt nó làm scene đầu trong Build Settings). Sau đó đặt `AutoBootstrap = false` trong `GameKitConfig`.

## Demo có sẵn để test vòng lặp

Mini-game demo: bấm vào vòng tròn trước khi nó co lại và biến mất. Hụt một vòng là mất một mạng. Nó tồn tại chỉ để chứng minh chuỗi **Menu → Play → Pause → Game Over → lưu high score** chạy thông suốt. Xoá hoặc tắt lúc nào cũng được.

---

## Cấu trúc

```
Assets/GameKit/
├── Editor/
│   └── GameKitMenu.cs          Tools ▸ GameKit ▸ ... (tạo scene, mở/xoá save)
└── Runtime/
    ├── Core/
    │   ├── GameKitConfig.cs    ★ MỌI THỨ BẠN CẦN CHỈNH Ở ĐÂY (tên game, màu, số mạng)
    │   ├── GameKitRoot.cs      Singleton xuyên scene, dựng mọi manager
    │   ├── GameManager.cs      State machine + score/lives + event
    │   └── InputCompat.cs      Chạy được với cả Input System cũ lẫn mới
    ├── Save/SaveSystem.cs      settings.json + save.json
    ├── Audio/
    │   ├── AudioManager.cs     Crossfade nhạc, pool 8 SFX source
    │   └── ProceduralAudio.cs  Sinh SFX + nhạc nền bằng code
    ├── Scenes/
    │   ├── SceneLoader.cs      LoadSceneAsync + min loading time
    │   └── ScreenFader.cs      Màn fade đen (sortingOrder 999)
    ├── UI/
    │   ├── UIFactory.cs        ★ Hàm dựng button/slider/toggle/panel
    │   ├── UIManager.cs        ★ Canvas + stack màn hình + nối UI với GameState
    │   ├── UIScreen.cs         Lớp cha của mọi màn hình
    │   └── MainMenu / Settings / Pause / GameOver / Loading / Hud Screen.cs
    └── Demo/DemoGame.cs        Mini-game demo — xoá khi có gameplay thật
```

---

## Gắn game thật của bạn vào

### Cách 1 — Gameplay nằm ở scene riêng (khuyên dùng)

```csharp
// GameKitConfig.cs
public const string GameSceneName = "Level01";   // nhớ Add Open Scenes vào Build Settings
```

Xong. Nút PLAY sẽ fade đen → hiện Loading screen → load async → vào `Playing`. `DemoGame` tự tắt khi chuỗi này khác rỗng.

Trong script gameplay của bạn:

```csharp
using GameKit;

GameManager.Instance.AddScore(100);
GameManager.Instance.LoseLife();          // về 0 mạng thì tự EndGame() + lưu high score
GameManager.Instance.EndGame();           // kết thúc thủ công

AudioManager.Instance.PlaySfx(myExplosionClip, 1f, Random.Range(0.9f, 1.1f));
AudioManager.Instance.PlayMusic(myBossMusic);

SaveSystem.Save.lastLevel = 3;
SaveSystem.SaveGame();
```

Nghe trạng thái thay vì hỏi liên tục:

```csharp
void OnEnable()  { GameManager.StateChanged += OnState; }
void OnDisable() { GameManager.StateChanged -= OnState; }

void OnState(GameState prev, GameState next)
{
    enabled = next == GameState.Playing;   // ví dụ: tắt AI khi pause
}
```

### Cách 2 — Gameplay nằm chung scene

Để `GameSceneName = ""`, xoá `DemoGame`, rồi đặt script gameplay của bạn lắng nghe `GameState.Playing` như trên.

---

## Thêm một màn hình mới

```csharp
public class CreditsScreen : UIScreen
{
    public override bool IsOverlay => true;

    protected override void Build()
    {
        UIFactory.MakeFullscreenImage(transform, "Scrim", Theme.Scrim);

        var column = UIFactory.MakeColumn(transform, "Content", 16f, 600f);
        column.anchorMin = column.anchorMax = column.pivot = new Vector2(0.5f, 0.5f);

        UIFactory.MakeLabel(column, "Header", "CREDITS", Theme.FontHeader, Theme.Text);
        UIFactory.MakeLabel(column, "Names", "Code: ...\nArt: ...", Theme.FontBody, Theme.TextDim);
        UIFactory.MakeButton(column, "BACK", () => UI.Pop());
    }
}
```

Mở nó: `UI.Push<CreditsScreen>()` (chồng lên) hoặc `UIManager.Instance.Show<CreditsScreen>()` (thay nền).

## Đổi giao diện

Mọi màu và cỡ chữ nằm trong `Theme` (file `GameKitConfig.cs`). Đổi `Theme.Accent` là đổi màu toàn bộ nút chính, slider, vòng tròn demo.

## Thay âm thanh sinh bằng code bằng file thật

`AudioManager` chỉ sinh clip cho những ô còn trống. Chọn object `[GameKit]` lúc đang Play → kéo file `.wav/.ogg` vào các ô của AudioManager. Muốn cố định thì tạo prefab từ `[GameKit]` và gán clip trong prefab đó.

---

## Những thứ đã xử lý sẵn (hay gãy vào đêm cuối jam)

| Vấn đề | Cách kit xử lý |
|---|---|
| Project bật Input System mới → `Input.GetKeyDown` ném exception | `InputCompat` biên dịch theo define, `UIManager` gắn đúng input module cho EventSystem |
| Pause bằng `Time.timeScale = 0` làm UI animation đứng | Mọi fade dùng `Time.unscaledDeltaTime` |
| Save hỏng khi game crash giữa lúc ghi | Ghi `.tmp` rồi mới thay thế file thật |
| Build WebGL lên itch.io mất save | Tự chuyển sang PlayerPrefs |
| Scene trống không có camera → UI đen thui, không có tiếng | `GameKitRoot` tự tạo camera + AudioListener dự phòng |
| Bấm Esc trong Settings lại resume game | `UIScreen.ConsumesPauseKey` cho màn hình trên cùng nuốt phím |
| "Enter Play Mode without domain reload" làm static bị kẹt | `ResetStatics()` ở mọi singleton |

---

## Checklist 7 ngày

| Ngày | Việc |
|---|---|
| 1 | Chốt core loop. Copy kit vào, đặt `GameTitle`, `Theme.Accent`, số mạng. Prototype gameplay thô — chưa cần đẹp. |
| 2 | Gameplay chạy được từ đầu đến `EndGame()`. Nối `AddScore` / `LoseLife`. |
| 3 | Tinh chỉnh cảm giác chơi: tốc độ, độ khó tăng dần, feedback khi trúng/hụt. |
| 4 | Art + SFX thật thay dần cho clip sinh bằng code. Đây là ngày dễ trượt lịch nhất — đặt hạn cứng. |
| 5 | **Build thử lần đầu.** Đừng để đến ngày 7. Kiểm tra fullscreen, save, âm lượng trên bản build. |
| 6 | Polish: juice, particle, màn hình credits, tutorial 1 câu. |
| 7 | Chỉ sửa bug + build cuối + viết mô tả trang submit. Không thêm tính năng mới. |

Một mẹo: build lần đầu vào **ngày 5**, không phải ngày cuối. Lỗi chỉ xuất hiện trên bản build (thiếu scene trong Build Settings, shader bị strip, save path) là nguyên nhân số một khiến team lỡ deadline jam.
