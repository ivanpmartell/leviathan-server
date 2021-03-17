using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class TextAsset : Object
	{
		public string text
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public byte[] bytes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public override string ToString()
		{
			return text;
		}
	}
}
