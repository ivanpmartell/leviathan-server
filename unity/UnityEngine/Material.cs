using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class Material : Object
	{
		public Shader shader
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Color color
		{
			get
			{
				return GetColor("_Color");
			}
			set
			{
				SetColor("_Color", value);
			}
		}

		public Texture mainTexture
		{
			get
			{
				return GetTexture("_MainTex");
			}
			set
			{
				SetTexture("_MainTex", value);
			}
		}

		public Vector2 mainTextureOffset
		{
			get
			{
				return GetTextureOffset("_MainTex");
			}
			set
			{
				SetTextureOffset("_MainTex", value);
			}
		}

		public Vector2 mainTextureScale
		{
			get
			{
				return GetTextureScale("_MainTex");
			}
			set
			{
				SetTextureScale("_MainTex", value);
			}
		}

		public int passCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int renderQueue
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Material(string contents)
		{
			Internal_CreateWithString(this, contents);
		}

		public Material(Shader shader)
		{
			Internal_CreateWithShader(this, shader);
		}

		public Material(Material source)
		{
			Internal_CreateWithMaterial(this, source);
		}

		public void SetColor(string propertyName, Color color)
		{
			INTERNAL_CALL_SetColor(this, propertyName, ref color);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetColor(Material self, string propertyName, ref Color color);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color GetColor(string propertyName);

		public void SetVector(string propertyName, Vector4 vector)
		{
			SetColor(propertyName, new Color(vector.x, vector.y, vector.z, vector.w));
		}

		public Vector4 GetVector(string propertyName)
		{
			Color color = GetColor(propertyName);
			return new Vector4(color.r, color.g, color.b, color.a);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetTexture(string propertyName, Texture texture);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Texture GetTexture(string propertyName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_GetTextureOffset(Material mat, string name, out Vector2 output);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_GetTextureScale(Material mat, string name, out Vector2 output);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_GetTexturePivot(Material mat, string name, out Vector2 output);

		public void SetTextureOffset(string propertyName, Vector2 offset)
		{
			INTERNAL_CALL_SetTextureOffset(this, propertyName, ref offset);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetTextureOffset(Material self, string propertyName, ref Vector2 offset);

		public Vector2 GetTextureOffset(string propertyName)
		{
			Internal_GetTextureOffset(this, propertyName, out var output);
			return output;
		}

		public void SetTextureScale(string propertyName, Vector2 scale)
		{
			INTERNAL_CALL_SetTextureScale(this, propertyName, ref scale);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetTextureScale(Material self, string propertyName, ref Vector2 scale);

		public Vector2 GetTextureScale(string propertyName)
		{
			Internal_GetTextureScale(this, propertyName, out var output);
			return output;
		}

		public void SetMatrix(string propertyName, Matrix4x4 matrix)
		{
			INTERNAL_CALL_SetMatrix(this, propertyName, ref matrix);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetMatrix(Material self, string propertyName, ref Matrix4x4 matrix);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Matrix4x4 GetMatrix(string propertyName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetFloat(string propertyName, float value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern float GetFloat(string propertyName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool HasProperty(string propertyName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern string GetTag(string tag, bool searchFallbacks, string defaultValue);

		public string GetTag(string tag, bool searchFallbacks)
		{
			string empty = string.Empty;
			return GetTag(tag, searchFallbacks, empty);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Lerp(Material start, Material end, float t);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool SetPass(int pass);

		[Obsolete("Use the Material constructor instead.")]
		public static Material Create(string scriptContents)
		{
			return new Material(scriptContents);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_CreateWithString(Material mono, string contents);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_CreateWithShader(Material mono, Shader shader);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_CreateWithMaterial(Material mono, Material source);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void CopyPropertiesFromMaterial(Material mat);
	}
}
