using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal static class LicenseStore60
{
	private static readonly byte[] entropy = Encoding.UTF8.GetBytes("Skyeye.SelfHosted.Client.Credentials.v1");

	internal static string DirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Skyeye", "SelfHostedLicensing");

	public static LicenseCredentials60 Load() => Load(DirectoryPath);

	internal static LicenseCredentials60 Load(string directoryPath)
	{
		try
		{
			byte[] bytes = ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(directoryPath, "credentials.dat")), entropy, DataProtectionScope.CurrentUser);
			LicenseCredentials60 licenseCredentials = new JavaScriptSerializer().Deserialize<LicenseCredentials60>(Encoding.UTF8.GetString(bytes));
			if (licenseCredentials != null && Guid.TryParse(licenseCredentials.Installation, out var result) && result != Guid.Empty)
			{
				return licenseCredentials;
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (CryptographicException)
		{
		}
		catch (ArgumentException)
		{
		}
		catch (InvalidOperationException)
		{
		}
		LicenseCredentials60 licenseCredentials2 = new LicenseCredentials60();
		licenseCredentials2.Installation = Guid.NewGuid().ToString();
		licenseCredentials2.Code = "";
		return licenseCredentials2;
	}

	public static void Save(LicenseCredentials60 data) => Save(data, DirectoryPath);

	internal static void Save(LicenseCredentials60 data, string directoryPath)
	{
		Directory.CreateDirectory(directoryPath);
		string text = Path.Combine(directoryPath, "credentials.dat");
		string text2 = text + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(data));
			File.WriteAllBytes(text2, ProtectedData.Protect(bytes, entropy, DataProtectionScope.CurrentUser));
			if (File.Exists(text))
			{
				File.Replace(text2, text, null);
			}
			else
			{
				File.Move(text2, text);
			}
		}
		finally
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
		}
	}
}
