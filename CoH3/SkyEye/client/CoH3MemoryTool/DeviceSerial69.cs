using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;

namespace CoH3MemoryTool;

internal static class DeviceSerial69
{
	private static readonly object gate = new object();

	private static bool loaded;

	private static string serial = "";

	private static string kind = "";

	internal static string Clean(string value)
	{
		value = (value ?? "").Trim();
		if (value.Length == 0 || value.Length > 64)
		{
			return "";
		}
		string text = value;
		foreach (char c in text)
		{
			if (char.IsControl(c))
			{
				return "";
			}
		}
		string text2 = value.ToLowerInvariant().Replace(" ", "").Replace("-", "")
			.Replace(".", "");
		if (text2.Length != 0)
		{
			switch (text2)
			{
			case "tobefilledbyoem":
			case "defaultstring":
			case "systemserialnumber":
			case "unknown":
			case "none":
			case "notapplicable":
			case "na":
			case "oem":
				break;
			default:
			{
				bool flag = true;
				bool flag2 = true;
				string text3 = text2;
				foreach (char c2 in text3)
				{
					flag = flag && c2 == '0';
					flag2 = flag2 && c2 == 'f';
				}
				if (flag || flag2)
				{
					return "";
				}
				return value;
			}
			}
		}
		return "";
	}

	private static void Load()
	{
		lock (gate)
		{
			if (loaded)
			{
				return;
			}
			loaded = true;
			string[] array = new string[2] { "Win32_BIOS", "Win32_BaseBoard" };
			foreach (string text in array)
			{
				try
				{
					EnumerationOptions enumerationOptions = new EnumerationOptions();
					enumerationOptions.Timeout = TimeSpan.FromSeconds(2.0);
					enumerationOptions.ReturnImmediately = false;
					EnumerationOptions options = enumerationOptions;
					using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT SerialNumber FROM " + text, options);
					using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
					foreach (ManagementBaseObject item in managementObjectCollection)
					{
						using (item)
						{
							string text2 = Clean(Convert.ToString(item["SerialNumber"], CultureInfo.InvariantCulture));
							if (text2.Length == 0)
							{
								continue;
							}
							serial = text2;
							kind = ((text == "Win32_BIOS") ? "bios" : "baseboard");
							return;
						}
					}
				}
				catch
				{
				}
			}
		}
	}

	public static void Append(Dictionary<string, object> payload)
	{
		Load();
		if (serial.Length > 0)
		{
			payload["device_serial"] = serial;
			payload["device_kind"] = kind;
		}
	}
}
