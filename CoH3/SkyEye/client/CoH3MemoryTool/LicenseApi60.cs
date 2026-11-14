using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CoH3MemoryTool;

internal sealed class LicenseApi60 : ILicenseApi60, IDisposable
{
	public const string ClientVersion = "0.60.8";

	private readonly HttpClient client;

	public LicenseApi60() : this(LicenseConfiguration60.ServerBaseAddress)
	{
	}

	internal LicenseApi60(Uri baseAddress)
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
		client = new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = false,
			UseCookies = false
		})
		{
			BaseAddress = LicenseConfiguration60.ParseServerBaseAddress(baseAddress.AbsoluteUri),
			Timeout = TimeSpan.FromSeconds(5.0),
			MaxResponseContentBufferSize = 32768L
		};
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Skyeye/0.60.8");
	}

	public async Task<LicenseReply60> Call(string path, Dictionary<string, object> payload, string token, CancellationToken cancel)
	{
		if (!new string[4] { "api/v1/version", "api/v1/activate", "api/v1/heartbeat", "api/v1/logout" }.Contains60(path))
		{
			throw new ArgumentException("Invalid API route");
		}
		LeaseSignature60 signatureContext = null;
		if (path == "api/v1/activate" || path == "api/v1/heartbeat")
		{
			if (payload == null) throw new ArgumentException("Missing lease request");
			payload = new Dictionary<string, object>(payload);
			payload["nonce"] = LeaseSignature60.NewNonce();
			signatureContext = new LeaseSignature60(payload, token, path == "api/v1/activate");
		}
		long started = LicenseClock60.Milliseconds;
		using HttpRequestMessage request = new HttpRequestMessage((payload == null) ? HttpMethod.Get : HttpMethod.Post, path);
		if (payload != null)
		{
			request.Content = new StringContent(new JavaScriptSerializer().Serialize(payload), Encoding.UTF8, "application/json");
		}
		if (!string.IsNullOrEmpty(token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
		using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancel).ConfigureAwait(continueOnCapturedContext: false);
		if (response.StatusCode >= HttpStatusCode.InternalServerError || response.StatusCode == (HttpStatusCode)429)
		{
			throw new HttpRequestException("服务暂时不可用");
		}
		string text = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (!(new JavaScriptSerializer
		{
			MaxJsonLength = 32768
		}.DeserializeObject(text) is Dictionary<string, object> body))
		{
			throw new HttpRequestException("授权响应格式不正确");
		}
		LicenseReply60 reply = new LicenseReply60(body, started);
		if (reply.State == "active") reply.VerifyActive(signatureContext);
		if (!response.IsSuccessStatusCode && string.IsNullOrEmpty(reply.State))
		{
			throw new HttpRequestException("授权服务连接失败");
		}
		return reply;
	}

	public void Dispose()
	{
		client.Dispose();
	}
}
