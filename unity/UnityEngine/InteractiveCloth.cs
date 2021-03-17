using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class InteractiveCloth : Cloth
	{
		public Mesh mesh
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float friction
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float density
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float pressure
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float collisionResponse
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float tearFactor
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float attachmentTearFactor
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float attachmentResponse
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool isTeared
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public void AddForceAtPosition(Vector3 force, Vector3 position, float radius, ForceMode mode)
		{
			INTERNAL_CALL_AddForceAtPosition(this, ref force, ref position, radius, mode);
		}

		public void AddForceAtPosition(Vector3 force, Vector3 position, float radius)
		{
			ForceMode mode = ForceMode.Force;
			INTERNAL_CALL_AddForceAtPosition(this, ref force, ref position, radius, mode);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_AddForceAtPosition(InteractiveCloth self, ref Vector3 force, ref Vector3 position, float radius, ForceMode mode);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void AttachToCollider(Collider collider, bool tearable, bool twoWayInteraction);

		public void AttachToCollider(Collider collider, bool tearable)
		{
			bool twoWayInteraction = false;
			AttachToCollider(collider, tearable, twoWayInteraction);
		}

		public void AttachToCollider(Collider collider)
		{
			bool twoWayInteraction = false;
			bool tearable = false;
			AttachToCollider(collider, tearable, twoWayInteraction);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void DetachFromCollider(Collider collider);
	}
}
