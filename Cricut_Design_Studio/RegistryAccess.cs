using Microsoft.Win32;

namespace Cricut_Design_Studio
{
	public class RegistryAccess
	{
		private const string SOFTWARE_KEY = "Software";

		private const string COMPANY_NAME = "Cognitive Devices";

		private const string APPLICATION_NAME = "Cricut PC";

		public static string GetStringRegistryValue(string key, string defaultValue)
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software", writable: false).OpenSubKey("Cognitive Devices", writable: false);
			if (registryKey != null)
			{
				RegistryKey registryKey2 = registryKey.OpenSubKey("Cricut PC", writable: true);
				if (registryKey2 != null)
				{
					string[] valueNames = registryKey2.GetValueNames();
					foreach (string text in valueNames)
					{
						if (text == key)
						{
							return (string)registryKey2.GetValue(text);
						}
					}
				}
			}
			return defaultValue;
		}

		public static void SetStringRegistryValue(string key, string stringValue)
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software", writable: true);
			registryKey.CreateSubKey("Cognitive Devices")?.CreateSubKey("Cricut PC")?.SetValue(key, stringValue);
		}
	}
}
