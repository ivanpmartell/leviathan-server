using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Shader : Object
	{
		public bool isSupported
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int maximumLOD
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int globalMaximumLOD
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int renderQueue
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Shader Find(string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern Shader FindBuiltin(string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void EnableKeyword(string keyword);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void DisableKeyword(string keyword);

		public static void SetGlobalColor(string propertyName, Color color)
		{
			INTERNAL_CALL_SetGlobalColor(propertyName, ref color);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetGlobalColor(string propertyName, ref Color color);

		public static void SetGlobalVector(string propertyName, Vector4 vec)
		{
			SetGlobalColor(propertyName, vec);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void SetGlobalFloat(string propertyName, float value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void SetGlobalTexture(string propertyName, Texture tex);

		public static void SetGlobalMatrix(string propertyName, Matrix4x4 mat)
		{
			INTERNAL_CALL_SetGlobalMatrix(propertyName, ref mat);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetGlobalMatrix(string propertyName, ref Matrix4x4 mat);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void SetGlobalTexGenMode(string propertyName, TexGenMode mode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void SetGlobalTextureMatrixName(string propertyName, string matrixName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern int PropertyToID(string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void WarmupAllShaders();
	}
}
