using System.Collections.Generic;

namespace LauncherPatcher.Localization
{
    public static partial class Text
    {
        private static readonly Dictionary<string, string> Chinese = Table(
            // Title bar
            "App.Tag", "修复工具",
            "App.Minimize", "最小化",
            "App.Close", "关闭",
            "App.LanguageButton", "EN",
            "App.LanguageTooltip", "Switch to English",

            // Main page
            "Main.Launcher", "启动器",
            "Main.NoLauncher", "未选择启动器",
            "Main.PickHint", "点击右侧按钮选择 PCL 或 HMCL 启动器文件，或直接把它拖到本窗口",
            "Main.Browse", "选择文件",
            "Main.OpenFolder", "打开所在文件夹",
            "Main.LaunchPcl", "启动",
            "Main.Patches", "修复内容",
            "Main.About", "关于",
            "Main.AboutTooltip", "版本、作者与开源仓库",
            "Main.Restore", "还原",
            "Main.RestoreTooltip", "用备份文件恢复修复前的启动器",
            "Main.Fix", "一键修复",
            "Main.Fixing", "正在修复……",
            "Main.Fixed", "已修复",
            "Main.Download", "下载启动器",
            "Main.DownloadHmcl", "HMCL 3.17.0.362",
            "Main.DownloadHmclHint", "官方发布版，来自 GitHub 加速镜像",
            "Main.DownloadPcl", "PCL 正式版",
            "Main.DownloadPclHint", "压缩包，下载后自动解压",
            "Main.DownloadButton", "下载",

            // Status messages
            "Status.Searching", "正在查找启动器……",
            "Status.NotFoundNearby", "没有在 LauncherPatcher 所在的文件夹中找到启动器，请点击“选择文件”手动选择。",
            "Status.Inspecting", "正在检测启动器……",
            "Status.InspectFailed", "检测失败：{0}",
            "Status.Fixing", "正在修复，请稍候……",
            "Status.FixDone", "修复完成！现在可以用启动器下载游戏了。原文件已备份为 {0}。",
            "Status.FixUnexpected", "出现意外错误，启动器未被修改：{0}",
            "Status.Restoring", "正在还原……",
            "Status.Restored", "已还原为修复前的启动器。",
            "Status.Unexpected", "出现意外错误：{0}",
            "Status.LaunchFailed", "启动失败：{0}",
            "Status.Downloading", "正在下载启动器，请稍候……",
            "Status.DownloadFailed", "下载失败：{0}",

            // Dialogs
            "Dialog.SelectTitle", "选择启动器",
            "Dialog.SelectFilter", "启动器 (*.exe;*.jar)|*.exe;*.jar",
            "Dialog.JavaNotFound", "未找到 Java 运行时，无法启动 HMCL。请先安装 Java。",
            "Dialog.BrowserFailed", "无法打开浏览器，请手动访问：\n{0}\n\n{1}",
            "Dialog.DownloadFailed", "下载失败：{0}",
            "Dialog.DownloadNoExe", "下载失败：压缩包中没有找到启动器程序。",
            "Dialog.DownloadBadZip", "下载失败：下载的内容不是有效的压缩包，请稍后重试。",

            // About page
            "About.Header", "关于",
            "About.Summary", "PCL 与 HMCL 启动器的增强与修复工具，开源免费。",
            "About.AuthorRole", "作者 · github.com/Xocio",
            "About.Author2Role", "二创整合 · github.com/PTlongling",
            "About.Homepage", "主页",
            "About.Notes", "说明",
            "About.Note1", "· LauncherPatcher 是社区项目，与 PCL、HMCL 及其作者无关。",
            "About.Note2", "· 修复只在本机进行；除你主动点击「下载启动器」外，本工具没有任何联网行为。原启动器会自动备份，可随时还原。",
            "About.Note3", "· 修复后启动器不再自动更新，升级启动器后请重新运行 LauncherPatcher。",
            "About.Note4", "· 鸣谢：HMCL 补丁代码移植自 hmcl-patcher，PCL 的「解除下载限制」「恢复下载页卡片」补丁移植自 pcl-patcher，感谢 Ryorin1337。",
            "About.Back", "返回",

            // Patch states
            "PatchState.Pending", "待修复",
            "PatchState.Restored", "已恢复",
            "PatchState.Disabled", "已停用",
            "PatchState.Normal", "正常",
            "PatchState.NotNeeded", "无需修复",
            "PatchState.NotFound", "未找到",

            // Patches
            "Patch.VersionList.Title", "Minecraft 版本列表",
            "Patch.VersionList.Description", "恢复“下载”页的游戏版本列表与安装功能",
            "Patch.ModpackPicker.Title", "手动安装整合包",
            "Patch.ModpackPicker.Description", "恢复“安装整合包”按钮的文件选择功能",
            "Patch.ModpackDispatch.Title", "整合包安装",
            "Patch.ModpackDispatch.Description", "恢复 CurseForge、Modrinth、MCBBS 等整合包的安装",
            "Patch.CardVisibility.Title", "恢复下载页卡片",
            "Patch.CardVisibility.Description", "将“下载”页被隐藏的 Minecraft 卡片恢复为可见",
            "Patch.RestrictConstant.Title", "解除下载限制",
            "Patch.RestrictConstant.Description", "将 ModSecret.RestrictMcDownload 常量由 true 改为 false",
            "Patch.UpdateBlock.Title", "停用自动更新",
            "Patch.UpdateBlock.Description", "防止启动器自动更新后再次关闭下载功能",
            "Patch.RegionUtils.Title", "停用地区网络探测",
            "Patch.RegionUtils.Description", "让 HMCL 的机器与网络地区探测（RegionUtils 的 5 个方法）始终返回“非大陆”",
            "Patch.IntegrityChecker.Title", "跳过启动器自校验",
            "Patch.IntegrityChecker.Description", "跳过 HMCL 的完整性自校验，等效于 -Dhmcl.self_integrity_check.disable=true",

            // Launcher inspection
            "Inspect.FileMissing", "文件不存在。",
            "Inspect.NotOfficial", "这不是受支持的启动器（PCL 正式版或 HMCL 官方版），无需修复。",
            "Inspect.ReadFailed", "无法读取文件：{0}",
            "Inspect.AccessDenied", "没有读取该文件的权限。",
            "Inspect.Patched", "启动器已由 LauncherPatcher 修复，可以正常使用。",
            "Inspect.NotBlocked", "此版本的启动器一切正常，无需修复。",
            "Inspect.Blocked", "此版本的启动器存在受限功能，可以一键修复。",
            "Inspect.Unsupported", "此启动器存在受限功能，但代码结构与已知版本不同，暂不支持修复。",

            // Repair and restore
            "Fix.PatchFailed", "修复“{0}”失败：{1}",
            "Fix.WriteFailed", "写入文件失败：{0}\n如果启动器位于 Program Files 等受保护的文件夹，请以管理员身份运行 LauncherPatcher。",
            "Fix.NoBackup", "没有找到备份文件，无法还原。",
            "Fix.BadBackup", "备份文件不是可识别的启动器，已取消还原。",
            "Fix.RestoreFailed", "还原失败：{0}",
            "Fix.ReadFailed", "无法读取启动器：{0}",
            "Fix.Running", "启动器正在运行，请先关闭启动器再试。",
            "Fix.BuildFailed", "生成修复后的启动器失败：{0}",
            "Fix.VerifyFailed", "修复结果校验失败，原文件未做任何改动。");
    }
}
