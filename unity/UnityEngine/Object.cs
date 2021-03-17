using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngineInternal;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public class Object
	{
		private ReferenceData m_UnityRuntimeReferenceData;

		public string name
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public HideFlags hideFlags
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public override bool Equals(object o)
		{
			return CompareBaseObjects(this, o as Object);
		}

		public override int GetHashCode()
		{
			return GetInstanceID();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern bool CompareBaseObjects(Object lhs, Object rhs);

		[NotRenamed]
		public int GetInstanceID()
		{
			return m_UnityRuntimeReferenceData.instanceID;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Object Internal_CloneSingle(Object data);

		private static Object Internal_InstantiateSingle(Object data, Vector3 pos, Quaternion rot)
		{
			return INTERNAL_CALL_Internal_InstantiateSingle(data, ref pos, ref rot);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Object INTERNAL_CALL_Internal_InstantiateSingle(Object data, ref Vector3 pos, ref Quaternion rot);

		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
		{
			CheckNullArgument(original, "The prefab you want to instantiate is null.");
			return Internal_InstantiateSingle(original, position, rotation);
		}

		[TypeInferenceRule(TypeInferenceRules.TypeOfFirstArgument)]
		public static Object Instantiate(Object original)
		{
			CheckNullArgument(original, "The thing you want to instantiate is null.");
			return Internal_CloneSingle(original);
		}

		private static void CheckNullArgument(object arg, string message)
		{
			if (arg == null)
			{
				throw new ArgumentException(message);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void Destroy(Object obj, float t);

		public static void Destroy(Object obj)
		{
			float t = 0f;
			Destroy(obj, t);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void DestroyImmediate(Object obj, bool allowDestroyingAssets);

		public static void DestroyImmediate(Object obj)
		{
			bool allowDestroyingAssets = false;
			DestroyImmediate(obj, allowDestroyingAssets);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		[TypeInferenceRule(TypeInferenceRules.ArrayOfTypeReferencedByFirstArgument)]
		public static extern Object[] FindObjectsOfType(Type type);

		[TypeInferenceRule(TypeInferenceRules.TypeReferencedByFirstArgument)]
		public static Object FindObjectOfType(Type type)
		{
			Object[] array = FindObjectsOfType(type);
			if (array.Length > 0)
			{
				return array[0];
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void DontDestroyOnLoad(Object target);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void DestroyObject(Object obj, float t);

		public static void DestroyObject(Object obj)
		{
			float t = 0f;
			DestroyObject(obj, t);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Object[] FindSceneObjectsOfType(Type type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Object[] FindObjectsOfTypeIncludingAssets(Type type);

		[Obsolete("Please use Resources.FindObjectsOfTypeAll instead")]
		public static Object[] FindObjectsOfTypeAll(Type type)
		{
			return Resources.FindObjectsOfTypeAll(type);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public override extern string ToString();

		public static implicit operator bool(Object exists)
		{
			return !CompareBaseObjects(exists, null);
		}

		public static bool operator ==(Object x, Object y)
		{
			return CompareBaseObjects(x, y);
		}

		public static bool operator !=(Object x, Object y)
		{
			return !CompareBaseObjects(x, y);
		}
	}
}
