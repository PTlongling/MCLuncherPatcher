using System.Collections.Generic;

namespace LauncherPatcher.Localization
{
    public static partial class Text
    {
        private static readonly Dictionary<string, string> English = Table(
            // Title bar
            "App.Tag", "Repair Tool",
            "App.Minimize", "Minimize",
            "App.Close", "Close",
            "App.LanguageButton", "中文",
            "App.LanguageTooltip", "切换为中文",

            // Main page
            "Main.Launcher", "Launcher",
            "Main.NoLauncher", "No launcher selected",
            "Main.PickHint", "Click Browse to select a PCL or HMCL launcher file, or drag it into this window",
            "Main.Browse", "Browse",
            "Main.OpenFolder", "Open folder",
            "Main.LaunchPcl", "Launch",
            "Main.Patches", "Repairs",
            "Main.About", "About",
            "Main.AboutTooltip", "Version, author and source code",
            "Main.Restore", "Restore",
            "Main.RestoreTooltip", "Restore the launcher from its backup",
            "Main.Fix", "Repair",
            "Main.Fixing", "Repairing…",
            "Main.Fixed", "Repaired",
            "Main.Download", "Get a launcher",
            "Main.DownloadHmcl", "HMCL 3.17.0.362",
            "Main.DownloadHmclHint", "Official release, via a GitHub mirror",
            "Main.DownloadPcl", "PCL release",
            "Main.DownloadPclHint", "Zip archive, extracted automatically",
            "Main.DownloadButton", "Download",

            // Status messages
            "Status.Searching", "Looking for the launcher…",
            "Status.NotFoundNearby", "No launcher found in the LauncherPatcher folder. Click Browse to select it.",
            "Status.Inspecting", "Checking the launcher…",
            "Status.InspectFailed", "Check failed: {0}",
            "Status.Fixing", "Repairing, please wait…",
            "Status.FixDone", "Done! You can now download the game with the launcher. The original file was backed up as {0}.",
            "Status.FixUnexpected", "Unexpected error; the launcher was not changed: {0}",
            "Status.Restoring", "Restoring…",
            "Status.Restored", "The launcher has been restored to its original version.",
            "Status.Unexpected", "Unexpected error: {0}",
            "Status.LaunchFailed", "Failed to launch: {0}",
            "Status.Downloading", "Downloading the launcher, please wait…",
            "Status.DownloadFailed", "Download failed: {0}",

            // Dialogs
            "Dialog.SelectTitle", "Select the launcher",
            "Dialog.SelectFilter", "Launcher (*.exe;*.jar)|*.exe;*.jar",
            "Dialog.JavaNotFound", "Java runtime not found. Install Java to launch HMCL.",
            "Dialog.BrowserFailed", "Could not open the browser. Please visit:\n{0}\n\n{1}",
            "Dialog.DownloadFailed", "Download failed: {0}",
            "Dialog.DownloadNoExe", "Download failed: no launcher executable found in the archive.",
            "Dialog.DownloadBadZip", "Download failed: the download is not a valid archive. Please try again later.",

            // About page
            "About.Header", "About",
            "About.Summary", "Enhancement and repair toolkit for the PCL and HMCL launchers. Free and open source.",
            "About.AuthorRole", "Author · github.com/Xocio",
            "About.Author2Role", "Derivative integration · github.com/PTlongling",
            "About.Homepage", "Profile",
            "About.Notes", "Notes",
            "About.Note1", "· LauncherPatcher is a community project, not affiliated with PCL, HMCL or their authors.",
            "About.Note2", "· Repairs run locally; the only network activity is downloading a launcher when you click it yourself. The original launcher is backed up and can be restored at any time.",
            "About.Note3", "· A repaired launcher no longer updates itself. Run LauncherPatcher again after upgrading the launcher.",
            "About.Note4", "· Credits: the HMCL patches are ported from hmcl-patcher; the \"download restriction\" and \"download page card\" PCL patches are ported from pcl-patcher. Thanks to Ryorin1337.",
            "About.Back", "Back",

            // Patch states
            "PatchState.Pending", "Pending",
            "PatchState.Restored", "Restored",
            "PatchState.Disabled", "Disabled",
            "PatchState.Normal", "OK",
            "PatchState.NotNeeded", "Not needed",
            "PatchState.NotFound", "Not found",

            // Patches
            "Patch.VersionList.Title", "Minecraft version list",
            "Patch.VersionList.Description", "Restores the version list and game installation on the Download page",
            "Patch.ModpackPicker.Title", "Install modpack from file",
            "Patch.ModpackPicker.Description", "Restores file selection for the Install Modpack button",
            "Patch.ModpackDispatch.Title", "Modpack installation",
            "Patch.ModpackDispatch.Description", "Restores CurseForge, Modrinth, MCBBS and other modpack formats",
            "Patch.CardVisibility.Title", "Download page card",
            "Patch.CardVisibility.Description", "Makes the hidden Minecraft card on the Download page visible again",
            "Patch.RestrictConstant.Title", "Lift download restriction",
            "Patch.RestrictConstant.Description", "Flips the ModSecret.RestrictMcDownload constant from true to false",
            "Patch.UpdateBlock.Title", "Disable auto-update",
            "Patch.UpdateBlock.Description", "Keeps an automatic update from disabling downloads again",
            "Patch.RegionUtils.Title", "Disable region detection",
            "Patch.RegionUtils.Description", "Makes HMCL's machine and network region probes (the 5 RegionUtils methods) always report \"not China mainland\"",
            "Patch.IntegrityChecker.Title", "Skip self integrity check",
            "Patch.IntegrityChecker.Description", "Skips HMCL's integrity self check, equivalent to -Dhmcl.self_integrity_check.disable=true",

            // Launcher inspection
            "Inspect.FileMissing", "The file does not exist.",
            "Inspect.NotOfficial", "This is not a supported launcher (official PCL or HMCL); no repair needed.",
            "Inspect.ReadFailed", "Cannot read the file: {0}",
            "Inspect.AccessDenied", "Access to the file is denied.",
            "Inspect.Patched", "The launcher has been repaired by LauncherPatcher.",
            "Inspect.NotBlocked", "This version of the launcher works as-is; no repair needed.",
            "Inspect.Blocked", "This launcher has restricted features. Click Repair to restore them.",
            "Inspect.Unsupported", "This launcher has restricted features, but its layout is not recognized yet. Repair is not supported.",

            // Repair and restore
            "Fix.PatchFailed", "Failed to apply \"{0}\": {1}",
            "Fix.WriteFailed", "Failed to write the file: {0}\nIf the launcher is in a protected folder such as Program Files, run LauncherPatcher as administrator.",
            "Fix.NoBackup", "No backup found; nothing to restore.",
            "Fix.BadBackup", "The backup is not a recognized launcher; restore cancelled.",
            "Fix.RestoreFailed", "Restore failed: {0}",
            "Fix.ReadFailed", "Cannot read the launcher: {0}",
            "Fix.Running", "The launcher is running. Close it and try again.",
            "Fix.BuildFailed", "Failed to build the repaired launcher: {0}",
            "Fix.VerifyFailed", "The repaired launcher failed verification; the original file was not changed.");
    }
}
