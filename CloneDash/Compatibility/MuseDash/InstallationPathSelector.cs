using System.Windows.Forms;

namespace CloneDash.Compatibility.MuseDash
{
	/// <summary>
	/// Utility for finding Muse Dash installation path.
	/// First attempts Steam detection, then falls back to manual folder selection.
	/// </summary>
	public static class InstallationPathSelector
	{
		/// <summary>
		/// Opens a folder browser dialog for users to manually select their Muse Dash installation.
		/// </summary>
		/// <returns>The selected Muse Dash installation path, or null if cancelled.</returns>
		public static string? SelectMuseDashInstallation()
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.Description = "Select your Muse Dash installation folder (where Muse Dash.exe is located)";
				dialog.ShowNewFolderButton = false;

				DialogResult result = dialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					string selectedPath = dialog.SelectedPath;

					// Verify that Muse Dash.exe exists in the selected folder
					if (IsValidMuseDashInstallation(selectedPath))
					{
						return selectedPath;
					}
					else
					{
						MessageBox.Show(
							"The selected folder does not contain Muse Dash.exe. Please select the correct installation folder.",
							"Invalid Installation",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
						return null;
					}
				}

				return null; // User cancelled
			}
		}

		/// <summary>
		/// Validates that the given path contains a Muse Dash installation by checking for Muse Dash.exe.
		/// </summary>
		private static bool IsValidMuseDashInstallation(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			string exePath = Path.Combine(path, "Muse Dash.exe");
			return File.Exists(exePath);
		}

		/// <summary>
		/// Gets the Muse Dash installation path from user selection or cached settings.
		/// </summary>
		public static string? GetMuseDashPath(bool forceSelection = false)
		{
			// Try to load from cached settings first
			if (!forceSelection)
			{
				string? cachedPath = LoadCachedPath();
				if (!string.IsNullOrEmpty(cachedPath) && IsValidMuseDashInstallation(cachedPath))
				{
					return cachedPath;
				}
			}

			// If no valid cached path, let user select
			string? selectedPath = SelectMuseDashInstallation();
			if (!string.IsNullOrEmpty(selectedPath))
			{
				SaveCachedPath(selectedPath);
			}

			return selectedPath;
		}

		/// <summary>
		/// Loads the cached Muse Dash installation path from settings.
		/// </summary>
		private static string? LoadCachedPath()
		{
			try
			{
				// Store in user's AppData folder
				string configPath = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"CloneDash",
					"musedash_path.txt");

				if (File.Exists(configPath))
				{
					return File.ReadAllText(configPath).Trim();
				}
			}
			catch { }

			return null;
		}

		/// <summary>
		/// Saves the Muse Dash installation path to settings.
		/// </summary>
		private static void SaveCachedPath(string path)
		{
			try
			{
				string configDir = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"CloneDash");

				Directory.CreateDirectory(configDir);
				File.WriteAllText(Path.Combine(configDir, "musedash_path.txt"), path);
			}
			catch { }
		}
	}
}
