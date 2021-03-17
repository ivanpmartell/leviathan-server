using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class OcclusionPortal : Component
	{
		public bool open
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}
	}
}
