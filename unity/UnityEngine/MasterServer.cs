using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class MasterServer
	{
		public static string ipAddress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int port
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static int updateRate
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static bool dedicatedServer
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
		public static extern void RequestHostList(string gameTypeName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern HostData[] PollHostList();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void RegisterHost(string gameTypeName, string gameName, string comment);

		public static void RegisterHost(string gameTypeName, string gameName)
		{
			string empty = string.Empty;
			RegisterHost(gameTypeName, gameName, empty);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void UnregisterHost();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void ClearHostList();
	}
}
