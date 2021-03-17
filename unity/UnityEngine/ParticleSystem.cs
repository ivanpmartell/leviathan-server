using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class ParticleSystem : Component
	{
		public struct Particle
		{
			private Vector3 m_Position;

			private Vector3 m_Velocity;

			private Vector3 m_AnimatedVelocity;

			private Vector3 m_AxisOfRotation;

			private float m_Rotation;

			private float m_AngularVelocity;

			private float m_Size;

			private Color32 m_Color;

			private float m_RandomValue;

			private float m_Lifetime;

			private float m_StartLifetime;

			private float m_EmitAccumulator;

			public Vector3 position
			{
				get
				{
					return m_Position;
				}
				set
				{
					m_Position = value;
				}
			}

			public Vector3 velocity
			{
				get
				{
					return m_Velocity;
				}
				set
				{
					m_Velocity = value;
				}
			}

			public float lifetime
			{
				get
				{
					return m_Lifetime;
				}
				set
				{
					m_Lifetime = value;
				}
			}

			public float startLifetime
			{
				get
				{
					return m_StartLifetime;
				}
				set
				{
					m_StartLifetime = value;
				}
			}

			public float size
			{
				get
				{
					return m_Size;
				}
				set
				{
					m_Size = value;
				}
			}

			public float rotation
			{
				get
				{
					return m_Rotation * 57.29578f;
				}
				set
				{
					m_Rotation = value * ((float)Math.PI / 180f);
				}
			}

			public float angularVelocity
			{
				get
				{
					return m_AngularVelocity * 57.29578f;
				}
				set
				{
					m_AngularVelocity = value * ((float)Math.PI / 180f);
				}
			}

			public Color32 color
			{
				get
				{
					return m_Color;
				}
				set
				{
					m_Color = value;
				}
			}

			public float randomValue
			{
				get
				{
					return m_RandomValue;
				}
				set
				{
					m_RandomValue = value;
				}
			}
		}

		public float startDelay
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool isPlaying
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool loop
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool playOnAwake
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float time
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float duration
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float playbackSpeed
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

		public bool enableEmission
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float emissionRate
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float startSpeed
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float startSize
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Color startColor
		{
			get
			{
				INTERNAL_get_startColor(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_startColor(ref value);
			}
		}

		public float startRotation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float startLifetime
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float gravityModifier
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
		private extern void INTERNAL_get_startColor(out Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_startColor(ref Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetParticles(Particle[] particles, int size);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int GetParticles(Particle[] particles);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_Simulate(float t);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_Play();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_Stop();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_Pause();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_Clear();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern bool Internal_IsAlive();

		public void Simulate(float t)
		{
			bool withChildren = true;
			Simulate(t, withChildren);
		}

		public void Simulate(float t, bool withChildren)
		{
			if (withChildren)
			{
				ParticleSystem[] particleSystems = GetParticleSystems(this);
				ParticleSystem[] array = particleSystems;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Internal_Simulate(t);
				}
			}
			else
			{
				Internal_Simulate(t);
			}
		}

		public void Play()
		{
			bool withChildren = true;
			Play(withChildren);
		}

		public void Play(bool withChildren)
		{
			if (withChildren)
			{
				ParticleSystem[] particleSystems = GetParticleSystems(this);
				ParticleSystem[] array = particleSystems;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Internal_Play();
				}
			}
			else
			{
				Internal_Play();
			}
		}

		public void Stop()
		{
			bool withChildren = true;
			Stop(withChildren);
		}

		public void Stop(bool withChildren)
		{
			if (withChildren)
			{
				ParticleSystem[] particleSystems = GetParticleSystems(this);
				ParticleSystem[] array = particleSystems;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Internal_Stop();
				}
			}
			else
			{
				Internal_Stop();
			}
		}

		public void Pause()
		{
			bool withChildren = true;
			Pause(withChildren);
		}

		public void Pause(bool withChildren)
		{
			if (withChildren)
			{
				ParticleSystem[] particleSystems = GetParticleSystems(this);
				ParticleSystem[] array = particleSystems;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Internal_Pause();
				}
			}
			else
			{
				Internal_Pause();
			}
		}

		public void Clear()
		{
			bool withChildren = true;
			Clear(withChildren);
		}

		public void Clear(bool withChildren)
		{
			if (withChildren)
			{
				ParticleSystem[] particleSystems = GetParticleSystems(this);
				ParticleSystem[] array = particleSystems;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Internal_Clear();
				}
			}
			else
			{
				Internal_Clear();
			}
		}

		public bool IsAlive()
		{
			bool withChildren = true;
			return IsAlive(withChildren);
		}

		public bool IsAlive(bool withChildren)
		{
			if (withChildren)
			{
				ParticleSystem[] particleSystems = GetParticleSystems(this);
				ParticleSystem[] array = particleSystems;
				foreach (ParticleSystem particleSystem in array)
				{
					if (particleSystem.Internal_IsAlive())
					{
						return true;
					}
				}
				return false;
			}
			return Internal_IsAlive();
		}

		public void Emit(int count)
		{
			INTERNAL_CALL_Emit(this, count);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Emit(ParticleSystem self, int count);

		internal static ParticleSystem[] GetParticleSystems(ParticleSystem root)
		{
			if (!root)
			{
				return null;
			}
			List<ParticleSystem> list = new List<ParticleSystem>();
			list.Add(root);
			GetDirectParticleSystemChildrenRecursive(root.transform, list);
			return list.ToArray();
		}

		private static void GetDirectParticleSystemChildrenRecursive(Transform transform, List<ParticleSystem> particleSystems)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				ParticleSystem component = child.gameObject.GetComponent<ParticleSystem>();
				if (component != null)
				{
					particleSystems.Add(component);
					GetDirectParticleSystemChildrenRecursive(child, particleSystems);
				}
			}
		}
	}
}
