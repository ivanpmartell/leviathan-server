using System.Runtime.CompilerServices;

namespace UnityEngine
{
	internal sealed class GUIClip
	{
		public static bool enabled
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static Rect topmostRect
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static Rect visibleRect
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public static void Push(Rect screenRect)
		{
			Push(Event.current, Event.current.type, screenRect, Vector2.zero, Vector2.zero, resetOffset: false);
		}

		public static void Push(Rect screenRect, Vector2 scrollOffset)
		{
			Push(Event.current, Event.current.type, screenRect, scrollOffset, Vector2.zero, resetOffset: false);
		}

		public static void Push(Event evt, EventType evtType, Rect screenRect, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset)
		{
			INTERNAL_CALL_Push(evt, evtType, ref screenRect, ref scrollOffset, ref renderOffset, resetOffset);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Push(Event evt, EventType evtType, ref Rect screenRect, ref Vector2 scrollOffset, ref Vector2 renderOffset, bool resetOffset);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void Pop(Event evt, EventType evtType);

		public static Vector2 Unclip(Vector2 pos)
		{
			Unclip_Vector2_icall(ref pos);
			return pos;
		}

		private static void Unclip_Vector2_icall(ref Vector2 pos)
		{
			INTERNAL_CALL_Unclip_Vector2_icall(ref pos);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Unclip_Vector2_icall(ref Vector2 pos);

		public static Rect Unclip(Rect rect)
		{
			Unclip_Rect_icall(ref rect);
			return rect;
		}

		private static void Unclip_Rect_icall(ref Rect rect)
		{
			INTERNAL_CALL_Unclip_Rect_icall(ref rect);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Unclip_Rect_icall(ref Rect rect);

		public static Vector2 Clip(Vector2 absolutePos)
		{
			Clip_Vector2_icall(ref absolutePos);
			return absolutePos;
		}

		private static void Clip_Vector2_icall(ref Vector2 absolutePos)
		{
			INTERNAL_CALL_Clip_Vector2_icall(ref absolutePos);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Clip_Vector2_icall(ref Vector2 absolutePos);

		public static Rect Clip(Rect absoluteRect)
		{
			Clip_Rect_icall(ref absoluteRect);
			return absoluteRect;
		}

		private static Rect Clip_Rect_icall(ref Rect absoluteRect)
		{
			return INTERNAL_CALL_Clip_Rect_icall(ref absoluteRect);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Rect INTERNAL_CALL_Clip_Rect_icall(ref Rect absoluteRect);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void Reapply(Event evt, EventType evtType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern Matrix4x4 GetMatrix();

		public static void SetMatrix(Event evt, EventType evtType, Matrix4x4 m)
		{
			INTERNAL_CALL_SetMatrix(evt, evtType, ref m);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetMatrix(Event evt, EventType evtType, ref Matrix4x4 m);

		internal static void SetGUIClipRect(Rect r)
		{
			INTERNAL_CALL_SetGUIClipRect(ref r);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetGUIClipRect(ref Rect r);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void Begin(Event evt, EventType evtType);

		public static void SetAbsoluteGUIScroll(Vector2 guiScroll)
		{
			INTERNAL_CALL_SetAbsoluteGUIScroll(ref guiScroll);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetAbsoluteGUIScroll(ref Vector2 guiScroll);

		public static void SetAbsoluteMousePosition(Vector2 pos)
		{
			INTERNAL_CALL_SetAbsoluteMousePosition(ref pos);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetAbsoluteMousePosition(ref Vector2 pos);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern void INTERNAL_GetAbsoluteMousePosition(out Vector2 output);

		public static Vector2 GetAbsoluteMousePosition()
		{
			INTERNAL_GetAbsoluteMousePosition(out var output);
			return output;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void End(EventType evtType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void EndThroughException();
	}
}
