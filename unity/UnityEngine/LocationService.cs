using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class LocationService
	{
		public bool isEnabledByUser
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public LocationServiceStatus status
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public LocationInfo lastData
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Start(float desiredAccuracyInMeters, float updateDistanceInMeters);

		public void Start(float desiredAccuracyInMeters)
		{
			float updateDistanceInMeters = 10f;
			Start(desiredAccuracyInMeters, updateDistanceInMeters);
		}

		public void Start()
		{
			float updateDistanceInMeters = 10f;
			float desiredAccuracyInMeters = 10f;
			Start(desiredAccuracyInMeters, updateDistanceInMeters);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Stop();
	}
}
