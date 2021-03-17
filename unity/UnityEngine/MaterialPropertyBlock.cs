using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class MaterialPropertyBlock
	{
		private IntPtr blockPtr;

		public MaterialPropertyBlock()
		{
			InitBlock();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void InitBlock();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern void DestroyBlock();

		~MaterialPropertyBlock()
		{
			DestroyBlock();
		}

		public void AddFloat(string name, float value)
		{
			AddFloat(Shader.PropertyToID(name), value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void AddFloat(int nameID, float value);

		public void AddVector(string name, Vector4 value)
		{
			AddVector(Shader.PropertyToID(name), value);
		}

		public void AddVector(int nameID, Vector4 value)
		{
			INTERNAL_CALL_AddVector(this, nameID, ref value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_AddVector(MaterialPropertyBlock self, int nameID, ref Vector4 value);

		public void AddColor(string name, Color value)
		{
			AddColor(Shader.PropertyToID(name), value);
		}

		public void AddColor(int nameID, Color value)
		{
			INTERNAL_CALL_AddColor(this, nameID, ref value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_AddColor(MaterialPropertyBlock self, int nameID, ref Color value);

		public void AddMatrix(string name, Matrix4x4 value)
		{
			AddMatrix(Shader.PropertyToID(name), value);
		}

		public void AddMatrix(int nameID, Matrix4x4 value)
		{
			INTERNAL_CALL_AddMatrix(this, nameID, ref value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_AddMatrix(MaterialPropertyBlock self, int nameID, ref Matrix4x4 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Clear();
	}
}
