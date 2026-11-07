using CloneDash.Common.Compatibility.Valve;
using CloneDash.Compatibility.Valve;
using Nucleus;

namespace CloneDash.Compatibility.MuseDash
{
	public static partial class MuseDash1Compatibility
	{
		private static string CustomInstallPathFile =>
			Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"CloneDash",
				"MuseDashInstallPath.txt"
			);

		private static bool IsValidMuseDashDirectory(string? directory) {
			if (string.IsNullOrWhiteSpace(directory))
				return false;

			return Directory.Exists(Path.Combine(directory, "MuseDash_Data"));
		}

		private static string? FindMuseDashDirectory(string directory) {
			// Check the directory itself.
			if (IsValidMuseDashDirectory(directory))
				return directory;

			// Check common wrapper layouts.
			string[] nestedNames = ["MuseDash", "Muse Dash"];

			foreach (string nestedName in nestedNames) {
				string nested = Path.Combine(directory, nestedName);

				if (IsValidMuseDashDirectory(nested))
					return nested;
			}

			return null;
		}

		private static string? ResolveMuseDashDirectory(string exePath) {
			if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
				return null;

			string? selectedDir = Path.GetDirectoryName(exePath);

			if (string.IsNullOrWhiteSpace(selectedDir))
				return null;

			// Check selected directory and its known game subfolders.
			string? result = FindMuseDashDirectory(selectedDir);

			if (result != null)
				return result;

			// Check parent directory and its known game subfolders.
			string? parentDir = Path.GetDirectoryName(selectedDir);

			if (!string.IsNullOrWhiteSpace(parentDir)) {
				result = FindMuseDashDirectory(parentDir);

				if (result != null)
					return result;
			}

			return null;
		}

		private static string? LoadSavedMuseDashDirectory() {
			try {
				if (!File.Exists(CustomInstallPathFile))
					return null;

				string savedPath = File.ReadAllText(CustomInstallPathFile).Trim();

				return IsValidMuseDashDirectory(savedPath) ? savedPath : null;
			}
			catch {
				return null;
			}
		}

		private static void SaveMuseDashDirectory(string directory) {
			try {
				string? parent = Path.GetDirectoryName(CustomInstallPathFile);

				if (!string.IsNullOrWhiteSpace(parent))
					Directory.CreateDirectory(parent);

				File.WriteAllText(CustomInstallPathFile, directory);
			}
			catch {
				// Saving the path is optional; initialization can still succeed.
			}
		}

		private static bool TryResolveCustomMuseDashInstallPathFromDialog(out string? installDir) {
			installDir = null;

			var defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

			var picked = Platform.OpenFileDialog(
				"Select Muse Dash.exe or its launcher",
				defaultPath,
				["*.exe"],
				"Executable files"
			);

			if (picked.Cancelled)
				return false;

			string? resolved = ResolveMuseDashDirectory(picked.Result);

			if (resolved == null)
				return false;

			installDir = resolved;
			SaveMuseDashDirectory(resolved);

			return true;
		}

		private static MD1CompatLayerInitResult INIT_WINDOWS() {
			if (!OperatingSystem.IsWindows())
				return MD1CompatLayerInitResult.OperatingSystemNotCompatible;

			string? musedash_installdir = SteamGames.WhereIsGameInstalled(MUSEDASH_APPID);

			// Try the previously saved custom installation.
			if (musedash_installdir == null)
				musedash_installdir = LoadSavedMuseDashDirectory();

			// Check whether the previously detected installation still exists.
			if (musedash_installdir != null &&
				!IsValidMuseDashDirectory(musedash_installdir)) {

				Platform.ShowMessageBox(
					"Muse Dash installation not found",
					"Sorry bro, your previously selected Muse Dash executable no longer exists or its installation has moved.\n\nPlease select the game executable again.",
					MessageBoxButtons.OK
				);

				musedash_installdir = null;
			}

			// Ask the user if neither Steam nor saved detection worked.
			if (musedash_installdir == null &&
				!TryResolveCustomMuseDashInstallPathFromDialog(out musedash_installdir))
				return MD1CompatLayerInitResult.MuseDashNotInstalled;

			WhereIsMuseDashInstalled = musedash_installdir;
			WhereIsMuseDashDataFolder = Path.Combine(musedash_installdir, "MuseDash_Data");

			string platform = "StandaloneWindows64";
			StandalonePlatform = platform;

			string musedash_streamingassets = Path.Combine(
				musedash_installdir,
				"MuseDash_Data",
				"StreamingAssets",
				"aa",
				platform
			);

			if (!Directory.Exists(musedash_streamingassets))
				return MD1CompatLayerInitResult.StreamingAssetsNotFound;

			BuildTarget = musedash_streamingassets;
			StreamingFiles = Directory.GetFiles(musedash_streamingassets);

			return MD1CompatLayerInitResult.OK;
		}
	}
}
