using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class WebCamTexture : Texture
	{
		public bool isPlaying
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string deviceName
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float requestedFPS
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int requestedWidth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int requestedHeight
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static WebCamDevice[] devices
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool didUpdateThisFrame
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public WebCamTexture(string deviceName, int requestedWidth, int requestedHeight, int requestedFPS)
		{
			Internal_CreateWebCamTexture(deviceName, requestedWidth, requestedHeight, requestedFPS);
		}

		public WebCamTexture(string deviceName, int requestedWidth, int requestedHeight)
		{
			Internal_CreateWebCamTexture(deviceName, requestedWidth, requestedHeight, 0);
		}

		public WebCamTexture(string deviceName)
		{
			Internal_CreateWebCamTexture(deviceName, 0, 0, 0);
		}

		public WebCamTexture(int requestedWidth, int requestedHeight, int requestedFPS)
		{
			Internal_CreateWebCamTexture(string.Empty, requestedWidth, requestedHeight, requestedFPS);
		}

		public WebCamTexture(int requestedWidth, int requestedHeight)
		{
			Internal_CreateWebCamTexture(string.Empty, requestedWidth, requestedHeight, 0);
		}

		public WebCamTexture()
		{
			Internal_CreateWebCamTexture(string.Empty, 0, 0, 0);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void Internal_CreateWebCamTexture(string device, int requestedWidth, int requestedHeight, int maxFramerate);

		public void Play()
		{
			INTERNAL_CALL_Play(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Play(WebCamTexture self);

		public void Pause()
		{
			INTERNAL_CALL_Pause(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Pause(WebCamTexture self);

		public void Stop()
		{
			INTERNAL_CALL_Stop(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Stop(WebCamTexture self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color GetPixel(int x, int y);

		public Color[] GetPixels()
		{
			return GetPixels(0, 0, width, height);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color[] GetPixels(int x, int y, int blockWidth, int blockHeight);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color32[] GetPixels32(Color32[] colors);

		public Color32[] GetPixels32()
		{
			Color32[] colors = null;
			return GetPixels32(colors);
		}
	}
}
