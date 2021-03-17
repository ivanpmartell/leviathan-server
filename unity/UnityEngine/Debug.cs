using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Debug
	{
		public static bool isDebugBuild
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
		{
			INTERNAL_CALL_DrawLine(ref start, ref end, ref color, duration, depthTest);
		}

		public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
		{
			bool depthTest = true;
			INTERNAL_CALL_DrawLine(ref start, ref end, ref color, duration, depthTest);
		}

		public static void DrawLine(Vector3 start, Vector3 end, Color color)
		{
			bool depthTest = true;
			float duration = 0f;
			INTERNAL_CALL_DrawLine(ref start, ref end, ref color, duration, depthTest);
		}

		public static void DrawLine(Vector3 start, Vector3 end)
		{
			bool depthTest = true;
			float duration = 0f;
			Color color = Color.white;
			INTERNAL_CALL_DrawLine(ref start, ref end, ref color, duration, depthTest);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_DrawLine(ref Vector3 start, ref Vector3 end, ref Color color, float duration, bool depthTest);

		public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
		{
			bool depthTest = true;
			DrawRay(start, dir, color, duration, depthTest);
		}

		public static void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			bool depthTest = true;
			float duration = 0f;
			DrawRay(start, dir, color, duration, depthTest);
		}

		public static void DrawRay(Vector3 start, Vector3 dir)
		{
			bool depthTest = true;
			float duration = 0f;
			Color white = Color.white;
			DrawRay(start, dir, white, duration, depthTest);
		}

		public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest)
		{
			DrawLine(start, start + dir, color, duration, depthTest);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void Break();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void DebugBreak();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_Log(int level, string msg, Object obj);

		public static void Log(object message)
		{
			Internal_Log(0, (message == null) ? "Null" : message.ToString(), null);
		}

		public static void Log(object message, Object context)
		{
			Internal_Log(0, (message == null) ? "Null" : message.ToString(), context);
		}

		public static void LogError(object message)
		{
			Internal_Log(2, message.ToString(), null);
		}

		public static void LogError(object message, Object context)
		{
			Internal_Log(2, message.ToString(), context);
		}

		public static void LogWarning(object message)
		{
			Internal_Log(1, message.ToString(), null);
		}

		public static void LogWarning(object message, Object context)
		{
			Internal_Log(1, message.ToString(), context);
		}
	}
}
