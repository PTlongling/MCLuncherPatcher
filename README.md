# LauncherPatcher

PCL（Plain Craft Launcher 2）与 HMCL 启动器的本地修复工具。检测启动器状态，一键修复被限制的功能，自动备份、可随时还原。

## 功能

### PCL 补丁（6 项）

| 补丁 | 说明 |
|---|---|
| Minecraft 版本列表 | 恢复"下载"页的游戏版本列表与安装功能 |
| 手动安装整合包 | 恢复"安装整合包"按钮的文件选择功能 |
| 整合包安装 | 恢复 CurseForge、Modrinth、MCBBS 等整合包的安装 |
| 恢复下载页卡片 | 将"下载"页被隐藏的 Minecraft 卡片恢复为可见 |
| 解除下载限制 | 将 `ModSecret.RestrictMcDownload` 常量由 true 改为 false |
| 停用自动更新 | 防止启动器自动更新后再次关闭下载功能 |

### HMCL 补丁（2 项）

| 补丁 | 说明 |
|---|---|
| 停用地区网络探测 | 让 `RegionUtils` 的机器与网络地区探测（5 个方法）始终返回"非大陆" |
| 跳过启动器自校验 | 跳过完整性自校验，等效于 `-Dhmcl.self_integrity_check.disable=true` |

HMCL 的 `.exe` 与 `.jar` 发行形式均可识别（`.exe` 为原生启动 stub + 尾部内嵌 jar，补丁在字节码层面重建目标方法后与 stub 重新拼接）。

## 用法

1. 把 `LauncherPatcher.exe` 放到启动器所在文件夹，运行时自动识别（也支持点击"选择文件"或把启动器文件拖入窗口）。
2. 本机还没有启动器时，主界面会出现「下载启动器」卡片，可一键下载 HMCL 3.17.0.362 或 PCL 正式版（经 GitHub 加速镜像；PCL 为压缩包，下载后自动解压），完成后自动检测。
3. 检测完成后点击"一键修复"；原文件自动备份为 `*.bak`。
4. 需要恢复时点击"还原"，用备份覆盖回修复前的启动器。

启动 `.jar` 形式的 HMCL 需要本机已安装 Java（`javaw.exe` / `java.exe`）。

## 运行环境

- Windows，.NET Framework 4.8
- 单文件发行：dnlib 以嵌入资源形式打包进 `LauncherPatcher.exe`，无需附带其他文件

## 构建

```cmd
dotnet build src/LauncherPatcher/LauncherPatcher.csproj -c Release
```

产物位于 `src/LauncherPatcher/bin/Release/LauncherPatcher.exe`。

## 来源

本项目整合自三个独立工具：

- **[PCLPlus](https://github.com/Xocio/PCLPlus)** — PCL 修复工具（WPF 界面与 4 项 IL 补丁）
- **[hmcl-patcher](https://github.com/Ryorin1337/hmcl-patcher)** — HMCL 补丁器（jar 内嵌提取与 JVM class 字节码重写）
- **[pcl-patcher](https://github.com/Ryorin1337/pcl-patcher)** — PCL 补丁器（下载限制常量与下载页卡片 2 处补丁）

## 鸣谢

以下补丁代码移植自社区开源作品，感谢原作者：

- **[hmcl-patcher](https://github.com/Ryorin1337/hmcl-patcher)**（Ryorin1337）：HMCL 侧的全部补丁能力——PE stub 与内嵌 jar 的剥离/拼接、`RegionUtils` 与 `IntegrityChecker` 的 JVM class 字节码重建逻辑，移植为 `src/LauncherPatcher/Hmcl/`。
- **[pcl-patcher](https://github.com/Ryorin1337/pcl-patcher)**（Ryorin1337）：PCL 侧的「解除下载限制」（`RestrictMcDownload` 常量翻转，对应 `RestrictConstantPatch`）与「恢复下载页卡片」（`CardMinecraft.Visibility`，对应 `CardVisibilityPatch`）两处补丁。
- **[PCLPlus](https://github.com/Xocio/PCLPlus)**（Xocio）：WPF 界面、补丁框架与其余 4 项 PCL 补丁。
- **[dnlib](https://github.com/0xd4d/dnlib)**：.NET 程序集读写库。

## 作者

- **Xocio** — PCLPlus 原作者（[github.com/Xocio](https://github.com/Xocio)）
- **Ryorin1337** — hmcl-patcher 与 pcl-patcher 作者（[github.com/Ryorin1337](https://github.com/Ryorin1337)）
- **Skyearth1** — 二创整合（[github.com/PTlongling](https://github.com/PTlongling)）

## 许可证

GPL-3.0。PCL 与 HMCL 均为 GPL-3.0 软件，本工具只修改你**本地持有**的副本；如需分发修改后的启动器，请遵守 GPL-3.0 的相应义务（附源码等）。

仅供学习研究使用。本项目为社区项目，与 PCL、HMCL 及其作者无关；请支持官方项目。
