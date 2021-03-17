using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class GUIUtility
	{
		internal enum TabControlSearchMode
		{
			NotActive,
			LookingForPrevious,
			LookingForNext,
			Found
		}

		internal static Rect dummyRect = new Rect(0f, 0f, 1f, 1f);

		internal static GUI.ScrollViewState activeScrollView = null;

		internal static Hashtable keyboardRects = new Hashtable();

		internal static TabControlSearchMode s_TabControlSearchMode;

		internal static int s_FirstKeyControl;

		internal static int s_PreviousKeyControl;

		internal static int s_NextKeyControl;

		internal static int s_LastKeyControl;

		internal static IDList s_CurrentList = null;

		private static int s_HotControl;

		protected static int s_KeyboardControl;

		public static bool s_LogControlID = false;

		[NotRenamed]
		private static int s_SkinMode;

		[NotRenamed]
		internal static int s_OriginalID;

		[NotRenamed]
		internal static IDList s_idList;

		[NotRenamed]
		private static int s_useGUILayout;

		private static bool s_IsInOnGUI = false;

		internal static Vector2 s_EditorScreenPointOffset = Vector2.zero;

		internal static bool s_HasKeyboardFocus = false;

		public static int hotControl
		{
			get
			{
				return s_HotControl;
			}
			set
			{
				s_HotControl = value;
			}
		}

		public static int keyboardControl
		{
			get
			{
				return s_KeyboardControl;
			}
			set
			{
				((IKeyboardControl)QueryStateObject(typeof(IKeyboardControl), s_KeyboardControl))?.OnLostFocus();
				s_KeyboardControl = value;
				SetKeyboardScriptInstanceID(GetCurrentScriptInstanceID());
				((IKeyboardControl)QueryStateObject(typeof(IKeyboardControl), value))?.OnFocus();
			}
		}

		internal static string systemCopyBuffer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		internal static bool mouseUsed
		{
			get
			{
				return GetMouseUsed();
			}
			set
			{
				SetMouseUsed(value);
			}
		}

		internal static bool textFieldInput
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		private static void BeginTabControlSearch()
		{
			s_FirstKeyControl = (s_PreviousKeyControl = (s_NextKeyControl = (s_LastKeyControl = 0)));
			s_TabControlSearchMode = TabControlSearchMode.LookingForPrevious;
		}

		private static void EndTabControlSearch(int moveDirection)
		{
			if (s_TabControlSearchMode == TabControlSearchMode.NotActive)
			{
				Debug.LogError("TabControlSearchMode == NotActive but moveDirection is " + moveDirection);
				return;
			}
			switch (moveDirection)
			{
			case 1:
				switch (s_TabControlSearchMode)
				{
				case TabControlSearchMode.LookingForPrevious:
				case TabControlSearchMode.LookingForNext:
					s_KeyboardControl = s_FirstKeyControl;
					break;
				case TabControlSearchMode.Found:
					s_KeyboardControl = s_NextKeyControl;
					break;
				default:
					Debug.Log("Trying to move tab forward, TabControlSearchMode is " + s_TabControlSearchMode);
					break;
				}
				break;
			case -1:
				switch (s_TabControlSearchMode)
				{
				case TabControlSearchMode.LookingForPrevious:
					s_KeyboardControl = s_LastKeyControl;
					break;
				case TabControlSearchMode.LookingForNext:
				case TabControlSearchMode.Found:
					if (s_PreviousKeyControl != 0)
					{
						s_KeyboardControl = s_PreviousKeyControl;
					}
					else
					{
						s_KeyboardControl = s_LastKeyControl;
					}
					break;
				default:
					Debug.Log("Trying to move tab back, TabControlSearchMode is " + s_TabControlSearchMode);
					break;
				}
				break;
			}
		}

		internal static void MoveKeyboardFocus(string name)
		{
			IDList.NamedControl namedControl = IDList.s_KeyboardFocusNames[name] as IDList.NamedControl;
			if (namedControl != null && keyboardControl != namedControl.controlID)
			{
				namedControl.MoveKeyboardFocusToThisControl();
			}
		}

		internal static void Internal_SetKeyboardControl(int control)
		{
			s_KeyboardControl = control;
		}

		internal static int Internal_GetKeyboardControl()
		{
			return s_KeyboardControl;
		}

		internal static void SetNextKeyboardFocusName(string name)
		{
			IDList.s_NextKeyboardFocusName = name;
		}

		internal static string GetNameOfFocusedControl()
		{
			if (keyboardControl < 0)
			{
				return string.Empty;
			}
			foreach (DictionaryEntry s_KeyboardFocusName in IDList.s_KeyboardFocusNames)
			{
				if ((s_KeyboardFocusName.Value as IDList.NamedControl).controlID == keyboardControl)
				{
					return s_KeyboardFocusName.Key as string;
				}
			}
			return string.Empty;
		}

		public static int GetControlID(FocusType focus)
		{
			return s_CurrentList.GetNext(0, focus);
		}

		public static int GetControlID(int hint, FocusType focus)
		{
			return s_CurrentList.GetNext(hint, focus);
		}

		public static int GetControlID(GUIContent contents, FocusType focus)
		{
			return s_CurrentList.GetNext(contents.hash, focus);
		}

		public static int GetControlID(FocusType focus, Rect position)
		{
			return s_CurrentList.GetNext(0, focus, position);
		}

		public static int GetControlID(int hint, FocusType focus, Rect position)
		{
			return s_CurrentList.GetNext(hint, focus, position);
		}

		public static int GetControlID(GUIContent contents, FocusType focus, Rect position)
		{
			return s_CurrentList.GetNext(contents.hash, focus, position);
		}

		public static object GetStateObject(Type t, int controlID)
		{
			return s_CurrentList.GetStateObject(t, controlID);
		}

		public static object QueryStateObject(Type t, int controlID)
		{
			return s_CurrentList.QueryStateObject(t, controlID);
		}

		internal static void SelectIDList(IDList idlist, int instanceID, bool isWindow, bool clearFocusList, bool resetIDListCursor)
		{
			s_CurrentList = idlist;
			GUILayoutUtility.SelectIDList(instanceID, isWindow);
			if (resetIDListCursor && s_CurrentList != null)
			{
				s_CurrentList.Reset(clearFocusList);
			}
		}

		protected static void SkipToControlID(int id)
		{
			s_CurrentList.SkipToControlID(id);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void UpdateUndoName();

		internal static bool HotControlIsUsed()
		{
			return s_HotControl != 0;
		}

		public static void ExitGUI()
		{
			throw new ExitGUIException();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void SetDidGUIWindowsEatLastEvent(bool value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern Object Internal_LoadSkin(int skinNo, Type type);

		internal static GUISkin GetDefaultSkin()
		{
			return GetBuiltinSkin(s_SkinMode);
		}

		internal static GUISkin GetBuiltinSkin(int skin)
		{
			return Internal_LoadSkin(skin, typeof(GUISkin)) as GUISkin;
		}

		internal static void SetBeginGUIArguments(Event e, int skinMode, int instanceID, int useGUILayout, IDList idlist)
		{
			s_SkinMode = skinMode;
			s_OriginalID = instanceID;
			Event.current = e;
			s_idList = idlist;
			s_useGUILayout = useGUILayout;
		}

		[NotRenamed]
		internal static void BeginGUI()
		{
			s_IsInOnGUI = true;
			IDList idlist = s_idList;
			GUI.skin = null;
			int num = s_OriginalID;
			int num2 = s_useGUILayout;
			Event current = Event.current;
			GUI.s_EditorTooltip = string.Empty;
			GUI.ResetSettings();
			GUIClip.Begin(current, current.type);
			SelectIDList(idlist, num, isWindow: false, Event.current.type == EventType.Layout, resetIDListCursor: true);
			if (num2 != 0)
			{
				GUILayoutUtility.Begin(num);
			}
			GUI.changed = false;
			if (num != 0)
			{
				BeginKeyboard();
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_ExitGUI();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern IDList GetInGameWindowsIDList();

		[NotRenamed]
		internal static void EndGUI(int doLayout, int doWindows, IDList idlist)
		{
			try
			{
				s_CurrentList = idlist;
				if (doLayout != 0 && Event.current.type == EventType.Layout)
				{
					GUILayoutUtility.End();
				}
				if (doWindows != 0)
				{
					GUI.EndWindows(GetInGameWindowsIDList());
				}
				SelectIDList(idlist, s_OriginalID, isWindow: false, clearFocusList: false, resetIDListCursor: true);
				GUIClip.End(Event.current.type);
			}
			finally
			{
				Event.current = null;
				Internal_ExitGUI();
				s_IsInOnGUI = false;
				s_CurrentList = null;
			}
		}

		internal static bool EndGUIFromException(Exception exception)
		{
			if (exception == null)
			{
				return false;
			}
			if (!(exception is ExitGUIException) && !(exception.InnerException is ExitGUIException))
			{
				return false;
			}
			try
			{
				GUIClip.EndThroughException();
			}
			finally
			{
				Internal_ExitGUI();
				s_IsInOnGUI = false;
				s_CurrentList = null;
			}
			return true;
		}

		internal static void CheckOnGUI()
		{
			if (!s_IsInOnGUI)
			{
				throw new ArgumentException("You can only call GUI functions from inside OnGUI.");
			}
		}

		private static void BeginKeyboard()
		{
			keyboardRects.Clear();
			activeScrollView = null;
		}

		protected static void MoveNextAndScroll(bool forward)
		{
			int num = keyboardControl;
			MoveKeyboardFocus(keyboardControl, forward, sendToNextScriptIfNeeded: true);
			if (num != keyboardControl && keyboardRects.ContainsKey(keyboardControl) && activeScrollView != null)
			{
				activeScrollView.ScrollTo((Rect)keyboardRects[keyboardControl]);
			}
		}

		internal static void CycleKeyboardWithinCurrentList()
		{
			if (Event.current.type != EventType.KeyDown || Event.current.keyCode != KeyCode.Tab)
			{
				return;
			}
			int num = -1;
			ArrayList keyboardFocusIDs = s_CurrentList.keyboardFocusIDs;
			for (int i = 0; i < keyboardFocusIDs.Count; i++)
			{
				if ((int)keyboardFocusIDs[i] == keyboardControl)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				bool moveForward = true;
				if (Event.current.shift)
				{
					moveForward = false;
				}
				MoveKeyboardFocus((int)keyboardFocusIDs[num], moveForward, sendToNextScriptIfNeeded: false);
			}
		}

		protected static void MoveKeyboardFocus(int ID, bool moveForward, bool sendToNextScriptIfNeeded)
		{
			ArrayList keyboardFocusIDs = s_CurrentList.keyboardFocusIDs;
			int i;
			if (ID != -1)
			{
				for (i = 0; i < keyboardFocusIDs.Count && (int)keyboardFocusIDs[i] != ID; i++)
				{
				}
			}
			else
			{
				i = -1;
			}
			if (i == -1)
			{
				i = ((!moveForward) ? keyboardFocusIDs.Count : (-1));
			}
			i += (moveForward ? 1 : (-1));
			if (i >= keyboardFocusIDs.Count)
			{
				if (sendToNextScriptIfNeeded)
				{
					SetKeyboardDirection(1);
				}
				else if (keyboardFocusIDs.Count > 0)
				{
					keyboardControl = (int)keyboardFocusIDs[0];
				}
			}
			else if (i < 0)
			{
				if (sendToNextScriptIfNeeded)
				{
					SetKeyboardDirection(-1);
				}
				else if (keyboardFocusIDs.Count > 0)
				{
					keyboardControl = (int)keyboardFocusIDs[keyboardFocusIDs.Count - 1];
				}
			}
			else
			{
				keyboardControl = (int)keyboardFocusIDs[i];
			}
		}

		internal static bool DecodeKeyboardControl(FocusType focus)
		{
			return focus switch
			{
				FocusType.Passive => false, 
				FocusType.Keyboard => true, 
				FocusType.Native => false, 
				_ => true, 
			};
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void SetHasKeyboardControl(bool hasKeyboard);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void SetKeyboardDirection(int dir);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern int GetKeyboardDirection();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern bool CurrentScriptHasKeyboardFocus();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern bool GetMouseUsed();

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void SetMouseUsed(bool value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void SetKeyboardScriptInstanceID(int instanceID);

		internal static int GetCurrentScriptInstanceID()
		{
			return s_OriginalID;
		}

		public static Vector2 GUIToScreenPoint(Vector2 guiPoint)
		{
			return GUIClip.Unclip(guiPoint) + s_EditorScreenPointOffset;
		}

		public static Vector2 ScreenToGUIPoint(Vector2 screenPoint)
		{
			return GUIClip.Clip(screenPoint) - s_EditorScreenPointOffset;
		}

		public static void RotateAroundPivot(float angle, Vector2 pivotPoint)
		{
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.identity;
			Vector2 vector = GUIClip.Unclip(pivotPoint);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.Euler(0f, 0f, angle), Vector3.one) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
			GUI.matrix = matrix4x * matrix;
		}

		public static void ScaleAroundPivot(Vector2 scale, Vector2 pivotPoint)
		{
			Matrix4x4 matrix = GUI.matrix;
			Vector2 vector = GUIClip.Unclip(pivotPoint);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.identity, new Vector3(scale.x, scale.y, 1f)) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
			GUI.matrix = matrix4x * matrix;
		}
	}
}
