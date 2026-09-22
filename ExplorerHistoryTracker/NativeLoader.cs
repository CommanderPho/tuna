using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ExplorerHistoryTracker
{
    /// <summary>
    /// Startup-time embedded native DLL loader.
    ///
    /// After NativeAOT single-exe publish, Avalonia native deps (libSkiaSharp / libHarfBuzzSharp / av_libglesv2)
    /// are gzip-embedded as exe resources. This class extracts them at process start (first line of Main,
    /// before Avalonia init) to %APPDATA%\Tuna\native and preloads them.
    ///
    /// Load strategy:
    ///  1. SetDllDirectoryW adds the extract directory to the process DLL search path
    ///  2. LoadLibraryExW preloads the DLLs (Windows dedupes by name; Avalonia LoadLibrary hits them)
    ///
    /// Failures are logged only; missing resources fall back to the default DLL search path.
    /// </summary>
    internal static class NativeLoader
    {
        private const string ResourcePrefix = "ExplorerHistoryTracker.Native.";

        private static readonly string[] NativeFileNames =
        {
            "libSkiaSharp.dll",
            "libHarfBuzzSharp.dll",
        };

        private static string? _extractDir;
        private static int _initialized;

        /// <summary>
        /// Entry point: must be called on the first line of Main, before Avalonia init. Idempotent.
        /// </summary>
        public static void Initialize()
        {
            if (System.Threading.Interlocked.CompareExchange(ref _initialized, 1, 0) != 0) return;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                _extractDir = ResolveExtractDirectory();

                var extracted = ExtractAll(assembly, _extractDir);
                if (extracted.Count == 0)
                {
                    // No embedded resources — not a single-exe publish; use default lookup
                    return;
                }

                // Add extract directory to the process DLL search path
                if (!SetDllDirectoryW(_extractDir))
                {
                    System.Diagnostics.Debug.WriteLine("NativeLoader: SetDllDirectoryW failed");
                }

                // Preload all DLLs so they enter the process module table first
                foreach (var file in NativeFileNames)
                {
                    PreLoadFromDisk(file);
                }

                System.Diagnostics.Debug.WriteLine("NativeLoader: initialized, dir=" + _extractDir);
            }
            catch (Exception ex)
            {
                // Fatal fallback: loader failure must never prevent the main app from starting
                System.Diagnostics.Debug.WriteLine("NativeLoader: " + ex.Message);
            }
        }

        private static string ResolveExtractDirectory()
        {
            // Extract under AppData/native so system cleaners are less likely to remove them
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tuna",
                "native");
        }

        /// <summary>
        /// Extract all embedded native resources to destDir. Skip existing files that are still fresh.
        /// </summary>
        private static List<string> ExtractAll(Assembly assembly, string destDir)
        {
            var done = new List<string>();
            try
            {
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                string[] allNames = assembly.GetManifestResourceNames();

                foreach (string fileName in NativeFileNames)
                {
                    string resourceName = ResourcePrefix + fileName;
                    int idx = Array.IndexOf(allNames, resourceName);
                    if (idx < 0) continue;

                    string destFile = Path.Combine(destDir, fileName);
                    if (NeedsRefresh(destFile))
                    {
                        using Stream? rs = assembly.GetManifestResourceStream(resourceName);
                        if (rs == null) continue;
                        using var gs = new GZipStream(rs, CompressionMode.Decompress);
                        using var fs = new FileStream(destFile, FileMode.Create, FileAccess.Write);
                        gs.CopyTo(fs);
                    }
                    done.Add(destFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NativeLoader ExtractAll: " + ex.Message);
            }
            return done;
        }

        /// <summary>Refresh if the target is missing, or the running exe is newer than the extracted DLL.</summary>
        private static bool NeedsRefresh(string destFile)
        {
            try
            {
                if (!File.Exists(destFile)) return true;

                string? exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    if (File.GetLastWriteTime(exePath) > File.GetLastWriteTime(destFile))
                        return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static IntPtr PreLoadFromDisk(string fileName)
        {
            try
            {
                if (_extractDir == null) return IntPtr.Zero;
                string full = Path.Combine(_extractDir, fileName);
                if (!File.Exists(full)) return IntPtr.Zero;

                IntPtr h = LoadLibraryExW(full, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
                if (h == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("NativeLoader: LoadLibraryExW failed: " + fileName);
                }
                return h;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NativeLoader PreLoad: " + fileName + " - " + ex.Message);
                return IntPtr.Zero;
            }
        }

        #region Win32 P/Invoke

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectoryW(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryExW(string lpLibFileName, IntPtr hFile, uint dwFlags);

        private const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

        #endregion
    }
}
