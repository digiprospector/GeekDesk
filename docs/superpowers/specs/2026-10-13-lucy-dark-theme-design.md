# Lucy 深色扁平主题设计

## 目标

将 GeekDesk 主界面（MainWindow）从当前的浅色圆角卡片风格改为与 Lucy 启动器一致的深色扁平风格。已确认完全照搬 Lucy：不保留现有毛玻璃、圆角、阴影和壁纸透明度逻辑。

## 布局结构

整体由「顶栏 + 左侧分类 Tab + 右侧图标网格」组成：

- **顶栏（高约 50px）**
  - 左侧：文字 Logo「Lucy」（可沿用应用名 GeekDesk），主题蓝色 `#4AA3FF`，字号约 22px，加粗。
  - 中间：自动日期「10月13日 星期二 九月初四」，灰色 `#9C9C9C`，字号约 14px。日期随系统更新。
  - 右侧：搜索、菜单（替换现有设置入口的下拉）、关闭 三个图标按钮，灰色 `#AAAAAA`，间距约 14px。
  - 底部 1px 分隔线 `#383838`，顶栏背景 `#2B2B2B`。
- **左侧分类栏（宽约 120px）**
  - 背景 `#2B2B2B`，右侧 1px 边框 `#383838`。
  - 列表项高约 40px，文字 16px，默认色 `#C9C9C9`，左对齐，左缩进约 14px。
  - **选中态**：整行亮灰背景 `#3D3D3D`，文字白色，左边缘 3px 标准蓝竖条 `#2196F3`。
  - 悬浮态：背景微亮（`#333333`），无竖条。
- **右侧图标网格**
  - 背景 `#1F1F1F`（比侧栏更深，形成层次）。
  - VirtualizingWrapPanel 保留现有实现，格子尺寸沿用现有 ImgPanel 宽高配置。
  - 图标下方名称：13px，颜色 `#E6E6E6`。
  - **悬浮/选中态**：整格背景 `#3A3A3A`，无圆角。

## 样式概览（关键颜色）

| 用途 | 色值 |
| --- | --- |
| 顶栏 / 左侧栏背景 | `#2B2B2B` |
| 右侧网格背景 | `#1F1F1F` |
| 分隔线 / 边框 | `#383838` |
| Logo 文字 | `#4AA3FF` |
| 日期 / 次要文字 | `#9C9C9C` |
| 分类默认文字 | `#C9C9C9` |
| 分类选中背景 | `#3D3D3D` |
| 分类选中竖条 | `#2196F3` |
| 图标悬浮背景 | `#3A3A3A` |
| 图标名称文字 | `#E6E6E6` |

字体统一使用系统默认（不额外加粗，除 Logo 外）。去除现有 `EffectShadow2`、`BorderThickness="1"` 卡片边框、以及 `CornerRadius` 圆角，改为直角扁平。

## 实现范围

- `MainWindow.xaml`
  - Window 背景改为深色。
  - 重写顶栏区域为上述三段式结构。
  - 移除/合并现有 Logo Image、设置/关闭按钮的 Presentation，保留事件绑定（`SettingButton`、关闭、菜单 ContextMenu 逻辑不变，仅样式替换）。
- `Control/UserControls/PannelCard/LeftCardControl.xaml`
  - 移除外层 `hc:Card`（阴影/圆角/背景动画），改为普通 Grid + Border。
  - 重写 `MenuStyle` 为深色 Tab 样式，含选中竖条（可用 Border 的 `BorderThickness="3,0,0,0"` 或单独 Rectangle 实现）。
  - 删除现有 FontSize 缩放动画。
- `Control/UserControls/PannelCard/RightCardControl.xaml`
  - 同样移除 `hc:Card` 外壳，改用 Grid。
  - `IconListBox` 添加 ItemContainerStyle 的悬浮/选中背景触发器。
- `Resource/Dictionary/CommonStyle.xaml`
  - 新增一组 `Lucy*` 前缀的画刷与样式资源（颜色集中管理，方便以后切回原主题）。

## 不改动的部分

- 所有 ViewModel、业务逻辑、拖拽排序、搜索、右键菜单、待办、Everything 插件等。
- 设置窗口（ConfigWindow）、ToDo 窗口、通知等二级弹窗样式。
- NotifyIcon 托盘图标。

## 风险与注意

- 现有代码大量使用 `CardOpacity`、`TextColor` 等 AppConfig 绑定；本次改动后这两个配置项对主面板不再生效，需在设置页隐藏或标注「深色主题下无效」。
- `ShowIconTitle` 开关保留，但默认颜色由 AppConfig.TextColor 改为固定 `#E6E6E6`（深色固定主题）。
- 拖拽交换命令、右键菜单 Handler 必须原样保留，避免功能回归。

## 验证方式

1. `dotnet build` 编译通过。
2. 手动运行，核对顶栏三段、左侧 Tab 选中态（蓝色竖条）、右侧图标悬浮背景与截图一致。
3. 验证拖拽图标换位、右键菜单、搜索弹出等核心交互不回归。
