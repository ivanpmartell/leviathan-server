using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Animation : Behaviour, IEnumerable
	{
		private sealed class Enumerator : IEnumerator
		{
			private Animation m_Outer;

			private int m_CurrentIndex = -1;

			public object Current => m_Outer.GetStateAtIndex(m_CurrentIndex);

			internal Enumerator(Animation outer)
			{
				m_Outer = outer;
			}

			public bool MoveNext()
			{
				int stateCount = m_Outer.GetStateCount();
				m_CurrentIndex++;
				return m_CurrentIndex < stateCount;
			}

			public void Reset()
			{
				m_CurrentIndex = -1;
			}
		}

		public AnimationClip clip
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool playAutomatically
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
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

		public bool isPlaying
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public AnimationState this[string name] => GetState(name);

		public bool animatePhysics
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("Use cullingType instead")]
		public bool animateOnlyIfVisible
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public AnimationCullingType cullingType
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

		public void Stop()
		{
			INTERNAL_CALL_Stop(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Stop(Animation self);

		public void Stop(string name)
		{
			Internal_StopByName(name);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_StopByName(string name);

		public void Rewind(string name)
		{
			Internal_RewindByName(name);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_RewindByName(string name);

		public void Rewind()
		{
			INTERNAL_CALL_Rewind(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Rewind(Animation self);

		public void Sample()
		{
			INTERNAL_CALL_Sample(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Sample(Animation self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool IsPlaying(string name);

		public bool Play()
		{
			PlayMode mode = PlayMode.StopSameLayer;
			return Play(mode);
		}

		public bool Play(PlayMode mode)
		{
			return PlayDefaultAnimation(mode);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool Play(string animation, PlayMode mode);

		public bool Play(string animation)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			return Play(animation, mode);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void CrossFade(string animation, float fadeLength, PlayMode mode);

		public void CrossFade(string animation, float fadeLength)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			CrossFade(animation, fadeLength, mode);
		}

		public void CrossFade(string animation)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			float fadeLength = 0.3f;
			CrossFade(animation, fadeLength, mode);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Blend(string animation, float targetWeight, float fadeLength);

		public void Blend(string animation, float targetWeight)
		{
			float fadeLength = 0.3f;
			Blend(animation, targetWeight, fadeLength);
		}

		public void Blend(string animation)
		{
			float fadeLength = 0.3f;
			float targetWeight = 1f;
			Blend(animation, targetWeight, fadeLength);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern AnimationState CrossFadeQueued(string animation, float fadeLength, QueueMode queue, PlayMode mode);

		public AnimationState CrossFadeQueued(string animation, float fadeLength, QueueMode queue)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			return CrossFadeQueued(animation, fadeLength, queue, mode);
		}

		public AnimationState CrossFadeQueued(string animation, float fadeLength)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			QueueMode queue = QueueMode.CompleteOthers;
			return CrossFadeQueued(animation, fadeLength, queue, mode);
		}

		public AnimationState CrossFadeQueued(string animation)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			QueueMode queue = QueueMode.CompleteOthers;
			float fadeLength = 0.3f;
			return CrossFadeQueued(animation, fadeLength, queue, mode);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern AnimationState PlayQueued(string animation, QueueMode queue, PlayMode mode);

		public AnimationState PlayQueued(string animation, QueueMode queue)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			return PlayQueued(animation, queue, mode);
		}

		public AnimationState PlayQueued(string animation)
		{
			PlayMode mode = PlayMode.StopSameLayer;
			QueueMode queue = QueueMode.CompleteOthers;
			return PlayQueued(animation, queue, mode);
		}

		public void AddClip(AnimationClip clip, string newName)
		{
			AddClip(clip, newName, int.MinValue, int.MaxValue);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void AddClip(AnimationClip clip, string newName, int firstFrame, int lastFrame, bool addLoopFrame);

		public void AddClip(AnimationClip clip, string newName, int firstFrame, int lastFrame)
		{
			bool addLoopFrame = false;
			AddClip(clip, newName, firstFrame, lastFrame, addLoopFrame);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void RemoveClip(AnimationClip clip);

		public void RemoveClip(string clipName)
		{
			RemoveClip2(clipName);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int GetClipCount();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void RemoveClip2(string clipName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern bool PlayDefaultAnimation(PlayMode mode);

		public bool Play(AnimationPlayMode mode)
		{
			return PlayDefaultAnimation((PlayMode)mode);
		}

		public bool Play(string animation, AnimationPlayMode mode)
		{
			return Play(animation, (PlayMode)mode);
		}

		public void SyncLayer(int layer)
		{
			INTERNAL_CALL_SyncLayer(this, layer);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SyncLayer(Animation self, int layer);

		public IEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern AnimationState GetState(string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern AnimationState GetStateAtIndex(int index);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern int GetStateCount();

		public AnimationClip GetClip(string name)
		{
			AnimationState state = GetState(name);
			if ((bool)state)
			{
				return state.clip;
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_localBounds(out Bounds value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_localBounds(ref Bounds value);
	}
}
