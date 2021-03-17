using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class Event
	{
		private EventType m_Type;

		private Vector2 m_MousePosition;

		private Vector2 m_Delta;

		private int m_Button;

		private EventModifiers m_Modifiers;

		private float m_Pressure;

		private int m_ClickCount;

		private char m_Character;

		private short m_KeyCode;

		private IntPtr m_CommandName;

		private static Event m_Current;

		public EventType rawType => m_Type;

		public EventType type
		{
			get
			{
				if (current == this)
				{
					if (!GUI.enabled)
					{
						if (m_Type == EventType.Repaint || m_Type == EventType.Layout || m_Type == EventType.Used)
						{
							return m_Type;
						}
						return EventType.Ignore;
					}
					if (GUIClip.enabled)
					{
						return m_Type;
					}
					if (m_Type == EventType.MouseDown || m_Type == EventType.MouseUp || m_Type == EventType.DragPerform || m_Type == EventType.DragUpdated)
					{
						return EventType.Ignore;
					}
					return m_Type;
				}
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public Vector2 mousePosition
		{
			get
			{
				return m_MousePosition;
			}
			set
			{
				m_MousePosition = value;
			}
		}

		public Vector2 delta
		{
			get
			{
				return m_Delta;
			}
			set
			{
				m_Delta = value;
			}
		}

		[Obsolete("Use HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);", true)]
		public Ray mouseRay
		{
			get
			{
				return new Ray(Vector3.up, Vector3.up);
			}
			set
			{
			}
		}

		public int button
		{
			get
			{
				return m_Button;
			}
			set
			{
				m_Button = value;
			}
		}

		public EventModifiers modifiers
		{
			get
			{
				return m_Modifiers;
			}
			set
			{
				m_Modifiers = value;
			}
		}

		public float pressure
		{
			get
			{
				return m_Pressure;
			}
			set
			{
				m_Pressure = value;
			}
		}

		public int clickCount
		{
			get
			{
				return m_ClickCount;
			}
			set
			{
				m_ClickCount = value;
			}
		}

		public char character
		{
			get
			{
				return m_Character;
			}
			set
			{
				m_Character = value;
			}
		}

		public string commandName
		{
			get
			{
				return ConvertCStringToString(m_CommandName);
			}
			set
			{
				m_CommandName = ConvertStringToCString(m_CommandName, value);
			}
		}

		public KeyCode keyCode
		{
			get
			{
				return (KeyCode)m_KeyCode;
			}
			set
			{
				m_KeyCode = (short)value;
			}
		}

		public bool shift
		{
			get
			{
				return (m_Modifiers & EventModifiers.Shift) != 0;
			}
			set
			{
				if (!value)
				{
					m_Modifiers &= ~EventModifiers.Shift;
				}
				else
				{
					m_Modifiers |= EventModifiers.Shift;
				}
			}
		}

		public bool control
		{
			get
			{
				return (m_Modifiers & EventModifiers.Control) != 0;
			}
			set
			{
				if (!value)
				{
					m_Modifiers &= ~EventModifiers.Control;
				}
				else
				{
					m_Modifiers |= EventModifiers.Control;
				}
			}
		}

		public bool alt
		{
			get
			{
				return (m_Modifiers & EventModifiers.Alt) != 0;
			}
			set
			{
				if (!value)
				{
					m_Modifiers &= ~EventModifiers.Alt;
				}
				else
				{
					m_Modifiers |= EventModifiers.Alt;
				}
			}
		}

		public bool command
		{
			get
			{
				return (m_Modifiers & EventModifiers.Command) != 0;
			}
			set
			{
				if (!value)
				{
					m_Modifiers &= ~EventModifiers.Command;
				}
				else
				{
					m_Modifiers |= EventModifiers.Command;
				}
			}
		}

		public bool capsLock
		{
			get
			{
				return (m_Modifiers & EventModifiers.CapsLock) != 0;
			}
			set
			{
				if (!value)
				{
					m_Modifiers &= ~EventModifiers.CapsLock;
				}
				else
				{
					m_Modifiers |= EventModifiers.CapsLock;
				}
			}
		}

		public bool numeric
		{
			get
			{
				return (m_Modifiers & EventModifiers.Numeric) != 0;
			}
			set
			{
				if (!value)
				{
					m_Modifiers &= ~EventModifiers.Shift;
				}
				else
				{
					m_Modifiers |= EventModifiers.Shift;
				}
			}
		}

		public bool functionKey => (m_Modifiers & EventModifiers.FunctionKey) != 0;

		public static Event current
		{
			get
			{
				return m_Current;
			}
			set
			{
				m_Current = value;
			}
		}

		public bool isKey
		{
			get
			{
				EventType eventType = type;
				return eventType == EventType.KeyDown || eventType == EventType.KeyUp;
			}
		}

		public bool isMouse
		{
			get
			{
				EventType eventType = type;
				return eventType == EventType.MouseMove || eventType == EventType.MouseDown || eventType == EventType.MouseUp || eventType == EventType.MouseDrag;
			}
		}

		public Event()
		{
		}

		public Event(Event other)
		{
			CopyInternal(other, this);
		}

		public EventType GetTypeForControl(int controlID)
		{
			if (GUIUtility.hotControl == 0)
			{
				return type;
			}
			switch (m_Type)
			{
			case EventType.MouseDown:
			case EventType.MouseUp:
			case EventType.MouseMove:
			case EventType.MouseDrag:
				if (!GUI.enabled)
				{
					return EventType.Ignore;
				}
				if (GUIClip.enabled || GUIUtility.hotControl == controlID)
				{
					return m_Type;
				}
				return EventType.Ignore;
			case EventType.KeyDown:
			case EventType.KeyUp:
				if (!GUI.enabled)
				{
					return EventType.Ignore;
				}
				if (GUIClip.enabled || GUIUtility.hotControl == controlID || GUIUtility.keyboardControl == controlID)
				{
					return m_Type;
				}
				return EventType.Ignore;
			case EventType.ScrollWheel:
				if (!GUI.enabled)
				{
					return EventType.Ignore;
				}
				if (GUIClip.enabled || GUIUtility.hotControl == controlID || GUIUtility.keyboardControl == controlID)
				{
					return m_Type;
				}
				return EventType.Ignore;
			default:
				return m_Type;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void CopyInternal(object src, object dst);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern string ConvertCStringToString(IntPtr ptr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern IntPtr ConvertStringToCString(IntPtr old, string value);

		public void Use()
		{
			type = EventType.Used;
		}

		public static Event KeyboardEvent(string key)
		{
			Event @event = new Event();
			@event.type = EventType.KeyDown;
			if (key == null || key == string.Empty)
			{
				return @event;
			}
			int num = 0;
			bool flag = false;
			do
			{
				flag = true;
				if (num >= key.Length)
				{
					flag = false;
					break;
				}
				switch (key[num])
				{
				case '&':
					@event.modifiers |= EventModifiers.Alt;
					num++;
					break;
				case '^':
					@event.modifiers |= EventModifiers.Control;
					num++;
					break;
				case '%':
					@event.modifiers |= EventModifiers.Command;
					num++;
					break;
				case '#':
					@event.modifiers |= EventModifiers.Shift;
					num++;
					break;
				default:
					flag = false;
					break;
				}
			}
			while (flag);
			string text = key.Substring(num, key.Length - num).ToLower();
			switch (text)
			{
			case "[0]":
				@event.character = '0';
				@event.keyCode = KeyCode.Keypad0;
				break;
			case "[1]":
				@event.character = '1';
				@event.keyCode = KeyCode.Keypad1;
				break;
			case "[2]":
				@event.character = '2';
				@event.keyCode = KeyCode.Keypad2;
				break;
			case "[3]":
				@event.character = '3';
				@event.keyCode = KeyCode.Keypad3;
				break;
			case "[4]":
				@event.character = '4';
				@event.keyCode = KeyCode.Keypad4;
				break;
			case "[5]":
				@event.character = '5';
				@event.keyCode = KeyCode.Keypad5;
				break;
			case "[6]":
				@event.character = '6';
				@event.keyCode = KeyCode.Keypad6;
				break;
			case "[7]":
				@event.character = '7';
				@event.keyCode = KeyCode.Keypad7;
				break;
			case "[8]":
				@event.character = '8';
				@event.keyCode = KeyCode.Keypad8;
				break;
			case "[9]":
				@event.character = '9';
				@event.keyCode = KeyCode.Keypad9;
				break;
			case "[.]":
				@event.character = '.';
				@event.keyCode = KeyCode.KeypadPeriod;
				break;
			case "[/]":
				@event.character = '/';
				@event.keyCode = KeyCode.KeypadDivide;
				break;
			case "[-]":
				@event.character = '-';
				@event.keyCode = KeyCode.KeypadMinus;
				break;
			case "[+]":
				@event.character = '+';
				@event.keyCode = KeyCode.KeypadPlus;
				break;
			case "[=]":
				@event.character = '=';
				@event.keyCode = KeyCode.KeypadEquals;
				break;
			case "[equals]":
				@event.character = '=';
				@event.keyCode = KeyCode.KeypadEquals;
				break;
			case "[enter]":
				@event.character = '\n';
				@event.keyCode = KeyCode.KeypadEnter;
				break;
			case "up":
				@event.keyCode = KeyCode.UpArrow;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "down":
				@event.keyCode = KeyCode.DownArrow;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "left":
				@event.keyCode = KeyCode.LeftArrow;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "right":
				@event.keyCode = KeyCode.RightArrow;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "insert":
				@event.keyCode = KeyCode.Insert;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "home":
				@event.keyCode = KeyCode.Home;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "end":
				@event.keyCode = KeyCode.End;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "pgup":
				@event.keyCode = KeyCode.PageDown;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "page up":
				@event.keyCode = KeyCode.PageUp;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "pgdown":
				@event.keyCode = KeyCode.PageUp;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "page down":
				@event.keyCode = KeyCode.PageDown;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "backspace":
				@event.keyCode = KeyCode.Backspace;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "delete":
				@event.keyCode = KeyCode.Delete;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "tab":
				@event.keyCode = KeyCode.Tab;
				break;
			case "f1":
				@event.keyCode = KeyCode.F1;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f2":
				@event.keyCode = KeyCode.F2;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f3":
				@event.keyCode = KeyCode.F3;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f4":
				@event.keyCode = KeyCode.F4;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f5":
				@event.keyCode = KeyCode.F5;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f6":
				@event.keyCode = KeyCode.F6;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f7":
				@event.keyCode = KeyCode.F7;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f8":
				@event.keyCode = KeyCode.F8;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f9":
				@event.keyCode = KeyCode.F9;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f10":
				@event.keyCode = KeyCode.F10;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f11":
				@event.keyCode = KeyCode.F11;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f12":
				@event.keyCode = KeyCode.F12;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f13":
				@event.keyCode = KeyCode.F13;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f14":
				@event.keyCode = KeyCode.F14;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "f15":
				@event.keyCode = KeyCode.F15;
				@event.modifiers |= EventModifiers.FunctionKey;
				break;
			case "[esc]":
				@event.keyCode = KeyCode.Escape;
				break;
			case "return":
				@event.character = '\n';
				@event.keyCode = KeyCode.Return;
				@event.modifiers &= ~EventModifiers.FunctionKey;
				break;
			case "space":
				@event.keyCode = KeyCode.Space;
				@event.character = ' ';
				@event.modifiers &= ~EventModifiers.FunctionKey;
				break;
			default:
				if (text.Length != 1)
				{
					try
					{
						@event.keyCode = (KeyCode)(int)Enum.Parse(typeof(KeyCode), text, ignoreCase: true);
						return @event;
					}
					catch (ArgumentException)
					{
						Debug.LogError($"Unable to find key name that matches '{text}'");
						return @event;
					}
				}
				@event.character = text.ToLower()[0];
				@event.keyCode = (KeyCode)@event.character;
				if (@event.modifiers != 0)
				{
					@event.character = '\0';
				}
				break;
			}
			return @event;
		}

		public override int GetHashCode()
		{
			int num = 1;
			if (isKey)
			{
				num = (ushort)keyCode;
			}
			if (isMouse)
			{
				num = mousePosition.GetHashCode();
			}
			return (num * 37) | (int)modifiers;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			Event @event = (Event)obj;
			if (type != @event.type || modifiers != @event.modifiers)
			{
				return false;
			}
			if (isKey)
			{
				return keyCode == @event.keyCode && modifiers == @event.modifiers;
			}
			if (isMouse)
			{
				return mousePosition == @event.mousePosition;
			}
			return false;
		}

		public override string ToString()
		{
			if (isKey)
			{
				if (character == '\0')
				{
					return $"Event:{type}   Character:\\0   Modifiers:{modifiers}   KeyCode:{keyCode}";
				}
				return string.Format(string.Concat("Event:", type, "   Character:", (int)character, "   Modifiers:", modifiers, "   KeyCode:", keyCode));
			}
			if (isMouse)
			{
				return $"Event: {type}   Position: {mousePosition} Modifiers: {modifiers}";
			}
			return string.Empty + type;
		}
	}
}
