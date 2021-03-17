using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class AssetBundleCreateRequest : AsyncOperation
	{
		public AssetBundle assetBundle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}
	}
}
