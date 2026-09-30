---
name: ugui-from-image
description: >-
  Builds a Unity UGUI screen from a reference image through Unity MCP.
  Use when the user gives a UI mock, screenshot, or design image and asks to
  generate a UGUI interface, recreate a screen in Unity, or 根据图片生成 UGUI.
---

# 按参考图生成 UGUI

把一张界面参考图做成可点击、可输入的 uGUI，并放进场景。只做图上能看到的控件。用户没要求的登录逻辑、跳转、存档不要加。

节点命名、目录划分、最简实现以项目规则为准：文本以 `Text` 结尾，图片以 `Img` 结尾，按钮以 `Btn` 结尾；贴图放 `Assets/Textures/UI/`。组件用现有的 `UIImage`、`UIText`、`UIButton`，输入框用 `TMP_InputField`。

## 流程

### 1. 接通 Unity MCP

1. `GetDynamicTools` 查看 `user-unityMCP`。若 `namespaceStatus` 不是 `ready`，先 `mcp_auth` 再查一次。
2. 读 `mcpforunity://instances`。多开编辑器时先 `set_active_instance`。
3. 读 `mcpforunity://editor/state`，等到 `data.advice.ready_for_tools` 为真，且不在编译、不在播放。
4. 调用任何工具前先看过它的 schema。

### 2. 读图并量出控件

看参考图，列出每个可见控件：背景、标题、面板、输入行、按钮、图标、底部入口。

用 `System.Drawing` 量像素（环境里通常没有 PIL）。坐标原点在图片左上角。记录每个控件的 `x, y, width, height`。面板按色块边界量，不要只靠目测。

场景：用户指定的场景优先。没指定时，用同名场景（登录图用 `LoginScene`）。不要把界面塞进已有玩法内容的场景。

### 3. 准备背景

把参考图复制到 `Assets/Textures/UI/<界面名>Bg.jpg`，作为全屏 `BackgroundImg`。

参考图上已经画了文字和控件。直接铺上去会和真 UGUI 叠成两层：

- 会被真文本盖住、但字形对不齐的区域（标题、底部标语、图标字），用附近像素做羽化贴图盖掉。
- 大色块（登录框、按钮）用同等位置的不透明 UGUI 盖住，不必从背景里抠掉。
- 面板必须半透明、要露出后面的场景时，先把该矩形用周围像素补上，再做半透明 `Image`。

羽化只处理要替换的区域，避免整块拉伸出硬边。

### 4. 用 execute_code 搭建

`compiler` 用 `codedom`（本机没有 Roslyn）。代码是方法体，不能写类，也不能写局部函数。用 lambda。lambda 参数名不要和后面的局部变量重名。

项目脚本类型用反射再转成基类，避免编译器找不到 `Assembly-CSharp`：

```csharp
var uiImageType = System.Type.GetType("UIImage, Assembly-CSharp");
var uiTextType = System.Type.GetType("UIText, Assembly-CSharp");
var uiButtonType = System.Type.GetType("UIButton, Assembly-CSharp");
var image = (UnityEngine.UI.Image)go.AddComponent(uiImageType);
var text = (TMPro.TextMeshProUGUI)go.AddComponent(uiTextType);
var button = (UnityEngine.UI.Button)go.AddComponent(uiButtonType);
```

字体：`Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset`。

画布：

- `Screen Space - Overlay`
- `CanvasScaler`：`Scale With Screen Size`，`1920x1080`，`matchWidthOrHeight = 0.5`
- 带 `GraphicRaycaster`
- 场景里要有 `EventSystem` + `StandaloneInputModule`
- 改完 `EditorSceneManager.SaveScene`

位置用参考图像素占整张图的比例做锚点，不要用写死的 Canvas 坐标。背景和图上的控件会一起缩放，换分辨率也不会错位。

```csharp
// x,y,w,h 是参考图像素，原点在左上角
rt.anchorMin = new Vector2(x / imgW, 1f - (y + h) / imgH);
rt.anchorMax = new Vector2((x + w) / imgW, 1f - y / imgH);
rt.offsetMin = Vector2.zero;
rt.offsetMax = Vector2.zero;
```

子控件用相对父面板的比例，同样的算法，分母换成父面板的设计宽高。

控件对应关系：

| 参考图 | UGUI |
| --- | --- |
| 全屏插画 | `BackgroundImg` |
| 标题、标签、按钮字 | `UIText` |
| 面板、输入条、分割线 | `UIImage`，圆角用带 `spriteBorder` 的白图，`Image.Type.Sliced` |
| 按钮 | 同一节点 `UIImage` + `UIButton`，节点名以 `Btn` 结尾，子文本以 `BtnText` 结尾 |
| 账号 | `TMP_InputField`，占位符用图上的点或提示字 |
| 密码 | `contentType = Password` |

图标做成小 PNG 放在 `Assets/Textures/UI/`，导入成 Sprite。文本 `raycastTarget = false`，点击落在按钮或输入框上。

### 5. 播放并对照参考图

编辑模式下截 Game 视图经常截不到 Overlay UI。必须：

1. `manage_editor` `action=play`
2. 等到 `editor/state` 里 `play_mode.is_changing` 为假
3. `manage_camera` `action=screenshot`，**不要指定 camera**（指定相机会丢掉 Overlay）
4. 读截图，和参考图比位置、间距、遮挡、重影
5. 不对就改锚点或颜色，再截一次
6. `manage_editor` `action=stop`
7. `read_console` 看有没有报错

截图有重影，就是背景上的旧字没盖干净，或控件比量到的色块小。截图只有天空盒，就是没进播放模式，或截图指定了相机。

## 完成标准

- 参考图上的每块文字、按钮、输入框都有对应节点，并且能点、能输入。
- 背景和控件对齐，没有两层字。
- 场景已保存，编辑器已退出播放模式。
