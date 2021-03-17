using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class PhysicMaterial : Object
	{
		public float dynamicFriction
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float staticFriction
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float bounciness
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("Use PhysicMaterial.bounciness instead", true)]
		public float bouncyness
		{
			get
			{
				return bounciness;
			}
			set
			{
				bounciness = value;
			}
		}

		public Vector3 frictionDirection2
		{
			get
			{
				INTERNAL_get_frictionDirection2(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_frictionDirection2(ref value);
			}
		}

		public float dynamicFriction2
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float staticFriction2
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public PhysicMaterialCombine frictionCombine
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public PhysicMaterialCombine bounceCombine
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3 frictionDirection
		{
			get
			{
				return frictionDirection2;
			}
			set
			{
				frictionDirection2 = value;
			}
		}

		public PhysicMaterial()
		{
			Internal_CreateDynamicsMaterial(null);
		}

		public PhysicMaterial(string name)
		{
			Internal_CreateDynamicsMaterial(name);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_CreateDynamicsMaterial(string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_frictionDirection2(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_frictionDirection2(ref Vector3 value);
	}
}
