using System;

namespace Bf6Esp;

/// <summary>
/// Battlefield 6 的游戏身份来源：
/// 进程部分来自 bf6.exe / bf6Trial.exe，账号部分优先取 Frostbite 本地日志，
/// 其次取本机 Steam 注册表 / loginusers.vdf。
/// </summary>
internal sealed class GameIdentityProvider60 : IGameIdentityProvider60
{
	public GameIdentity60 Read()
	{
		GameIdentity60 identity = new GameIdentity60();
		GameProcess process = GameProcess.Attach();
		if (process == null)
		{
			identity.Error = "未检测到 Battlefield 6 进程（bf6.exe），请先启动游戏。";
			return identity;
		}
		try
		{
			identity.Pid = process.ProcessId;
			identity.Started = process.Started;
		}
		finally
		{
			process.Dispose();
		}
		ulong? logged = LocalAccountLog59.Read(identity.Started);
		if (logged.HasValue)
		{
			identity.SteamId = logged.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
			identity.Name = "Battlefield 6";
			return identity;
		}
		SteamIdentity60 steam = SteamIdentity60.Read();
		if (!steam.Ok)
		{
			identity.Error = "尚未读到本次 Steam 账号信息，请确认 Steam 已登录。";
			return identity;
		}
		identity.SteamId = steam.SteamId64;
		identity.Name = steam.PersonaName;
		return identity;
	}
}
