namespace Licensing;

/// <summary>
/// 授权系统支持同时服务多个产品（游戏修改器）。每个产品拥有独立的产品标识、
/// 独立的授权码哈希空间与独立的 Keygen 策略。
/// </summary>
public static class ProductIds
{
    /// <summary>
    /// 历史兼容的默认产品标识（CoH3 战争先锋）。
    /// 新增修改器时必须在客户端 <see cref="LicenseClientOptions.ProductId"/> 与服务端产品清单中显式声明自己的产品 id。
    /// </summary>
    public const string Default = "CoH3FowTool";
}
