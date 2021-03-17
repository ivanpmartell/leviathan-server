using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class AnimationClip : Object
	{
		public float length
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float frameRate
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public WrapMode wrapMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Bounds localBounds
		{
			get
			{
				INTERNAL_get_localBounds(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_localBounds(ref value);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern AnimationClip();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetCurve(string relativePath, Type type, string propertyName, AnimationCurve curve);

		public void EnsureQuaternionContinuity()
		{
			INTERNAL_CALL_EnsureQuaternionContinuity(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_EnsureQuaternionContinuity(AnimationClip self);

		public void ClearCurves()
		{
			INTERNAL_CALL_ClearCurves(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ClearCurves(AnimationClip self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void AddEvent(AnimationEvent evt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_localBounds(out Bounds value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_localBounds(ref Bounds value);
	}
}
