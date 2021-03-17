using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class AudioClip : Object
	{
		public delegate void PCMReaderCallback(float[] data);

		public delegate void PCMSetPositionCallback(int position);

		public float length
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int samples
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int channels
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int frequency
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool isReadyToPlay
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		private event PCMReaderCallback m_PCMReaderCallback;

		private event PCMSetPositionCallback m_PCMSetPositionCallback;

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void GetData(float[] data, int offsetSamples);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetData(float[] data, int offsetSamples);

		public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool _3D, bool stream)
		{
			return Create(name, lengthSamples, channels, frequency, _3D, stream, null, null);
		}

		public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool _3D, bool stream, PCMReaderCallback pcmreadercallback)
		{
			return Create(name, lengthSamples, channels, frequency, _3D, stream, pcmreadercallback, null);
		}

		public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool _3D, bool stream, PCMReaderCallback pcmreadercallback, PCMSetPositionCallback pcmsetpositioncallback)
		{
			AudioClip audioClip = Construct_Internal();
			if (pcmreadercallback != null)
			{
				audioClip.m_PCMReaderCallback = (PCMReaderCallback)Delegate.Combine(audioClip.m_PCMReaderCallback, pcmreadercallback);
			}
			if (pcmsetpositioncallback != null)
			{
				audioClip.m_PCMSetPositionCallback = (PCMSetPositionCallback)Delegate.Combine(audioClip.m_PCMSetPositionCallback, pcmsetpositioncallback);
			}
			audioClip.Init_Internal(name, lengthSamples, channels, frequency, _3D, stream);
			return audioClip;
		}

		private void InvokePCMReaderCallback_Internal(float[] data)
		{
			if (this.m_PCMReaderCallback != null)
			{
				this.m_PCMReaderCallback(data);
			}
		}

		private void InvokePCMSetPositionCallback_Internal(int position)
		{
			if (this.m_PCMSetPositionCallback != null)
			{
				this.m_PCMSetPositionCallback(position);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern AudioClip Construct_Internal();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Init_Internal(string name, int lengthSamples, int channels, int frequency, bool _3D, bool stream);
	}
}
