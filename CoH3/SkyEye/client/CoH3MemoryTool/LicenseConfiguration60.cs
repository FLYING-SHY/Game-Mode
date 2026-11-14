using System;
using System.Configuration;

namespace CoH3MemoryTool;

internal static class LicenseConfiguration60
{
	public static string ProductId => string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["LicensingProductId"]) ? "CoH3FowTool" : ConfigurationManager.AppSettings["LicensingProductId"].Trim();

	public static Uri ServerBaseAddress => ParseServerBaseAddress(ConfigurationManager.AppSettings["LicensingServerBaseUrl"] ?? "http://127.0.0.1:47835/");

	internal static Uri ParseServerBaseAddress(string value)
	{
		if (!Uri.TryCreate(value, UriKind.Absolute, out var address) || string.IsNullOrEmpty(address.Host) || !string.IsNullOrEmpty(address.UserInfo) || !string.IsNullOrEmpty(address.Query) || !string.IsNullOrEmpty(address.Fragment) || (address.Scheme != Uri.UriSchemeHttps && (address.Scheme != Uri.UriSchemeHttp || address.Host != "127.0.0.1")))
		{
			throw new ArgumentException("授权服务器地址必须使用 HTTPS；127.0.0.1 可使用 HTTP。");
		}
		return new Uri(address.AbsoluteUri.TrimEnd('/') + "/");
	}
}
