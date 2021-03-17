using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class ParticleEmitter : Component
	{
		public bool emit
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float minSize
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float maxSize
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float minEnergy
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float maxEnergy
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float minEmission
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float maxEmission
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float emitterVelocityScale
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3 worldVelocity
		{
			get
			{
				INTERNAL_get_worldVelocity(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_worldVelocity(ref value);
			}
		}

		public Vector3 localVelocity
		{
			get
			{
				INTERNAL_get_localVelocity(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_localVelocity(ref value);
			}
		}

		public Vector3 rndVelocity
		{
			get
			{
				INTERNAL_get_rndVelocity(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_rndVelocity(ref value);
			}
		}

		public bool useWorldSpace
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool rndRotation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float angularVelocity
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float rndAngularVelocity
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Particle[] particles
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int particleCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool enabled
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
		private extern void INTERNAL_get_worldVelocity(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_worldVelocity(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_localVelocity(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_localVelocity(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_rndVelocity(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_rndVelocity(ref Vector3 value);

		public void ClearParticles()
		{
			INTERNAL_CALL_ClearParticles(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ClearParticles(ParticleEmitter self);

		public void Emit()
		{
			Emit2((int)Random.Range(minEmission, maxEmission));
		}

		public void Emit(int count)
		{
			Emit2(count);
		}

		public void Emit(Vector3 pos, Vector3 velocity, float size, float energy, Color color)
		{
			InternalEmitParticleArguments args = default(InternalEmitParticleArguments);
			args.pos = pos;
			args.velocity = velocity;
			args.size = size;
			args.energy = energy;
			args.color = color;
			args.rotation = 0f;
			args.angularVelocity = 0f;
			Emit3(ref args);
		}

		public void Emit(Vector3 pos, Vector3 velocity, float size, float energy, Color color, float rotation, float angularVelocity)
		{
			InternalEmitParticleArguments args = default(InternalEmitParticleArguments);
			args.pos = pos;
			args.velocity = velocity;
			args.size = size;
			args.energy = energy;
			args.color = color;
			args.rotation = rotation;
			args.angularVelocity = angularVelocity;
			Emit3(ref args);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Emit2(int count);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Emit3(ref InternalEmitParticleArguments args);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Simulate(float deltaTime);
	}
}
