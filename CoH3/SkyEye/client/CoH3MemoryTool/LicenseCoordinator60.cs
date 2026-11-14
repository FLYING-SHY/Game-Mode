using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CoH3MemoryTool;

internal sealed class LicenseCoordinator60 : IDisposable
{
	private readonly object gate = new object();

	private readonly ILicenseApi60 api;

	private readonly IGameIdentityProvider60 identityProvider;

	private readonly CancellationTokenSource cancel = new CancellationTokenSource();

	private readonly LicenseCredentials60 credentials;

	private Task running;

	private string code;

	private string sessionToken;

	private string identityKey;

	private string requestId;

	private string lastUpdateKey;

	private Dictionary<string, object> activation;

	private bool wanted;

	private bool blocked;

	private bool submitted;

	private readonly Action<LicenseCredentials60> saveCredentials;

	private long nextRequest;

	private long nextIdentity;

	private long expiryTick;

	private GameIdentity60 identity;

	private int generation;

	public LicenseCredentials60 Credentials => credentials;

	public event Action<LicenseView60> Changed;

	public bool IsCurrent(LicenseView60 view)
	{
		lock (gate)
		{
			return !cancel.IsCancellationRequested && view.Generation == generation;
		}
	}

	public LicenseCoordinator60(ILicenseApi60 connection = null, IGameIdentityProvider60 provider = null, LicenseCredentials60 storedCredentials = null, Action<LicenseCredentials60> save = null)
	{
		api = connection ?? new LicenseApi60();
		identityProvider = provider ?? new GameIdentityProvider60();
		credentials = storedCredentials ?? LicenseStore60.Load();
		saveCredentials = save ?? LicenseStore60.Save;
		code = credentials.Code ?? "";
		wanted = credentials.AutoLogin && !string.IsNullOrEmpty(code);
	}

	public void Start()
	{
		if (running == null)
		{
			running = Task.Run((Func<Task>)Loop);
		}
	}

	public void Submit(string value, bool remember)
	{
		lock (gate)
		{
			if (cancel.IsCancellationRequested)
			{
				return;
			}
			code = (value ?? "").Trim().ToUpperInvariant().Replace(" ", "");
			credentials.Code = (remember ? code : "");
			credentials.Remember = remember;
			credentials.AutoLogin = true;
			wanted = true;
			submitted = true;
			generation++;
			LicenseGate60.Revoke();
		}
		TrySave();
	}

	public void PauseForLogin()
	{
		lock (gate)
		{
			wanted = false;
			submitted = true;
			generation++;
			credentials.AutoLogin = false;
			LicenseGate60.Revoke();
		}
		TrySave();
	}

	public void RevalidateAfterResume()
	{
		lock (gate)
		{
			if (!cancel.IsCancellationRequested)
			{
				generation++;
				nextRequest = 0L;
				nextIdentity = 0L;
				LicenseGate60.Revoke();
			}
		}
	}

	private void TrySave()
	{
		lock (gate)
		{
			try
			{
				saveCredentials(credentials);
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
		}
	}

	private void Notify(string state, string message = "", string download = "", long expires = 0L, string latest = "", string notes = "", string kind = "")
	{
		lock (gate)
		{
			if (!cancel.IsCancellationRequested)
			{
				this.Changed?.Invoke(new LicenseView60
				{
					State = state,
					Message = message,
					Nickname = ((identity == null) ? "" : identity.Name),
					Silent = (state == "blocked"),
					DownloadUrl = download,
					Expires = expires,
					LicenseKind = kind,
					Latest = latest,
					Notes = notes,
					Generation = generation
				});
			}
		}
	}

	private void NotifyUpdate(LicenseReply60 reply)
	{
		string text = reply.Text("latest");
		string text2 = reply.Text("download_url");
		string text3 = reply.Text("notes");
		if (!string.IsNullOrEmpty(text) && LicenseVersion60.IsOlder("0.60.8", text) && LicenseVersion60.DownloadAllowed(text2))
		{
			string text4 = text + "\n" + text2 + "\n" + text3;
			if (!(text4 == lastUpdateKey))
			{
				lastUpdateKey = text4;
				Notify("update_available", "", text2, 0L, text, text3);
			}
		}
	}

	private Dictionary<string, object> Heartbeat()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("steam_id64", identity.SteamId);
		dictionary.Add("installation_id", credentials.Installation);
		dictionary.Add("client_version", "0.60.8");
		dictionary.Add("device_id", LicenseDevice60.GetWindowsDeviceId());
		return dictionary;
	}

	private async Task Release()
	{
		string token = sessionToken;
		sessionToken = null;
		identityKey = null;
		activation = null;
		requestId = Guid.NewGuid().ToString();
		expiryTick = 0L;
		LicenseGate60.Revoke();
		if (string.IsNullOrEmpty(token))
		{
			return;
		}
		try
		{
			await api.Call("api/v1/logout", new Dictionary<string, object>(), token, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			if (!(ex is HttpRequestException) && !(ex is TaskCanceledException) && !(ex is ArgumentException) && !(ex is InvalidOperationException))
			{
				throw;
			}
		}
	}

	private async Task Loop()
	{
		bool versionReady = false;
		while (!cancel.IsCancellationRequested)
		{
			try
			{
				long now = LicenseClock60.Milliseconds;
				int revision;
				bool loginWanted;
				string currentCode;
				bool reset;
				lock (gate)
				{
					revision = generation;
					loginWanted = wanted;
					currentCode = code;
					reset = submitted;
					submitted = false;
				}
				if (reset)
				{
					await Release().ConfigureAwait(continueOnCapturedContext: false);
					blocked = false;
					nextRequest = 0L;
					requestId = Guid.NewGuid().ToString();
					versionReady = false;
				}
				if (expiryTick > 0 && now >= expiryTick && !blocked)
				{
					LicenseGate60.Revoke();
					lock (gate)
					{
						wanted = false;
						credentials.AutoLogin = false;
					}
					loginWanted = false;
					Notify("expired", "卡密已到期", "", 0L);
					await Release().ConfigureAwait(continueOnCapturedContext: false);
					TrySave();
				}
				if (now >= nextIdentity)
				{
					GameIdentity60 fresh = identityProvider.Read();
					nextIdentity = LicenseClock60.Milliseconds + 1000;
					if (!string.IsNullOrEmpty(fresh.Error))
					{
						if (!string.IsNullOrEmpty(sessionToken) || activation != null)
						{
							await Release().ConfigureAwait(continueOnCapturedContext: false);
						}
						identity = fresh;
						LicenseGate60.Revoke();
						if (!blocked)
						{
							Notify("waiting_game", fresh.Error, "", 0L);
						}
					}
					else
					{
						if (identity != null && !string.IsNullOrEmpty(identity.SteamId) && identity.Key != fresh.Key)
						{
							await Release().ConfigureAwait(continueOnCapturedContext: false);
							requestId = Guid.NewGuid().ToString();
							nextRequest = 0L;
						}
						identity = fresh;
					}
				}
				if (versionReady || now < nextRequest)
				{
					goto IL_07c3;
				}
				LicenseReply60 policy = await api.Call("api/v1/version", null, null, cancel.Token).ConfigureAwait(continueOnCapturedContext: false);
				lock (gate)
				{
					if (revision != generation || cancel.IsCancellationRequested)
					{
						continue;
					}
					if (LicenseVersion60.IsOlder("0.60.8", policy.Text("minimum")))
					{
						LicenseGate60.Revoke();
						lock (gate)
						{
							wanted = false;
						}
						Notify("update_required", "当前版本需要更新。", policy.Text("download_url"), 0L, policy.Text("latest"), policy.Text("notes"));
						nextRequest = now + 30000;
					}
					else
					{
						versionReady = true;
						nextRequest = 0L;
					}
					goto IL_07c3;
				}
				IL_1238:
				if (!LicenseGate60.Allowed && !blocked && !string.IsNullOrEmpty(sessionToken))
				{
					Notify("network_wait", "正在重新验证授权，请稍候。", "", 0L);
				}
				goto IL_1392;
				IL_0885:
				if (string.IsNullOrEmpty(sessionToken) && activation == null)
				{
					requestId = requestId ?? Guid.NewGuid().ToString();
					activation = Heartbeat();
					activation["code"] = currentCode;
					activation["game_name"] = identity.Name ?? "";
					activation["request_id"] = requestId;
				}
				string requestIdentity = identity.Key;
				LicenseReply60 reply = await api.Call(string.IsNullOrEmpty(sessionToken) ? "api/v1/activate" : "api/v1/heartbeat", string.IsNullOrEmpty(sessionToken) ? activation : Heartbeat(), sessionToken, cancel.Token).ConfigureAwait(continueOnCapturedContext: false);
				bool stale;
				lock (gate)
				{
					stale = revision != generation || cancel.IsCancellationRequested;
				}
				if (stale)
				{
					string late = reply.Text("session_token");
					if (!string.IsNullOrEmpty(late))
					{
						try
						{
							await api.Call("api/v1/logout", new Dictionary<string, object>(), late, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
						}
						catch (HttpRequestException)
						{
						}
						catch (OperationCanceledException)
						{
						}
					}
					continue;
				}
				GameIdentity60 check = identityProvider.Read();
				if (!string.IsNullOrEmpty(check.Error) || check.Key != requestIdentity)
				{
					string lateToken = reply.Text("session_token");
					if (!string.IsNullOrEmpty(lateToken))
					{
						sessionToken = lateToken;
					}
					await Release().ConfigureAwait(continueOnCapturedContext: false);
					identity = check;
					nextIdentity = 0L;
					nextRequest = 0L;
					continue;
				}
				bool releaseReply = false;
				lock (gate)
				{
					if (revision != generation || cancel.IsCancellationRequested)
					{
						string value = reply.Text("session_token");
						if (!string.IsNullOrEmpty(value))
						{
							sessionToken = value;
						}
						continue;
					}
					if (!string.IsNullOrEmpty(reply.Text("session_token")))
					{
						sessionToken = reply.Text("session_token");
					}
					switch (reply.State)
					{
					case "active":
						if (!reply.SignatureVerified || string.IsNullOrEmpty(sessionToken) || sessionToken.Length != 64 || !LicenseGate60.Grant(reply.Number("server_time"), reply.Number("lease_until"), reply.Number("expires_at"), reply.RequestStarted, identity))
						{
							throw new HttpRequestException("授权时间信息无效");
						}
						identityKey = identity.Key;
						blocked = false;
						expiryTick = reply.RequestStarted + Math.Max(0L, reply.Number("expires_at") - reply.Number("server_time")) * 1000 - 1000;
						nextRequest = LicenseClock60.Milliseconds + 28000;
						Notify("active", "", "", reply.Number("expires_at"), kind: reply.Text("license_kind"));
						TrySave();
						NotifyUpdate(reply);
						break;
					case "blocked":
						LicenseGate60.Revoke();
						blocked = true;
						nextRequest = LicenseClock60.Milliseconds + 30000;
						Notify("blocked", "", "", 0L);
						break;
					case "in_use":
						LicenseGate60.Revoke();
						nextRequest = LicenseClock60.Milliseconds + 15000;
						Notify("in_use", "该激活码正在使用中，请等待另一台退出。", "", 0L);
						break;
					case "expired":
						LicenseGate60.Revoke();
						blocked = false;
						wanted = false;
						credentials.AutoLogin = false;
						Notify("expired", "卡密已到期", "", 0L);
						releaseReply = true;
						break;
					case "update_required":
						LicenseGate60.Revoke();
						lock (gate)
						{
							wanted = false;
						}
						Notify("update_required", "当前版本需要更新。", reply.Text("download_url"), 0L, reply.Text("latest"), reply.Text("notes"));
						nextRequest = LicenseClock60.Milliseconds + 30000;
						break;
					default:
						LicenseGate60.Revoke();
						wanted = false;
						credentials.AutoLogin = false;
						nextRequest = LicenseClock60.Milliseconds + 30000;
						Notify("login_required", (reply.State == "identity_changed") ? "游戏账号已变化，请重新激活。" : ((reply.State == "revoked") ? "本次登录已解除，请重新确认激活码。" : ((reply.State == "request_conflict") ? "请重新点击激活后再试。" : "激活码无效或登录已失效，请重新确认。")), "", 0L);
						releaseReply = true;
						break;
					}
				}
				if (releaseReply)
				{
					await Release().ConfigureAwait(continueOnCapturedContext: false);
					TrySave();
				}
				goto IL_1238;
				IL_07c3:
				if (!versionReady || !loginWanted || identity == null || !string.IsNullOrEmpty(identity.Error) || now < nextRequest)
				{
					goto IL_1238;
				}
				lock (gate)
				{
					if (revision != generation || cancel.IsCancellationRequested)
					{
						continue;
					}
					goto IL_0885;
				}
			}
			catch (OperationCanceledException)
			{
				if (cancel.IsCancellationRequested)
				{
					break;
				}
				nextRequest = LicenseClock60.Milliseconds + 5000;
				if (!blocked)
				{
					Notify("network_wait", "连接暂时中断，正在重试。", "", 0L);
				}
				goto IL_1392;
			}
			catch (Exception ex4)
			{
				if (!(ex4 is HttpRequestException) && !(ex4 is IOException) && !(ex4 is ArgumentException) && !(ex4 is InvalidOperationException) && !(ex4 is OverflowException))
				{
					LicenseGate60.Revoke();
					Notify("network_wait", "授权检查暂时失败，正在重试。", "", 0L);
				}
				else if (!blocked)
				{
					Notify("network_wait", "暂时无法连接授权服务器，正在重试。", "", 0L);
				}
				nextRequest = LicenseClock60.Milliseconds + 5000;
				goto IL_1392;
			}
			IL_1392:
			try
			{
				await Task.Delay(250, cancel.Token).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				break;
			}
		}
	}

	public async Task CloseAsync()
	{
		lock (gate)
		{
			cancel.Cancel();
			generation++;
			LicenseGate60.Revoke();
		}
		if (running != null)
		{
			try
			{
				await running.ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
			}
		}
		try
		{
			await Release().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			api.Dispose();
		}
	}

	public void Dispose()
	{
		lock (gate)
		{
			cancel.Cancel();
			generation++;
			LicenseGate60.Revoke();
		}
	}
}
