using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Ping
	{
		private IntPtr pingWrapper;

		public bool isDone
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int time
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string ip
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Ping(string address);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void DestroyPing();

		~Ping()
		{
			DestroyPing();
		}
	}
}
