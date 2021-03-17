using System;
using System.Runtime.CompilerServices;
using UnityEngineInternal;

namespace UnityEngine
{
	public sealed class Resources
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Object[] FindObjectsOfTypeAll(Type type);

		public static Object Load(string path)
		{
			return Load(path, typeof(Object));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		[TypeInferenceRule(TypeInferenceRules.TypeReferencedBySecondArgument)]
		public static extern Object Load(string path, Type type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Object[] LoadAll(string path, Type type);

		public static Object[] LoadAll(string path)
		{
			return LoadAll(path, typeof(Object));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Object GetBuiltinResource(Type type, string path);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Object LoadAssetAtPath(string assetPath, Type type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void UnloadAsset(Object assetToUnload);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern AsyncOperation UnloadUnusedAssets();
	}
}
