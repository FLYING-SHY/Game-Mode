using System;

namespace Bf6Esp;

/// <summary>
/// 一次游戏会话的身份：进程 + Steam 账号。
/// 授权租约会绑定到这个身份上，进程重启后旧租约立即失效。
/// </summary>
internal sealed class GameIdentity60
{
	public int Pid;

	public DateTime Started;

	/// <summary>17 位 SteamID64；为空表示尚未读到账号。</summary>
	public string SteamId;

	/// <summary>显示用昵称（Steam persona name）。</summary>
	public string Name;

	/// <summary>错误说明（非空表示身份暂不可用）。</summary>
	public string Error;

	/// <summary>身份指纹：进程 + 启动时间 + 账号。</summary>
	public string Key
	{
		get { return Pid + ":" + Started.Ticks + ":" + SteamId; }
	}
}
