namespace Bf6Esp;

/// <summary>游戏身份来源。</summary>
internal interface IGameIdentityProvider60
{
	/// <summary>读取当前身份；失败时返回带 Error 的结果。</summary>
	GameIdentity60 Read();
}
