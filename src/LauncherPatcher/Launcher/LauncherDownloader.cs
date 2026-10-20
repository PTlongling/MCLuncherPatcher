using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LauncherPatcher.Launcher
{
    /// <summary>
    /// Downloads an official launcher when none is available locally.
    /// HMCL ships as a single .exe; PCL ships as a zip that has to be extracted.
    /// </summary>
    internal static class LauncherDownloader
    {
        private const string HmclUrl = "https://v6.gh-proxy.org/https://github.com/HMCL-dev/HMCL/releases/download/v3.17.0.362/HMCL-3.17.0.362.exe";
        private const string PclUrl = "https://v6.gh-proxy.org/https://github.com/Meloong-Git/PCL/blob/main/%E6%9C%80%E6%96%B0%E6%AD%A3%E5%BC%8F%E7%89%88.zip";

        private const string HmclFileName = "HMCL-3.17.0.362.exe";
        private const string PclFileName = "Plain Craft Launcher 2.exe";

        static LauncherDownloader()
        {
            // .NET Framework defaults to TLS 1.0/1.1, which the proxy and GitHub reject.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        /// <summary>Downloads the official HMCL release into <paramref name="directory"/>.</summary>
        /// <param name="progress">Receives the percentage 0–100, or −1 when the total size is unknown.</param>
        /// <returns>The path of the downloaded file.</returns>
        public static Task<string> DownloadHmclAsync(string directory, IProgress<double> progress)
        {
            string target = UniquePath(Path.Combine(directory, HmclFileName));
            return DownloadToFileAsync(HmclUrl, target, progress);
        }

        /// <summary>Downloads the official PCL zip and extracts the launcher into <paramref name="directory"/>.</summary>
        /// <param name="progress">Receives the percentage 0–100, or −1 when the total size is unknown.</param>
        /// <returns>The path of the extracted executable.</returns>
        public static async Task<string> DownloadPclAsync(string directory, IProgress<double> progress)
        {
            string tempZip = Path.Combine(directory, "LauncherPatcher-download.tmp");
            try
            {
                await DownloadToFileAsync(PclUrl, tempZip, progress);

                string target = UniquePath(Path.Combine(directory, PclFileName));
                try
                {
                    using (var zip = ZipFile.OpenRead(tempZip))
                    {
                        var entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                                && e.Name.IndexOf("Plain Craft Launcher", StringComparison.OrdinalIgnoreCase) >= 0)
                            ?? zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                        if (entry == null)
                            throw new LauncherException("Dialog.DownloadNoExe");

                        entry.ExtractToFile(target, overwrite: false);
                    }
                }
                catch (InvalidDataException)
                {
                    // The proxy returned something that is not a zip (an error page, most likely).
                    throw new LauncherException("Dialog.DownloadBadZip");
                }
                return target;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempZip))
                        File.Delete(tempZip);
                }
                catch (IOException)
                {
                }
            }
        }

        private static async Task<string> DownloadToFileAsync(string url, string target, IProgress<double> progress)
        {
            try
            {
                using (var handler = new HttpClientHandler { AllowAutoRedirect = true })
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) })
                using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    long? total = response.Content.Headers.ContentLength;
                    if (!total.HasValue || total.Value <= 0)
                        progress?.Report(-1);

                    using (var source = await response.Content.ReadAsStreamAsync())
                    using (var file = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[81920];
                        long received = 0;
                        int lastPercent = -1;
                        for (;;)
                        {
                            int n = await source.ReadAsync(buffer, 0, buffer.Length);
                            if (n <= 0)
                                break;
                            await file.WriteAsync(buffer, 0, n);
                            received += n;

                            if (total.HasValue && total.Value > 0)
                            {
                                int percent = (int)(received * 100 / total.Value);
                                if (percent != lastPercent)
                                {
                                    lastPercent = percent;
                                    progress?.Report(percent);
                                }
                            }
                        }
                    }
                }
                return target;
            }
            catch (LauncherException)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is IOException)
            {
                TryDelete(target);
                throw new LauncherException(ex, "Dialog.DownloadFailed", ex.Message);
            }
        }

        /// <summary>Appends " (n)" before the extension when the file already exists.</summary>
        private static string UniquePath(string path)
        {
            if (!File.Exists(path))
                return path;

            string directory = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            for (int i = 1; ; i++)
            {
                string candidate = Path.Combine(directory, $"{name} ({i}){extension}");
                if (!File.Exists(candidate))
                    return candidate;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (IOException)
            {
            }
        }
    }
}
