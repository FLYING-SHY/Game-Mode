using System;

namespace Bf6Esp;

/// <summary>把授权状态翻译成中文提示文案。</summary>
internal static class LicenseStateText60
{
	/// <summary>短状态（用于状态栏）。</summary>
	public static string Short(LicenseView60 view)
	{
		if (view == null)
		{
			return "未授权";
		}
		switch (view.State)
		{
			case "active":
				return "已授权";
			case "expired":
				return "已过期";
			case "blocked":
				return "设备不匹配";
			case "in_use":
				return "使用中";
			case "login_required":
			case "revoked":
			case "identity_changed":
			case "request_conflict":
				return "未授权";
			case "update_required":
				return "需要更新";
			case "update_available":
				return "可更新";
			case "network_wait":
				return "网络异常";
			case "waiting_game":
				return "等待游戏";
			default:
				return "未授权";
		}
	}

	/// <summary>完整说明（含昵称、到期时间与提示信息）。</summary>
	public static string Describe(LicenseView60 view)
	{
		if (view == null)
		{
			return "未授权：请输入卡密完成激活。";
		}
		string head = Short(view);
		string tail = (view.Message ?? "").Trim();
		string nickname = (view.Nickname ?? "").Trim();
		if (nickname.Length > 0)
		{
			head = head + "（" + nickname + "）";
		}
		if (view.Expires > 0)
		{
			try
			{
				DateTime expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(view.Expires).ToLocalTime();
				head = head + " · 到期 " + expires.ToString("yyyy-MM-dd HH:mm");
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
		return tail.Length > 0 ? (head + "：" + tail) : head;
	}
}
