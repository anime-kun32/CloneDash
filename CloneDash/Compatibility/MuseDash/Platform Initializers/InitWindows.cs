using CloneDash.Compatibility.Valve;

using Microsoft.Win32;

namespace CloneDash.Compatibility.MuseDash
{
	public static partial class MuseDashCompatibility
	{
		private static MDCompatLayerInitResult INIT_WINDOWS() {
			if (!OperatingSystem.IsWindows())
				return MDCompatLayerInitResult.OperatingSystemNotCompatible;

			// Where is Steam installed?
			string? steamInstallPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", null) as string;
			if (steamInstallPath == null) { // Sometimes the install path will be here instead
				steamInstallPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432NODE\\Valve\\Steam", "InstallPath", null) as string;
				if (steamInstallPath == null) {
					// Steam not found, ask user to manually select Muse Dash installation
					Logs.Warn("Steam installation not found. Prompting user to manually select Muse Dash installation...");
					string? manualPath = InstallationPathSelector.SelectMuseDashInstallation();
					if (manualPath == null)
						return MDCompatLayerInitResult.MuseDashNotInstalled;
					
					WhereIsMuseDashInstalled = manualPath;
					WhereIsMuseDashDataFolder = Path.Combine(manualPath, "MuseDash_Data");
					
					string platform = "StandaloneWindows64";
					StandalonePlatform = platform;
					
					string musedash_streamingassets = manualPath + $"\\MuseDash_Data\\StreamingAssets\\aa\\{platform}\\";
					if (!Directory.Exists(musedash_streamingassets))
						return MDCompatLayerInitResult.StreamingAssetsNotFound;
					
					BuildTarget = musedash_streamingassets;
					StreamingFiles = Directory.GetFiles(musedash_streamingassets);
					
					return MDCompatLayerInitResult.OK;
				}
			}

			// Figure out from Steam where Muse Dash is installed, if it is installed, otherwise break out
			ValveDataFile games = ValveDataFile.FromFile(steamInstallPath + "\\steamapps\\libraryfolders.vdf");
			string musedash_appid = "" + MUSEDASH_APPID;
			string musedash_installdir = "";
			bool musedash_installed = false;

			foreach (KeyValuePair<string, ValveDataFile.VDFItem> vdfItemPair in games["libraryfolders"]) {
				var apps = (vdfItemPair.Value["apps"] as ValveDataFile.VDFDict)!;
				if (apps.Contains(musedash_appid)) {
					ValveDataFile appManifest = ValveDataFile.FromFile(vdfItemPair.Value.GetString("path") + $"\\steamapps\\appmanifest_{musedash_appid}.acf");
					musedash_installed = true;
					musedash_installdir = vdfItemPair.Value.GetString("path") + "\\steamapps\\common\\" + appManifest["AppState"].GetString("installdir");
				}
			}

			if (!musedash_installed) {
				// Muse Dash not found in Steam, ask user to manually select installation
				Logs.Warn("Muse Dash not found in Steam libraries. Prompting user to manually select Muse Dash installation...");
				string? manualPath = InstallationPathSelector.SelectMuseDashInstallation();
				if (manualPath == null)
					return MDCompatLayerInitResult.MuseDashNotInstalled;
				
				WhereIsMuseDashInstalled = manualPath;
				WhereIsMuseDashDataFolder = Path.Combine(manualPath, "MuseDash_Data");
				
				string platform = "StandaloneWindows64";
				StandalonePlatform = platform;
				
				string musedash_streamingassets = manualPath + $"\\MuseDash_Data\\StreamingAssets\\aa\\{platform}\\";
				if (!Directory.Exists(musedash_streamingassets))
					return MDCompatLayerInitResult.StreamingAssetsNotFound;
				
				BuildTarget = musedash_streamingassets;
				StreamingFiles = Directory.GetFiles(musedash_streamingassets);
				
				return MDCompatLayerInitResult.OK;
			}
			
			WhereIsMuseDashInstalled = musedash_installdir;
			WhereIsMuseDashDataFolder = Path.Combine(musedash_installdir, "MuseDash_Data");

			// If installed, load noteinfo.json for BMS references
			// The bundle is named globalconfigs_assets_notedatamananger

			string platform_steam = "StandaloneWindows64";
			StandalonePlatform = platform_steam;

			string musedash_streamingassets_steam = musedash_installdir + $"\\MuseDash_Data\\StreamingAssets\\aa\\{platform_steam}\\"; // TODO: support multiple platforms
			if (!Directory.Exists(musedash_streamingassets_steam))
				return MDCompatLayerInitResult.StreamingAssetsNotFound;

			BuildTarget = musedash_streamingassets_steam;
			StreamingFiles = Directory.GetFiles(musedash_streamingassets_steam);

			// The note data file would be loaded here from the assetbundle, then the notedata extracted

			return MDCompatLayerInitResult.OK;
		}
	}
}
