using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Tree : Component
	{
		public ScriptableObject data
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
