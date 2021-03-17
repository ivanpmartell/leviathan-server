using System.Collections;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class MonoBehaviour : Behaviour
	{
		public bool useGUILayout
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern MonoBehaviour();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_CancelInvokeAll();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern bool Internal_IsInvokingAll();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Invoke(string methodName, float time);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void InvokeRepeating(string methodName, float time, float repeatRate);

		public void CancelInvoke()
		{
			Internal_CancelInvokeAll();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void CancelInvoke(string methodName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool IsInvoking(string methodName);

		public bool IsInvoking()
		{
			return Internal_IsInvokingAll();
		}

		public Coroutine StartCoroutine(IEnumerator routine)
		{
			return StartCoroutine_Auto(routine);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Coroutine StartCoroutine_Auto(IEnumerator routine);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Coroutine StartCoroutine(string methodName, object value);

		public Coroutine StartCoroutine(string methodName)
		{
			object value = null;
			return StartCoroutine(methodName, value);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void StopCoroutine(string methodName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void StopAllCoroutines();

		public static void print(object message)
		{
			Debug.Log(message);
		}
	}
}
