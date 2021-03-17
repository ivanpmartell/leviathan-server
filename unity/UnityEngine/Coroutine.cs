using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class Coroutine : YieldInstruction
	{
		private IntPtr ptr;

		private Coroutine()
		{
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void ReleaseCoroutine();

		~Coroutine()
		{
			ReleaseCoroutine();
		}
	}
}
