using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class AudioSource : Behaviour
	{
		public float volume
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float pitch
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

		public int timeSamples
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public AudioClip clip
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

		public bool ignoreListenerVolume
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

		public AudioVelocityUpdateMode velocityUpdateMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float panLevel
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool bypassEffects
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float dopplerLevel
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float spread
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int priority
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool mute
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float minDistance
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float maxDistance
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float pan
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public AudioRolloffMode rolloffMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("minVolume is not supported anymore. Use min-, maxDistance and rolloffMode instead.", true)]
		public float minVolume
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("maxVolume is not supported anymore. Use min-, maxDistance and rolloffMode instead.", true)]
		public float maxVolume
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("rolloffFactor is not supported anymore. Use min-, maxDistance and rolloffMode instead.", true)]
		public float rolloffFactor
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
		public extern void Play(ulong delay);

		public void Play()
		{
			ulong delay = 0uL;
			Play(delay);
		}

		public void Stop()
		{
			INTERNAL_CALL_Stop(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Stop(AudioSource self);

		public void Pause()
		{
			INTERNAL_CALL_Pause(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Pause(AudioSource self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void PlayOneShot(AudioClip clip, float volumeScale);

		public void PlayOneShot(AudioClip clip)
		{
			float volumeScale = 1f;
			PlayOneShot(clip, volumeScale);
		}

		public static void PlayClipAtPoint(AudioClip clip, Vector3 position)
		{
			float num = 1f;
			PlayClipAtPoint(clip, position, num);
		}

		public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume)
		{
			GameObject gameObject = new GameObject("One shot audio");
			gameObject.transform.position = position;
			AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
			audioSource.clip = clip;
			audioSource.volume = volume;
			audioSource.Play();
			Object.Destroy(gameObject, clip.length);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void GetOutputDataHelper(float[] samples, int channel);

		[Obsolete("GetOutputData return a float[] is deprecated, use GetOutputData passing a pre allocated array instead.")]
		public float[] GetOutputData(int numSamples, int channel)
		{
			float[] array = new float[numSamples];
			GetOutputDataHelper(array, channel);
			return array;
		}

		public void GetOutputData(float[] samples, int channel)
		{
			GetOutputDataHelper(samples, channel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void GetSpectrumDataHelper(float[] samples, int channel, FFTWindow window);

		[Obsolete("GetSpectrumData returning a float[] is deprecated, use GetSpectrumData passing a pre allocated array instead.")]
		public float[] GetSpectrumData(int numSamples, int channel, FFTWindow window)
		{
			float[] array = new float[numSamples];
			GetSpectrumDataHelper(array, channel, window);
			return array;
		}

		public void GetSpectrumData(float[] samples, int channel, FFTWindow window)
		{
			GetSpectrumDataHelper(samples, channel, window);
		}
	}
}
