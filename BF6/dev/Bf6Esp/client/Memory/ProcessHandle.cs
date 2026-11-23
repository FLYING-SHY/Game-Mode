using System;
using Microsoft.Win32.SafeHandles;

namespace Bf6Esp;

/// <summary>
/// 进程句柄的安全包装。
///
/// 权限说明（重要）：本工具只申请
///   PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION
/// 两个权限位。理由：
///   1) Battlefield 6 带 EA Javelin 内核级反作弊，写入游戏内存会触发完整性校验，
///      外部注入/写内存是最容易被检测、也最容易被封禁的路径；
///   2) 只读句柄不会在游戏进程里留下任何被修改的页，也不占用远程线程槽位；
///   3) 真正的产品诉求（方框、血条、距离）全部可以由 ReadProcessMemory + 覆盖层绘制满足，
///      不需要写权限。
/// 因此这个类永远不接受写入型访问掩码，也不在本项目内提供任何写入 API。
/// </summary>
internal sealed class ProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	internal ProcessHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		return NativeMemory.CloseHandle(handle);
	}

	/// <summary>接管 OpenProcess 返回的原生句柄。</summary>
	internal void Attach(IntPtr raw)
	{
		SetHandle(raw);
	}
}
