using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace CoH3MemoryTool;

internal static class LicenseDevice60
{
	public static string GetWindowsDeviceId()
	{
		using RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
		string machineGuid = key?.GetValue("MachineGuid") as string;
		if (string.IsNullOrWhiteSpace(machineGuid))
		{
			throw new PlatformNotSupportedException("MachineGuid missing");
		}
		return Fingerprint(LicenseConfiguration60.ProductId, machineGuid);
	}

	internal static string Fingerprint(string productId, string machineGuid)
	{
		using SHA256 sha = SHA256.Create();
		return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(productId + "|device-v1|" + machineGuid.Trim().ToLowerInvariant()))).Replace("-", "");
	}
}
