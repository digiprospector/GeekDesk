# 新增"常用"菜单 — 修改方案

## 需求概述

在左侧菜单列表中新增一个固定的「常用」菜单：

- 「常用」本身不可删除、不可编辑菜单名。
- 里面的快捷方式也不可通过右键"从列表删除"。
- 内容根据每个快捷方式在全项目中累计使用次数（`IconInfo.Count`）自动排序，使用次数最多的若干个显示在「常用」中。

## 现状分析

- `AppData.MenuList : ObservableCollection<MenuInfo>` 是全部菜单来源；`MenuInfo.MenuType` 目前只有 `NORMAL / LINK` 两种。
- `IconInfo.Count` 字段已存在，`ProcessUtil.StartIconApp` 中通过 `icon.Count++` 递增并持久化。
- `RightCardControl.RemoveIcon` 是删除图标的入口。
- `LeftCardControl.DeleteMenu` 是删除菜单的入口。
- `CommonCode.SortIconList` 已有按 `Count` 排序逻辑，但只对单个 `MenuInfo` 内部排序，没有跨菜单聚合视图。

## 设计方案

### 1. 新增 `MenuType.FREQUENT`

`Constant/MenuType.cs`：

```csharp
public enum MenuType
{
    NORMAL,
    LINK,
    FREQUENT,  // 常用菜单（固定，不可删除）
}
```

### 2. 数据层：不持久化"常用"

「常用」作为虚拟菜单存在，不往 `AppData.MenuList` 序列化一个持久 `MenuInfo`。

- `AppData` 新增 `[NonSerialized]` 运行时属性 `FrequentMenu : MenuInfo`。
- 启动时构造：
  - `MenuId = "frequent"`
  - `MenuName = "常用"`
  - `MenuType = MenuType.FREQUENT`
  - `IconList` 为空集合，实际内容由 `FrequentStats` 动态计算后填入。
- 显示时插入 `MenuList` 头部：`MenuList.Insert(0, FrequentMenu)`。
- 序列化时通过 `[NonSerialized]` 或序列化回调跳过，保证旧数据兼容、不会被用户误存破坏。

好处：不改动已有序列化结构，旧数据无需迁移；删除/编辑入口用 `MenuType == FREQUENT` 拦截即可。

### 3. 统计逻辑：`Util/FrequentStats.cs`

```csharp
public static class FrequentStats
{
    public static List<IconInfo> GetTopIcons(AppData data, int topN)
    {
        // 遍历 data.MenuList 中 MenuType == NORMAL / LINK 的菜单，
        // 收集所有 IconInfo（跳过 FrequentMenu）。
        // 按 Count 降序、Name 升序 tiebreak，取前 topN。
        // 返回的 IconInfo 是原菜单中同一引用，Count++ 自动生效。
    }

    public static void Refresh(AppData data)
    {
        // 调用 GetTopIcons(data, data.AppConfig.FrequentIconCount)。
        // 与 data.FrequentMenu.IconList 做 diff，只对变化的项增删，
        // 不整体替换集合，避免 UI 闪烁。
    }
}
```

### 4. 禁止删除

#### 4.1 禁止删除"常用"菜单

`LeftCardControl.DeleteMenu` 开头：

```csharp
if (menuInfo.MenuType == MenuType.FREQUENT)
{
    HandyControl.Controls.Growl.Warning("「常用」菜单不可删除。");
    return;
}
```

同时在菜单右键 XAML 中，用 `MenuType == FREQUENT` 控制"删除"菜单项不可见（参考现有 `MenuEdit / NotMenuEdit` 的 Visibility 绑定方式）。

#### 4.2 禁止删除"常用"中的图标

`RightCardControl.RemoveIcon` 开头：

```csharp
var currentMenu = appData.MenuList[appData.AppConfig.SelectedMenuIndex];
if (currentMenu.MenuType == MenuType.FREQUENT)
{
    HandyControl.Controls.Growl.Warning("「常用」中的图标不能在此删除，请到所属菜单删除。");
    return;
}
```

批量 / 编辑入口（如有）用同样条件拦截。

由于"常用"里的 `IconInfo` 与源菜单同引用，从原菜单删除后 Refresh 会自动把它从"常用"里去掉。

### 5. UI 显示与刷新时机

启动后绑定前执行一次：

```
AppDataMenuList.Insert(0, appData.FrequentMenu);
FrequentStats.Refresh(appData);
```

刷新触发点：

| 时机 | 动作 |
|---|---|
| 程序启动 | `Refresh` |
| `icon.Count++` 后（`ProcessUtil.StartIconApp` 末尾） | `Refresh` |
| 图标增删后 | `Refresh` |
| `FrequentIconCount` 修改后 | `Refresh` |

`CommonCode.SortIconList` 内部加 `if (menuInfo.MenuType == MenuType.FREQUENT) continue;`，避免"常用"被全局排序逻辑打乱（"常用"固定按 `Count` 降序）。

### 6. 配置项

`AppConfig` 新增：

```csharp
private int frequentIconCount = 10; // 常用显示数量
public int FrequentIconCount { get => frequentIconCount; set { frequentIconCount = value; OnPropertyChanged("FrequentIconCount"); } }
```

"动作"配置页新增一个 Slider，范围 5~30，步长 1，绑定 `FrequentIconCount`。

### 7. 文件修改清单

| 文件 | 变更 |
|---|---|
| `Constant/MenuType.cs` | 新增 `FREQUENT` 枚举值 |
| `ViewModel/AppData.cs` | 新增 `[NonSerialized] FrequentMenu` 与启动时插头部逻辑 |
| `Util/FrequentStats.cs`（新文件） | 统计 + diff 刷新逻辑 |
| `Util/ProcessUtil.cs` | `icon.Count++` 后调 `FrequentStats.Refresh` |
| `Util/CommonCode.cs` | `SortIconList` 跳过 FREQUENT |
| `Control/UserControls/PannelCard/LeftCardControl.xaml(.cs)` | 拦截"常用"删除；右键菜单隐藏删除项 |
| `Control/UserControls/PannelCard/RightCardControl.xaml(.cs)` | 拦截"常用"中的 RemoveIcon |
| `ViewModel/AppConfig.cs` | 新增 `FrequentIconCount` |
| `Control/UserControls/Config/MotionControl.xaml`（或相应配置页） | 新增显示数量 Slider |

### 8. 可选优化

- 若希望"常用"不显示 `Count` 提示，可以在 `IconInfo.Content` 构造时判断 `MenuType == FREQUENT` 时换成 `Name + "\n使用次数: xx"`（不显式说明），或直接显示正常格式。
- 图标删除时同时 Refresh，保证"常用"实时收缩。
- 若源菜单被删除，其内部 `IconInfo` 随之消失，Refresh 后"常用"中对应项自动移除。
