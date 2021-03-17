using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class GUI
	{
		internal sealed class ScrollViewState
		{
			public Rect position;

			public Rect visibleRect;

			public Vector2 scrollPosition;

			public bool apply;

			public bool hasScrollTo;

			public Rect scrollTo;

			internal void ScrollTo(Rect position)
			{
				Vector2 vector = new Vector2(position.xMin, position.yMin);
				if (!hasScrollTo)
				{
					hasScrollTo = true;
					scrollTo.xMin = vector.x;
					scrollTo.yMin = vector.y;
					vector = new Vector2(position.xMax, position.yMax);
					scrollTo.xMax = vector.x;
					scrollTo.yMax = vector.y;
					hasScrollTo = true;
					Rect rect = visibleRect;
					rect.x += scrollPosition.x;
					rect.y += scrollPosition.y;
					Vector2 vector2 = new Vector2(scrollTo.xMax, scrollTo.yMax);
					Vector2 vector3 = new Vector2(scrollTo.xMin, scrollTo.yMin);
					if (vector2.x > rect.xMax)
					{
						scrollPosition.x += vector2.x - rect.xMax;
					}
					if (vector3.x < rect.xMin)
					{
						scrollPosition.x -= rect.xMin - vector3.x;
					}
					if (vector2.y > rect.yMax)
					{
						scrollPosition.y += vector2.y - rect.yMax;
					}
					if (vector3.y < rect.yMin)
					{
						scrollPosition.y -= rect.yMin - vector3.y;
					}
					apply = true;
					hasScrollTo = false;
				}
				else
				{
					scrollTo.x = Mathf.Min(vector.x, scrollTo.x);
					scrollTo.y = Mathf.Min(vector.y, scrollTo.y);
					vector = new Vector2(position.xMax, position.yMax);
					scrollTo.xMax = Mathf.Max(vector.x, scrollTo.xMax);
					scrollTo.yMax = Mathf.Max(vector.y, scrollTo.yMax);
				}
			}
		}

		internal sealed class _WindowList
		{
			internal static _WindowList instance = new _WindowList();

			internal static Hashtable s_EditorWindows;

			internal Hashtable windows = new Hashtable();

			internal _Window Get(int id)
			{
				_Window window = (_Window)windows[id];
				if (window == null)
				{
					Debug.LogError("can't find window with ID " + id);
				}
				return window;
			}
		}

		internal sealed class _Window : IComparable
		{
			internal static _Window current;

			internal Rect rect;

			internal int depth;

			internal float opacity;

			internal GUIStyle style;

			internal GUIContent title = new GUIContent();

			internal int id;

			internal bool used;

			internal WindowFunction func;

			internal bool moved;

			internal bool forceRect;

			internal Color color;

			internal Color backgroundColor;

			internal Color contentColor;

			internal GUISkin skin;

			internal Matrix4x4 matrix;

			internal int hashCode;

			internal bool enabled;

			internal _Window(int id)
			{
				this.id = id;
				hashCode = ("Window" + id).GetHashCode();
				depth = _WindowList.instance.windows.Count;
			}

			internal void Do()
			{
				GUIUtility.GetControlID(hashCode, FocusType.Passive);
				current = this;
				GUIClip.Push(rect);
				GUIStyle.showKeyboardFocus = focusedWindow == id;
				try
				{
					func(id);
				}
				finally
				{
					GUIStyle.showKeyboardFocus = true;
					GUIClip.Pop(Event.current, Event.current.type);
					current = null;
				}
			}

			internal void SetupGUIValues()
			{
				GUI.color = color;
				GUI.backgroundColor = backgroundColor;
				GUI.contentColor = contentColor;
				GUI.matrix = matrix;
				GUI.skin = skin;
				GUI.enabled = enabled;
			}

			public int CompareTo(object obj)
			{
				return depth - ((_Window)obj).depth;
			}
		}

		internal sealed class WindowDragState
		{
			public Vector2 dragStartPos = Vector2.zero;

			public Rect dragStartRect = new Rect(0f, 0f, 0f, 0f);
		}

		public delegate void WindowFunction(int id);

		private static float scrollStepSize;

		private static int scrollControlID;

		private static GUISkin s_Skin;

		private static bool s_Changed;

		private static Color s_Color;

		private static Color s_BackgroundColor;

		private static Color s_ContentColor;

		private static bool s_Enabled;

		internal static string s_KeyTooltip;

		internal static string s_EditorTooltip;

		internal static string s_MouseTooltip;

		internal static Rect s_ToolTipRect;

		private static int boxHash;

		private static int buttonHash;

		private static int repeatButtonHash;

		private static int toggleHash;

		private static int buttonGridHash;

		private static int sliderHash;

		private static int beginGroupHash;

		private static int scrollviewHash;

		private static Stack s_ScrollViewStates;

		internal static int focusedWindow;

		private static bool s_LayersChanged;

		private static _WindowList s_GameWindowList;

		internal static DateTime nextScrollStepTime { get; set; }

		internal static int scrollTroughSide { get; set; }

		public static GUISkin skin
		{
			get
			{
				GUIUtility.CheckOnGUI();
				return s_Skin;
			}
			set
			{
				GUIUtility.CheckOnGUI();
				if (!value)
				{
					value = GUIUtility.GetDefaultSkin();
				}
				s_Skin = value;
				value.MakeCurrent();
			}
		}

		public static Color color
		{
			get
			{
				return s_Color;
			}
			set
			{
				s_Color = value;
				UpdateColors();
			}
		}

		public static Color backgroundColor
		{
			get
			{
				return s_BackgroundColor;
			}
			set
			{
				s_BackgroundColor = value;
				UpdateColors();
			}
		}

		public static Color contentColor
		{
			get
			{
				return s_ContentColor;
			}
			set
			{
				s_ContentColor = value;
				UpdateColors();
			}
		}

		public static bool changed
		{
			get
			{
				return s_Changed;
			}
			set
			{
				s_Changed = value;
			}
		}

		public static bool enabled
		{
			get
			{
				return s_Enabled;
			}
			set
			{
				s_Enabled = value;
				Internal_SetEnabled(value);
			}
		}

		public static Matrix4x4 matrix
		{
			get
			{
				return GUIClip.GetMatrix();
			}
			set
			{
				GUIClip.SetMatrix(Event.current, Event.current.type, value);
			}
		}

		public static string tooltip
		{
			get
			{
				if (s_MouseTooltip != string.Empty)
				{
					return s_MouseTooltip;
				}
				if (s_KeyTooltip != string.Empty)
				{
					return s_KeyTooltip;
				}
				return s_EditorTooltip;
			}
			set
			{
				s_MouseTooltip = (s_KeyTooltip = (s_EditorTooltip = value));
			}
		}

		protected static string mouseTooltip => s_MouseTooltip;

		protected static Rect tooltipRect
		{
			get
			{
				return s_ToolTipRect;
			}
			set
			{
				s_ToolTipRect = value;
			}
		}

		public static int depth
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		private static Material blendMaterial
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		private static Material blitMaterial
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		internal static bool usePageScrollbars
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		static GUI()
		{
			scrollStepSize = 10f;
			s_Changed = false;
			s_Color = Color.white;
			s_BackgroundColor = Color.white;
			s_ContentColor = Color.white;
			s_Enabled = true;
			s_KeyTooltip = string.Empty;
			s_EditorTooltip = string.Empty;
			s_MouseTooltip = string.Empty;
			boxHash = "Box".GetHashCode();
			buttonHash = "Button".GetHashCode();
			repeatButtonHash = "repeatButton".GetHashCode();
			toggleHash = "Toggle".GetHashCode();
			buttonGridHash = "ButtonGrid".GetHashCode();
			sliderHash = "Slider".GetHashCode();
			beginGroupHash = "BeginGroup".GetHashCode();
			scrollviewHash = "scrollView".GetHashCode();
			s_ScrollViewStates = new Stack();
			focusedWindow = -1;
			s_LayersChanged = false;
			nextScrollStepTime = DateTime.Now;
		}

		internal static void ResetSettings()
		{
			s_Color = Color.white;
			s_BackgroundColor = Color.white;
			s_ContentColor = Color.white;
			enabled = true;
			s_MouseTooltip = (s_KeyTooltip = string.Empty);
			UpdateColors();
		}

		private static void UpdateColors()
		{
			Internal_UpdateColors(s_Color, s_BackgroundColor, s_ContentColor);
		}

		private static void Internal_UpdateColors(Color col, Color backgroundCol, Color contentCol)
		{
			INTERNAL_CALL_Internal_UpdateColors(ref col, ref backgroundCol, ref contentCol);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Internal_UpdateColors(ref Color col, ref Color backgroundCol, ref Color contentCol);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_SetEnabled(bool enab);

		public static void Label(Rect position, string text)
		{
			Label(position, GUIContent.Temp(text), s_Skin.label);
		}

		public static void Label(Rect position, Texture image)
		{
			Label(position, GUIContent.Temp(image), s_Skin.label);
		}

		public static void Label(Rect position, GUIContent content)
		{
			Label(position, content, s_Skin.label);
		}

		public static void Label(Rect position, string text, GUIStyle style)
		{
			Label(position, GUIContent.Temp(text), style);
		}

		public static void Label(Rect position, Texture image, GUIStyle style)
		{
			Label(position, GUIContent.Temp(image), style);
		}

		public static void Label(Rect position, GUIContent content, GUIStyle style)
		{
			GUIUtility.CheckOnGUI();
			if (Event.current.type == EventType.Repaint)
			{
				style.Draw(position, content, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
				if (content.tooltip != null && content.tooltip != string.Empty && position.Contains(Event.current.mousePosition) && GUIClip.visibleRect.Contains(Event.current.mousePosition))
				{
					s_EditorTooltip = (s_MouseTooltip = content.tooltip);
					Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
					s_ToolTipRect = new Rect(vector.x, vector.y, position.width, position.height);
				}
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void InitializeGUIClipTexture();

		public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend)
		{
			float imageAspect = 0f;
			DrawTexture(position, image, scaleMode, alphaBlend, imageAspect);
		}

		public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode)
		{
			float imageAspect = 0f;
			bool alphaBlend = true;
			DrawTexture(position, image, scaleMode, alphaBlend, imageAspect);
		}

		public static void DrawTexture(Rect position, Texture image)
		{
			float imageAspect = 0f;
			bool alphaBlend = true;
			ScaleMode scaleMode = ScaleMode.StretchToFill;
			DrawTexture(position, image, scaleMode, alphaBlend, imageAspect);
		}

		public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (imageAspect == 0f)
			{
				imageAspect = (float)image.width / (float)image.height;
			}
			Material mat = ((!alphaBlend) ? blitMaterial : blendMaterial);
			float num = position.width / position.height;
			InternalDrawTextureArguments arguments = default(InternalDrawTextureArguments);
			arguments.texture = image;
			arguments.leftBorder = 0;
			arguments.rightBorder = 0;
			arguments.topBorder = 0;
			arguments.bottomBorder = 0;
			arguments.color = color;
			arguments.mat = mat;
			switch (scaleMode)
			{
			case ScaleMode.StretchToFill:
				arguments.screenRect = position;
				arguments.sourceRect = new Rect(0f, 0f, 1f, 1f);
				Graphics.DrawTexture(ref arguments);
				break;
			case ScaleMode.ScaleAndCrop:
				if (num > imageAspect)
				{
					float num4 = imageAspect / num;
					arguments.screenRect = position;
					arguments.sourceRect = new Rect(0f, (1f - num4) * 0.5f, 1f, num4);
					Graphics.DrawTexture(ref arguments);
				}
				else
				{
					float num5 = num / imageAspect;
					arguments.screenRect = position;
					arguments.sourceRect = new Rect(0.5f - num5 * 0.5f, 0f, num5, 1f);
					Graphics.DrawTexture(ref arguments);
				}
				break;
			case ScaleMode.ScaleToFit:
				if (num > imageAspect)
				{
					float num2 = imageAspect / num;
					arguments.screenRect = new Rect(position.xMin + position.width * (1f - num2) * 0.5f, position.yMin, num2 * position.width, position.height);
					arguments.sourceRect = new Rect(0f, 0f, 1f, 1f);
					Graphics.DrawTexture(ref arguments);
				}
				else
				{
					float num3 = num / imageAspect;
					arguments.screenRect = new Rect(position.xMin, position.yMin + position.height * (1f - num3) * 0.5f, position.width, num3 * position.height);
					arguments.sourceRect = new Rect(0f, 0f, 1f, 1f);
					Graphics.DrawTexture(ref arguments);
				}
				break;
			}
		}

		public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords)
		{
			bool alphaBlend = true;
			DrawTextureWithTexCoords(position, image, texCoords, alphaBlend);
		}

		public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords, bool alphaBlend)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Material mat = ((!alphaBlend) ? blitMaterial : blendMaterial);
				InternalDrawTextureArguments arguments = default(InternalDrawTextureArguments);
				arguments.texture = image;
				arguments.leftBorder = 0;
				arguments.rightBorder = 0;
				arguments.topBorder = 0;
				arguments.bottomBorder = 0;
				arguments.color = color;
				arguments.mat = mat;
				arguments.screenRect = position;
				arguments.sourceRect = texCoords;
				Graphics.DrawTexture(ref arguments);
			}
		}

		public static void Box(Rect position, string text)
		{
			Box(position, GUIContent.Temp(text), s_Skin.box);
		}

		public static void Box(Rect position, Texture image)
		{
			Box(position, GUIContent.Temp(image), s_Skin.box);
		}

		public static void Box(Rect position, GUIContent content)
		{
			Box(position, content, s_Skin.box);
		}

		public static void Box(Rect position, string text, GUIStyle style)
		{
			Box(position, GUIContent.Temp(text), style);
		}

		public static void Box(Rect position, Texture image, GUIStyle style)
		{
			Box(position, GUIContent.Temp(image), style);
		}

		public static void Box(Rect position, GUIContent content, GUIStyle style)
		{
			GUIUtility.CheckOnGUI();
			int controlID = GUIUtility.GetControlID(boxHash, FocusType.Passive);
			if (Event.current.type == EventType.Repaint)
			{
				style.Draw(position, content, controlID);
			}
		}

		public static bool Button(Rect position, string text)
		{
			return Button(position, GUIContent.Temp(text), s_Skin.button);
		}

		public static bool Button(Rect position, Texture image)
		{
			return Button(position, GUIContent.Temp(image), s_Skin.button);
		}

		public static bool Button(Rect position, GUIContent content)
		{
			return Button(position, content, s_Skin.button);
		}

		public static bool Button(Rect position, string text, GUIStyle style)
		{
			return Button(position, GUIContent.Temp(text), style);
		}

		public static bool Button(Rect position, Texture image, GUIStyle style)
		{
			return Button(position, GUIContent.Temp(image), style);
		}

		public static bool Button(Rect position, GUIContent content, GUIStyle style)
		{
			GUIUtility.CheckOnGUI();
			int controlID = GUIUtility.GetControlID(buttonHash, FocusType.Native, position);
			switch (Event.current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:
				if (position.Contains(Event.current.mousePosition))
				{
					GUIUtility.hotControl = controlID;
					Event.current.Use();
				}
				return false;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
					return position.Contains(Event.current.mousePosition);
				}
				return false;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					Event.current.Use();
				}
				break;
			case EventType.Repaint:
				style.Draw(position, content, controlID);
				break;
			}
			return false;
		}

		public static bool RepeatButton(Rect position, string text)
		{
			return DoRepeatButton(position, GUIContent.Temp(text), s_Skin.button, FocusType.Native);
		}

		public static bool RepeatButton(Rect position, Texture image)
		{
			return DoRepeatButton(position, GUIContent.Temp(image), s_Skin.button, FocusType.Native);
		}

		public static bool RepeatButton(Rect position, GUIContent content)
		{
			return DoRepeatButton(position, content, s_Skin.button, FocusType.Native);
		}

		public static bool RepeatButton(Rect position, string text, GUIStyle style)
		{
			return DoRepeatButton(position, GUIContent.Temp(text), style, FocusType.Native);
		}

		public static bool RepeatButton(Rect position, Texture image, GUIStyle style)
		{
			return DoRepeatButton(position, GUIContent.Temp(image), style, FocusType.Native);
		}

		public static bool RepeatButton(Rect position, GUIContent content, GUIStyle style)
		{
			return DoRepeatButton(position, content, style, FocusType.Native);
		}

		private static bool DoRepeatButton(Rect position, GUIContent content, GUIStyle style, FocusType focusType)
		{
			GUIUtility.CheckOnGUI();
			int controlID = GUIUtility.GetControlID(repeatButtonHash, focusType, position);
			switch (Event.current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:
				if (position.Contains(Event.current.mousePosition))
				{
					GUIUtility.hotControl = controlID;
					Event.current.Use();
				}
				return false;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
					return position.Contains(Event.current.mousePosition);
				}
				return false;
			case EventType.Repaint:
				style.Draw(position, content, controlID);
				return controlID == GUIUtility.hotControl && position.Contains(Event.current.mousePosition);
			default:
				return false;
			}
		}

		public static string TextField(Rect position, string text)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, -1, skin.textField);
			return gUIContent.text;
		}

		public static string TextField(Rect position, string text, int maxLength)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, maxLength, skin.textField);
			return gUIContent.text;
		}

		public static string TextField(Rect position, string text, GUIStyle style)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, -1, style);
			return gUIContent.text;
		}

		public static string TextField(Rect position, string text, int maxLength, GUIStyle style)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, maxLength, style);
			return gUIContent.text;
		}

		public static string PasswordField(Rect position, string password, char maskChar)
		{
			return PasswordField(position, password, maskChar, -1, skin.textField);
		}

		public static string PasswordField(Rect position, string password, char maskChar, int maxLength)
		{
			return PasswordField(position, password, maskChar, maxLength, skin.textField);
		}

		public static string PasswordField(Rect position, string password, char maskChar, GUIStyle style)
		{
			return PasswordField(position, password, maskChar, -1, style);
		}

		public static string PasswordField(Rect position, string password, char maskChar, int maxLength, GUIStyle style)
		{
			string t = PasswordFieldGetStrToShow(password, maskChar);
			GUIContent gUIContent = GUIContent.Temp(t);
			bool flag = changed;
			changed = false;
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, maxLength, style);
			t = ((!changed) ? password : gUIContent.text);
			changed |= flag;
			return t;
		}

		internal static string PasswordFieldGetStrToShow(string password, char maskChar)
		{
			return (Event.current.type != EventType.Repaint && Event.current.type != 0) ? password : string.Empty.PadRight(password.Length, maskChar);
		}

		public static string TextArea(Rect position, string text)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, -1, skin.textArea);
			return gUIContent.text;
		}

		public static string TextArea(Rect position, string text, int maxLength)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, maxLength, skin.textArea);
			return gUIContent.text;
		}

		public static string TextArea(Rect position, string text, GUIStyle style)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: true, -1, style);
			return gUIContent.text;
		}

		public static string TextArea(Rect position, string text, int maxLength, GUIStyle style)
		{
			GUIContent gUIContent = GUIContent.Temp(text);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, maxLength, style);
			return gUIContent.text;
		}

		private static string TextArea(Rect position, GUIContent content, int maxLength, GUIStyle style)
		{
			GUIContent gUIContent = GUIContent.Temp(content.text, content.image);
			DoTextField(position, GUIUtility.GetControlID(FocusType.Keyboard, position), gUIContent, multiline: false, maxLength, style);
			return gUIContent.text;
		}

		internal static void DoTextField(Rect position, int id, GUIContent content, bool multiline, int maxLength, GUIStyle style)
		{
			if (maxLength >= 0 && content.text.Length > maxLength)
			{
				content.text = content.text.Substring(0, maxLength);
			}
			GUIUtility.CheckOnGUI();
			TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), id);
			textEditor.content.text = content.text;
			textEditor.SaveBackup();
			textEditor.position = position;
			textEditor.style = style;
			textEditor.multiline = multiline;
			textEditor.controlID = id;
			textEditor.ClampPos();
			Event current = Event.current;
			bool flag = false;
			switch (current.type)
			{
			case EventType.MouseDown:
				if (position.Contains(current.mousePosition))
				{
					GUIUtility.hotControl = id;
					GUIUtility.keyboardControl = id;
					textEditor.MoveCursorToPosition(Event.current.mousePosition);
					if (Event.current.clickCount == 2 && skin.settings.doubleClickSelectsWord)
					{
						textEditor.SelectCurrentWord();
						textEditor.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
						textEditor.MouseDragSelectsWholeWords(on: true);
					}
					if (Event.current.clickCount == 3 && skin.settings.tripleClickSelectsLine)
					{
						textEditor.SelectCurrentParagraph();
						textEditor.MouseDragSelectsWholeWords(on: true);
						textEditor.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
					}
					current.Use();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == id)
				{
					if (current.shift)
					{
						textEditor.MoveCursorToPosition(Event.current.mousePosition);
					}
					else
					{
						textEditor.SelectToPosition(Event.current.mousePosition);
					}
					current.Use();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == id)
				{
					textEditor.MouseDragSelectsWholeWords(on: false);
					GUIUtility.hotControl = 0;
					current.Use();
				}
				break;
			case EventType.KeyDown:
			{
				if (GUIUtility.keyboardControl != id)
				{
					return;
				}
				if (textEditor.HandleKeyEvent(current))
				{
					current.Use();
					flag = true;
					content.text = textEditor.content.text;
					break;
				}
				if (current.keyCode == KeyCode.Tab || current.character == '\t')
				{
					return;
				}
				char character = current.character;
				if (character == '\n' && !multiline && !current.alt)
				{
					return;
				}
				Font font = style.font;
				if (!font)
				{
					font = skin.font;
				}
				if (font.HasCharacter(character) || character == '\n')
				{
					textEditor.Insert(character);
					flag = true;
				}
				else if (character == '\0')
				{
					if (Input.compositionString.Length > 0)
					{
						textEditor.ReplaceSelection(string.Empty);
						flag = true;
					}
					current.Use();
				}
				break;
			}
			case EventType.Repaint:
				if (GUIUtility.keyboardControl != id)
				{
					style.Draw(position, content, id, on: false);
				}
				else
				{
					textEditor.DrawCursor(content.text);
				}
				break;
			}
			if (GUIUtility.keyboardControl == id)
			{
				GUIUtility.textFieldInput = true;
			}
			if (flag)
			{
				changed = true;
				content.text = textEditor.content.text;
				if (maxLength >= 0 && content.text.Length > maxLength)
				{
					content.text = content.text.Substring(0, maxLength);
				}
				current.Use();
			}
		}

		public static void SetNextControlName(string name)
		{
			GUIUtility.SetNextKeyboardFocusName(name);
		}

		public static string GetNameOfFocusedControl()
		{
			return GUIUtility.GetNameOfFocusedControl();
		}

		public static void FocusControl(string name)
		{
			GUIUtility.MoveKeyboardFocus(name);
		}

		public static bool Toggle(Rect position, bool value, string text)
		{
			return Toggle(position, value, GUIContent.Temp(text), s_Skin.toggle);
		}

		public static bool Toggle(Rect position, bool value, Texture image)
		{
			return Toggle(position, value, GUIContent.Temp(image), s_Skin.toggle);
		}

		public static bool Toggle(Rect position, bool value, GUIContent content)
		{
			return Toggle(position, value, content, s_Skin.toggle);
		}

		public static bool Toggle(Rect position, bool value, string text, GUIStyle style)
		{
			return Toggle(position, value, GUIContent.Temp(text), style);
		}

		public static bool Toggle(Rect position, bool value, Texture image, GUIStyle style)
		{
			return Toggle(position, value, GUIContent.Temp(image), style);
		}

		public static bool Toggle(Rect position, bool value, GUIContent content, GUIStyle style)
		{
			return DoToggle(position, GUIUtility.GetControlID(toggleHash, FocusType.Native, position), value, content, style);
		}

		protected static bool DoToggle(Rect position, int id, bool value, GUIContent content, GUIStyle style)
		{
			GUIUtility.CheckOnGUI();
			switch (Event.current.GetTypeForControl(id))
			{
			case EventType.MouseDown:
				if (position.Contains(Event.current.mousePosition))
				{
					GUIUtility.hotControl = id;
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == id)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
					if (position.Contains(Event.current.mousePosition))
					{
						s_Changed = true;
						return !value;
					}
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == id)
				{
					Event.current.Use();
				}
				break;
			case EventType.Repaint:
				style.Draw(position, content, id, value);
				break;
			}
			return value;
		}

		public static int Toolbar(Rect position, int selected, string[] texts)
		{
			return Toolbar(position, selected, GUIContent.Temp(texts), s_Skin.button);
		}

		public static int Toolbar(Rect position, int selected, Texture[] images)
		{
			return Toolbar(position, selected, GUIContent.Temp(images), s_Skin.button);
		}

		public static int Toolbar(Rect position, int selected, GUIContent[] content)
		{
			return Toolbar(position, selected, content, s_Skin.button);
		}

		public static int Toolbar(Rect position, int selected, string[] texts, GUIStyle style)
		{
			return Toolbar(position, selected, GUIContent.Temp(texts), style);
		}

		public static int Toolbar(Rect position, int selected, Texture[] images, GUIStyle style)
		{
			return Toolbar(position, selected, GUIContent.Temp(images), style);
		}

		public static int Toolbar(Rect position, int selected, GUIContent[] contents, GUIStyle style)
		{
			FindStyles(ref style, out var firstStyle, out var midStyle, out var lastStyle, "left", "mid", "right");
			return DoButtonGrid(position, selected, contents, contents.Length, style, firstStyle, midStyle, lastStyle);
		}

		public static int SelectionGrid(Rect position, int selected, string[] texts, int xCount)
		{
			return SelectionGrid(position, selected, GUIContent.Temp(texts), xCount, null);
		}

		public static int SelectionGrid(Rect position, int selected, Texture[] images, int xCount)
		{
			return SelectionGrid(position, selected, GUIContent.Temp(images), xCount, null);
		}

		public static int SelectionGrid(Rect position, int selected, GUIContent[] content, int xCount)
		{
			return SelectionGrid(position, selected, content, xCount, null);
		}

		public static int SelectionGrid(Rect position, int selected, string[] texts, int xCount, GUIStyle style)
		{
			return SelectionGrid(position, selected, GUIContent.Temp(texts), xCount, style);
		}

		public static int SelectionGrid(Rect position, int selected, Texture[] images, int xCount, GUIStyle style)
		{
			return SelectionGrid(position, selected, GUIContent.Temp(images), xCount, style);
		}

		public static int SelectionGrid(Rect position, int selected, GUIContent[] contents, int xCount, GUIStyle style)
		{
			if (style == null)
			{
				style = s_Skin.button;
			}
			return DoButtonGrid(position, selected, contents, xCount, style, style, style, style);
		}

		internal static void FindStyles(ref GUIStyle style, out GUIStyle firstStyle, out GUIStyle midStyle, out GUIStyle lastStyle, string first, string mid, string last)
		{
			if (style == null)
			{
				style = skin.button;
			}
			string name = style.name;
			midStyle = skin.FindStyle(name + mid);
			if (midStyle == null)
			{
				midStyle = style;
			}
			firstStyle = skin.FindStyle(name + first);
			if (firstStyle == null)
			{
				firstStyle = midStyle;
			}
			lastStyle = skin.FindStyle(name + last);
			if (lastStyle == null)
			{
				lastStyle = midStyle;
			}
		}

		internal static int CalcTotalHorizSpacing(int xCount, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle)
		{
			if (xCount < 2)
			{
				return 0;
			}
			if (xCount == 2)
			{
				return Mathf.Max(firstStyle.margin.right, lastStyle.margin.left);
			}
			int num = Mathf.Max(midStyle.margin.left, midStyle.margin.right);
			return Mathf.Max(firstStyle.margin.right, midStyle.margin.left) + Mathf.Max(midStyle.margin.right, lastStyle.margin.left) + num * (xCount - 3);
		}

		private static int DoButtonGrid(Rect position, int selected, GUIContent[] contents, int xCount, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle)
		{
			GUIUtility.CheckOnGUI();
			int num = contents.Length;
			if (num == 0)
			{
				return selected;
			}
			int controlID = GUIUtility.GetControlID(buttonGridHash, FocusType.Native, position);
			int num2 = num / xCount;
			if (num % xCount != 0)
			{
				num2++;
			}
			float num3 = CalcTotalHorizSpacing(xCount, style, firstStyle, midStyle, lastStyle);
			float num4 = Mathf.Max(style.margin.top, style.margin.bottom) * (num2 - 1);
			float elemWidth = (position.width - num3) / (float)xCount;
			float elemHeight = (position.height - num4) / (float)num2;
			if (style.fixedWidth != 0f)
			{
				elemWidth = style.fixedWidth;
			}
			if (style.fixedHeight != 0f)
			{
				elemHeight = style.fixedHeight;
			}
			switch (Event.current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:
				if (position.Contains(Event.current.mousePosition))
				{
					Rect[] array = CalcMouseRects(position, num, xCount, elemWidth, elemHeight, style, firstStyle, midStyle, lastStyle, addBorders: false);
					if (GetButtonGridMouseSelection(array, Event.current.mousePosition, findNearest: true) != -1)
					{
						GUIUtility.hotControl = controlID;
						Event.current.Use();
					}
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
					Rect[] array = CalcMouseRects(position, num, xCount, elemWidth, elemHeight, style, firstStyle, midStyle, lastStyle, addBorders: false);
					int buttonGridMouseSelection2 = GetButtonGridMouseSelection(array, Event.current.mousePosition, findNearest: true);
					changed = true;
					return buttonGridMouseSelection2;
				}
				break;
			case EventType.Repaint:
			{
				GUIStyle gUIStyle = null;
				GUIClip.Push(position);
				position = new Rect(0f, 0f, position.width, position.height);
				Rect[] array = CalcMouseRects(position, num, xCount, elemWidth, elemHeight, style, firstStyle, midStyle, lastStyle, addBorders: false);
				int buttonGridMouseSelection = GetButtonGridMouseSelection(array, Event.current.mousePosition, controlID == GUIUtility.hotControl);
				bool flag = position.Contains(Event.current.mousePosition);
				GUIUtility.mouseUsed |= flag;
				for (int i = 0; i < num; i++)
				{
					GUIStyle gUIStyle2 = null;
					gUIStyle2 = ((i == 0) ? firstStyle : midStyle);
					if (i == num - 1)
					{
						gUIStyle2 = lastStyle;
					}
					if (num == 1)
					{
						gUIStyle2 = style;
					}
					if (i != selected)
					{
						gUIStyle2.Draw(array[i], contents[i], i == buttonGridMouseSelection && (GUIClip.enabled || controlID == GUIUtility.hotControl) && (controlID == GUIUtility.hotControl || GUIUtility.hotControl == 0), controlID == GUIUtility.hotControl && enabled, on: false, hasKeyboardFocus: false);
					}
					else
					{
						gUIStyle = gUIStyle2;
					}
				}
				if (selected < num && selected > -1)
				{
					gUIStyle.Draw(array[selected], contents[selected], selected == buttonGridMouseSelection && (GUIClip.enabled || controlID == GUIUtility.hotControl) && (controlID == GUIUtility.hotControl || GUIUtility.hotControl == 0), controlID == GUIUtility.hotControl || (selected == buttonGridMouseSelection && GUIUtility.hotControl == 0), on: true, hasKeyboardFocus: false);
				}
				GUIClip.Pop(Event.current, Event.current.type);
				break;
			}
			}
			return selected;
		}

		private static Rect[] CalcMouseRects(Rect position, int count, int xCount, float elemWidth, float elemHeight, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle, bool addBorders)
		{
			int num = 0;
			int num2 = 0;
			float num3 = position.xMin;
			float num4 = position.yMin;
			GUIStyle gUIStyle = style;
			Rect[] array = new Rect[count];
			if (count > 1)
			{
				gUIStyle = firstStyle;
			}
			for (int i = 0; i < count; i++)
			{
				if (!addBorders)
				{
					ref Rect reference = ref array[i];
					reference = new Rect(num3, num4, elemWidth, elemHeight);
				}
				else
				{
					ref Rect reference2 = ref array[i];
					reference2 = gUIStyle.margin.Add(new Rect(num3, num4, elemWidth, elemHeight));
				}
				array[i].width = Mathf.Round(array[i].xMax) - Mathf.Round(array[i].x);
				array[i].x = Mathf.Round(array[i].x);
				GUIStyle gUIStyle2 = midStyle;
				if (i == count - 2)
				{
					gUIStyle2 = lastStyle;
				}
				num3 += elemWidth + (float)Mathf.Max(gUIStyle.margin.right, gUIStyle2.margin.left);
				num2++;
				if (num2 >= xCount)
				{
					num++;
					num2 = 0;
					num4 += elemHeight + (float)Mathf.Max(style.margin.top, style.margin.bottom);
					num3 = position.xMin;
				}
			}
			return array;
		}

		private static int GetButtonGridMouseSelection(Rect[] buttonRects, Vector2 mousePos, bool findNearest)
		{
			for (int i = 0; i < buttonRects.Length; i++)
			{
				if (buttonRects[i].Contains(mousePos))
				{
					return i;
				}
			}
			if (!findNearest)
			{
				return -1;
			}
			float num = 1E+07f;
			int result = -1;
			for (int j = 0; j < buttonRects.Length; j++)
			{
				Rect rect = buttonRects[j];
				Vector2 vector = new Vector2(Mathf.Clamp(mousePos.x, rect.xMin, rect.xMax), Mathf.Clamp(mousePos.y, rect.yMin, rect.yMax));
				float sqrMagnitude = (mousePos - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = j;
					num = sqrMagnitude;
				}
			}
			return result;
		}

		public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue)
		{
			return Slider(position, value, 0f, leftValue, rightValue, skin.horizontalSlider, skin.horizontalSliderThumb, horiz: true, GUIUtility.GetControlID(sliderHash, FocusType.Native, position));
		}

		public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb)
		{
			return Slider(position, value, 0f, leftValue, rightValue, slider, thumb, horiz: true, GUIUtility.GetControlID(sliderHash, FocusType.Native, position));
		}

		public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue)
		{
			return Slider(position, value, 0f, topValue, bottomValue, skin.verticalSlider, skin.verticalSliderThumb, horiz: false, GUIUtility.GetControlID(sliderHash, FocusType.Native, position));
		}

		public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue, GUIStyle slider, GUIStyle thumb)
		{
			return Slider(position, value, 0f, topValue, bottomValue, slider, thumb, horiz: false, GUIUtility.GetControlID(sliderHash, FocusType.Native, position));
		}

		public static float Slider(Rect position, float value, float size, float start, float end, GUIStyle slider, GUIStyle thumb, bool horiz, int id)
		{
			GUIUtility.CheckOnGUI();
			return new SliderHandler(position, value, size, start, end, slider, thumb, horiz, id).Handle();
		}

		public static float HorizontalScrollbar(Rect position, float value, float size, float leftValue, float rightValue)
		{
			return Scroller(position, value, size, leftValue, rightValue, skin.horizontalScrollbar, skin.horizontalScrollbarThumb, skin.horizontalScrollbarLeftButton, skin.horizontalScrollbarRightButton, horiz: true);
		}

		public static float HorizontalScrollbar(Rect position, float value, float size, float leftValue, float rightValue, GUIStyle style)
		{
			return Scroller(position, value, size, leftValue, rightValue, style, skin.GetStyle(style.name + "thumb"), skin.GetStyle(style.name + "leftbutton"), skin.GetStyle(style.name + "rightbutton"), horiz: true);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal static extern void InternalRepaintEditorWindow();

		private static bool ScrollerRepeatButton(int scrollerID, Rect rect, GUIStyle style)
		{
			bool result = false;
			if (DoRepeatButton(rect, GUIContent.none, style, FocusType.Passive))
			{
				bool flag = scrollControlID != scrollerID;
				scrollControlID = scrollerID;
				if (flag)
				{
					s_Changed = true;
					result = true;
					nextScrollStepTime = DateTime.Now.AddMilliseconds(250.0);
				}
				else if (DateTime.Now >= nextScrollStepTime)
				{
					s_Changed = true;
					result = true;
					nextScrollStepTime = DateTime.Now.AddMilliseconds(30.0);
				}
				if (Event.current.type == EventType.Repaint)
				{
					InternalRepaintEditorWindow();
				}
			}
			return result;
		}

		public static float VerticalScrollbar(Rect position, float value, float size, float topValue, float bottomValue)
		{
			return Scroller(position, value, size, topValue, bottomValue, skin.verticalScrollbar, skin.verticalScrollbarThumb, skin.verticalScrollbarUpButton, skin.verticalScrollbarDownButton, horiz: false);
		}

		public static float VerticalScrollbar(Rect position, float value, float size, float topValue, float bottomValue, GUIStyle style)
		{
			return Scroller(position, value, size, topValue, bottomValue, style, skin.GetStyle(style.name + "thumb"), skin.GetStyle(style.name + "upbutton"), skin.GetStyle(style.name + "downbutton"), horiz: false);
		}

		private static float Scroller(Rect position, float value, float size, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, GUIStyle leftButton, GUIStyle rightButton, bool horiz)
		{
			GUIUtility.CheckOnGUI();
			int controlID = GUIUtility.GetControlID(sliderHash, FocusType.Passive, position);
			Rect position2;
			Rect rect;
			Rect rect2;
			if (horiz)
			{
				position2 = new Rect(position.x + leftButton.fixedWidth, position.y, position.width - leftButton.fixedWidth - rightButton.fixedWidth, position.height);
				rect = new Rect(position.x, position.y, leftButton.fixedWidth, position.height);
				rect2 = new Rect(position.xMax - rightButton.fixedWidth, position.y, rightButton.fixedWidth, position.height);
			}
			else
			{
				position2 = new Rect(position.x, position.y + leftButton.fixedHeight, position.width, position.height - leftButton.fixedHeight - rightButton.fixedHeight);
				rect = new Rect(position.x, position.y, position.width, leftButton.fixedHeight);
				rect2 = new Rect(position.x, position.yMax - rightButton.fixedHeight, position.width, rightButton.fixedHeight);
			}
			value = Slider(position2, value, size, leftValue, rightValue, slider, thumb, horiz, controlID);
			bool flag = false;
			if (Event.current.type == EventType.MouseUp)
			{
				flag = true;
			}
			if (ScrollerRepeatButton(controlID, rect, leftButton))
			{
				value -= scrollStepSize * ((!(leftValue < rightValue)) ? (-1f) : 1f);
			}
			if (ScrollerRepeatButton(controlID, rect2, rightButton))
			{
				value += scrollStepSize * ((!(leftValue < rightValue)) ? (-1f) : 1f);
			}
			if (flag && Event.current.type == EventType.Used)
			{
				scrollControlID = 0;
			}
			value = ((!(leftValue < rightValue)) ? Mathf.Clamp(value, rightValue, leftValue - size) : Mathf.Clamp(value, leftValue, rightValue - size));
			return value;
		}

		public static void BeginGroup(Rect position)
		{
			BeginGroup(position, GUIContent.none, GUIStyle.none);
		}

		public static void BeginGroup(Rect position, string text)
		{
			BeginGroup(position, GUIContent.Temp(text), GUIStyle.none);
		}

		public static void BeginGroup(Rect position, Texture image)
		{
			BeginGroup(position, GUIContent.Temp(image), GUIStyle.none);
		}

		public static void BeginGroup(Rect position, GUIContent content)
		{
			BeginGroup(position, content, GUIStyle.none);
		}

		public static void BeginGroup(Rect position, GUIStyle style)
		{
			BeginGroup(position, GUIContent.none, style);
		}

		public static void BeginGroup(Rect position, string text, GUIStyle style)
		{
			BeginGroup(position, GUIContent.Temp(text), style);
		}

		public static void BeginGroup(Rect position, Texture image, GUIStyle style)
		{
			BeginGroup(position, GUIContent.Temp(image), style);
		}

		public static void BeginGroup(Rect position, GUIContent content, GUIStyle style)
		{
			GUIUtility.CheckOnGUI();
			int controlID = GUIUtility.GetControlID(beginGroupHash, FocusType.Passive);
			if (content != GUIContent.none || style != GUIStyle.none)
			{
				EventType type = Event.current.type;
				if (type == EventType.Repaint)
				{
					style.Draw(position, content, controlID);
				}
				else if (position.Contains(Event.current.mousePosition))
				{
					GUIUtility.mouseUsed = true;
				}
			}
			GUIClip.Push(position);
		}

		public static void EndGroup()
		{
			GUIClip.Pop(Event.current, Event.current.type);
		}

		public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect)
		{
			return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal: false, alwaysShowVertical: false, skin.horizontalScrollbar, skin.verticalScrollbar, skin.scrollView);
		}

		public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical)
		{
			return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, skin.horizontalScrollbar, skin.verticalScrollbar, skin.scrollView);
		}

		public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
		{
			return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal: false, alwaysShowVertical: false, horizontalScrollbar, verticalScrollbar, skin.scrollView);
		}

		public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar)
		{
			return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, null);
		}

		protected static Vector2 DoBeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background)
		{
			return BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background);
		}

		internal static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background)
		{
			GUIUtility.CheckOnGUI();
			int controlID = GUIUtility.GetControlID(scrollviewHash, FocusType.Passive);
			ScrollViewState scrollViewState = (ScrollViewState)GUIUtility.GetStateObject(typeof(ScrollViewState), controlID);
			if (scrollViewState.apply)
			{
				scrollPosition = scrollViewState.scrollPosition;
				scrollViewState.apply = false;
			}
			scrollViewState.position = position;
			scrollViewState.scrollPosition = scrollPosition;
			scrollViewState.visibleRect = viewRect;
			scrollViewState.visibleRect.width = position.width - verticalScrollbar.fixedWidth + (float)verticalScrollbar.margin.left;
			scrollViewState.visibleRect.height = position.height - horizontalScrollbar.fixedHeight + (float)horizontalScrollbar.margin.top;
			s_ScrollViewStates.Push(scrollViewState);
			Rect screenRect = new Rect(position);
			switch (Event.current.type)
			{
			case EventType.Layout:
				GUIUtility.GetControlID(sliderHash, FocusType.Passive);
				GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
				GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
				GUIUtility.GetControlID(sliderHash, FocusType.Passive);
				GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
				GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
				break;
			default:
			{
				bool flag = alwaysShowVertical;
				bool flag2 = alwaysShowHorizontal;
				if (flag2 || viewRect.width > screenRect.width)
				{
					screenRect.height -= horizontalScrollbar.fixedHeight + (float)horizontalScrollbar.margin.top;
					flag2 = true;
				}
				if (flag || viewRect.height > screenRect.height)
				{
					screenRect.width -= verticalScrollbar.fixedWidth + (float)verticalScrollbar.margin.left;
					flag = true;
					if (!flag2 && viewRect.width > screenRect.width)
					{
						screenRect.height -= horizontalScrollbar.fixedHeight + (float)horizontalScrollbar.margin.top;
						flag2 = true;
					}
				}
				if (Event.current.type == EventType.Repaint && background != GUIStyle.none)
				{
					background.Draw(position, position.Contains(Event.current.mousePosition), isActive: false, flag2 && flag, hasKeyboardFocus: false);
				}
				if (flag2 && horizontalScrollbar != GUIStyle.none)
				{
					scrollPosition.x = HorizontalScrollbar(new Rect(position.x, position.yMax - horizontalScrollbar.fixedHeight, screenRect.width, horizontalScrollbar.fixedHeight), scrollPosition.x, screenRect.width, 0f, viewRect.width, horizontalScrollbar);
				}
				else
				{
					GUIUtility.GetControlID(sliderHash, FocusType.Passive);
					GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
					GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
					scrollPosition.x = 0f;
				}
				if (flag)
				{
					scrollPosition.y = VerticalScrollbar(new Rect(screenRect.xMax + (float)verticalScrollbar.margin.left, screenRect.y, verticalScrollbar.fixedWidth, screenRect.height), scrollPosition.y, screenRect.height, 0f, viewRect.height, verticalScrollbar);
					break;
				}
				scrollPosition.y = 0f;
				GUIUtility.GetControlID(sliderHash, FocusType.Passive);
				GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
				GUIUtility.GetControlID(repeatButtonHash, FocusType.Passive);
				break;
			}
			case EventType.Used:
				break;
			}
			GUIClip.Push(screenRect, new Vector2(Mathf.Round(0f - scrollPosition.x - viewRect.x), Mathf.Round(0f - scrollPosition.y - viewRect.y)));
			return scrollPosition;
		}

		public static void EndScrollView()
		{
			EndScrollView(handleScrollWheel: true);
		}

		public static void EndScrollView(bool handleScrollWheel)
		{
			ScrollViewState scrollViewState = (ScrollViewState)s_ScrollViewStates.Peek();
			GUIUtility.CheckOnGUI();
			GUIClip.Pop(Event.current, Event.current.type);
			s_ScrollViewStates.Pop();
			if (handleScrollWheel && Event.current.type == EventType.ScrollWheel && scrollViewState.position.Contains(Event.current.mousePosition))
			{
				scrollViewState.scrollPosition += Event.current.delta * 20f;
				scrollViewState.apply = true;
				Event.current.Use();
			}
		}

		internal static ScrollViewState GetTopScrollView()
		{
			if (s_ScrollViewStates.Count != 0)
			{
				return (ScrollViewState)s_ScrollViewStates.Peek();
			}
			return null;
		}

		public static void ScrollTo(Rect position)
		{
			ScrollViewState topScrollView = GetTopScrollView();
			topScrollView.ScrollTo(GUIClip.Unclip(position));
		}

		private static ArrayList GetSortedWindowList()
		{
			ArrayList arrayList = new ArrayList(_WindowList.instance.windows.Values);
			arrayList.Sort();
			return arrayList;
		}

		public static Rect Window(int id, Rect clientRect, WindowFunction func, string text)
		{
			return DoWindow(id, clientRect, func, GUIContent.Temp(text), skin.window, forceRectOnLayout: true);
		}

		public static Rect Window(int id, Rect clientRect, WindowFunction func, Texture image)
		{
			return DoWindow(id, clientRect, func, GUIContent.Temp(image), skin.window, forceRectOnLayout: true);
		}

		public static Rect Window(int id, Rect clientRect, WindowFunction func, GUIContent content)
		{
			return DoWindow(id, clientRect, func, content, skin.window, forceRectOnLayout: true);
		}

		public static Rect Window(int id, Rect clientRect, WindowFunction func, string text, GUIStyle style)
		{
			return DoWindow(id, clientRect, func, GUIContent.Temp(text), style, forceRectOnLayout: true);
		}

		public static Rect Window(int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style)
		{
			return DoWindow(id, clientRect, func, GUIContent.Temp(image), style, forceRectOnLayout: true);
		}

		public static Rect Window(int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style)
		{
			return DoWindow(id, clientRect, func, title, style, forceRectOnLayout: true);
		}

		internal static Rect DoWindow(int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style, bool forceRectOnLayout)
		{
			GUIUtility.CheckOnGUI();
			_Window window = (_Window)_WindowList.instance.windows[id];
			if (window == null)
			{
				window = new _Window(id);
				_WindowList.instance.windows[id] = window;
				s_LayersChanged = true;
			}
			if (!window.moved)
			{
				window.rect = clientRect;
			}
			window.moved = false;
			window.opacity = 1f;
			window.style = style;
			window.title.text = title.text;
			window.title.image = title.image;
			window.title.tooltip = title.tooltip;
			window.func = func;
			window.used = true;
			window.enabled = enabled;
			window.color = color;
			window.backgroundColor = backgroundColor;
			window.matrix = matrix;
			window.skin = skin;
			window.contentColor = contentColor;
			window.forceRect = forceRectOnLayout;
			return window.rect;
		}

		public static void DragWindow(Rect position)
		{
			GUIUtility.CheckOnGUI();
			if (_Window.current == null)
			{
				Debug.LogError("Dragwindow can only be called within a window callback");
				return;
			}
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			_Window current = _Window.current;
			Event current2 = Event.current;
			if (current == null)
			{
				return;
			}
			WindowDragState windowDragState = (WindowDragState)GUIUtility.GetStateObject(typeof(WindowDragState), controlID + 100);
			switch (Event.current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:
				if (position.Contains(current2.mousePosition) && GUIUtility.hotControl == 0)
				{
					GUIUtility.hotControl = controlID;
					Event.current.Use();
					Matrix4x4 matrix4x = _Window.current.matrix;
					windowDragState.dragStartPos = GUIClip.GetAbsoluteMousePosition() - (Vector2)matrix4x.MultiplyPoint(new Vector2(current.rect.x, current.rect.y));
					windowDragState.dragStartRect = current.rect;
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					Vector2 vector = _Window.current.matrix.inverse.MultiplyPoint(GUIClip.GetAbsoluteMousePosition() - windowDragState.dragStartPos);
					current.rect = new Rect(vector.x, vector.y, windowDragState.dragStartRect.width, windowDragState.dragStartRect.height);
					current.moved = true;
					Event.current.Use();
				}
				break;
			case EventType.MouseMove:
				break;
			}
		}

		public static void DragWindow()
		{
			DragWindow(new Rect(0f, 0f, 10000f, 10000f));
		}

		public static void BringWindowToFront(int windowID)
		{
			GUIUtility.CheckOnGUI();
			_Window window = _WindowList.instance.Get(windowID);
			if (window == null)
			{
				return;
			}
			int num = 0;
			foreach (_Window value in _WindowList.instance.windows.Values)
			{
				if (value.depth < num)
				{
					num = value.depth;
				}
			}
			window.depth = num - 1;
			s_LayersChanged = true;
		}

		public static void BringWindowToBack(int windowID)
		{
			GUIUtility.CheckOnGUI();
			_Window window = _WindowList.instance.Get(windowID);
			if (window == null)
			{
				return;
			}
			int num = 0;
			foreach (_Window value in _WindowList.instance.windows.Values)
			{
				if (value.depth > num)
				{
					num = value.depth;
				}
			}
			window.depth = num + 1;
			s_LayersChanged = true;
		}

		public static void FocusWindow(int windowID)
		{
			GUIUtility.CheckOnGUI();
			focusedWindow = windowID;
		}

		public static void UnfocusWindow()
		{
			GUIUtility.CheckOnGUI();
			focusedWindow = -1;
		}

		private static _Window FindWindowUnderMouse()
		{
			Event current = Event.current;
			foreach (_Window sortedWindow in GetSortedWindowList())
			{
				matrix = sortedWindow.matrix;
				if (sortedWindow.rect.Contains(current.mousePosition))
				{
					return sortedWindow;
				}
			}
			return null;
		}

		[NotRenamed]
		internal static void BeginWindows(int skinMode, IDList idlist, int editorWindowInstanceID)
		{
			GUILayoutGroup topLevel = GUILayoutUtility.current.topLevel;
			Stack layoutGroups = GUILayoutUtility.current.layoutGroups;
			GUILayoutGroup windows = GUILayoutUtility.current.windows;
			if (editorWindowInstanceID == 0)
			{
				if (s_GameWindowList == null)
				{
					s_GameWindowList = new _WindowList();
				}
				_WindowList.instance = s_GameWindowList;
			}
			else
			{
				if (_WindowList.s_EditorWindows == null)
				{
					_WindowList.s_EditorWindows = new Hashtable();
				}
				_WindowList.instance = (_WindowList)_WindowList.s_EditorWindows[editorWindowInstanceID];
				if (_WindowList.instance == null)
				{
					_WindowList.s_EditorWindows[editorWindowInstanceID] = (_WindowList.instance = new _WindowList());
				}
			}
			_Window window = null;
			Matrix4x4 matrix4x = matrix;
			Event current = Event.current;
			switch (current.type)
			{
			case EventType.Layout:
				foreach (_Window value in _WindowList.instance.windows.Values)
				{
					value.used = false;
				}
				break;
			case EventType.DragUpdated:
			case EventType.DragPerform:
			case EventType.DragExited:
				window = FindWindowUnderMouse();
				break;
			case EventType.MouseUp:
			case EventType.MouseDrag:
				window = ((GUIUtility.hotControl != 0) ? ((_Window)_WindowList.instance.windows[focusedWindow]) : FindWindowUnderMouse());
				break;
			case EventType.MouseDown:
			{
				focusedWindow = -1;
				bool flag = false;
				foreach (_Window sortedWindow in GetSortedWindowList())
				{
					matrix = sortedWindow.matrix;
					if (sortedWindow.rect.Contains(current.mousePosition))
					{
						focusedWindow = sortedWindow.id;
						window = sortedWindow;
						((_Window)_WindowList.instance.windows[sortedWindow.id]).depth = -1;
						flag = true;
						break;
					}
				}
				if (!flag && !s_LayersChanged)
				{
					break;
				}
				int num = 0;
				foreach (_Window sortedWindow2 in GetSortedWindowList())
				{
					sortedWindow2.depth = num;
					num++;
				}
				s_LayersChanged = false;
				break;
			}
			case EventType.Repaint:
				return;
			default:
				window = (_Window)_WindowList.instance.windows[focusedWindow];
				break;
			}
			if (window != null)
			{
				window.SetupGUIValues();
				IDList s_CurrentList = GUIUtility.s_CurrentList;
				GUIUtility.SelectIDList(idlist, window.id, isWindow: true, Event.current.type == EventType.Layout, resetIDListCursor: true);
				window.Do();
				GUIUtility.SelectIDList(s_CurrentList, 0, isWindow: false, Event.current.type == EventType.Layout, resetIDListCursor: false);
				GUIUtility.SetDidGUIWindowsEatLastEvent(value: true);
			}
			matrix = matrix4x;
			GUILayoutUtility.current.topLevel = topLevel;
			GUILayoutUtility.current.layoutGroups = layoutGroups;
			GUILayoutUtility.current.windows = windows;
		}

		internal static void EndWindows(IDList idlist)
		{
			GUILayoutGroup topLevel = GUILayoutUtility.current.topLevel;
			Stack layoutGroups = GUILayoutUtility.current.layoutGroups;
			GUILayoutGroup windows = GUILayoutUtility.current.windows;
			Event current = Event.current;
			ResetSettings();
			switch (current.type)
			{
			case EventType.Layout:
			{
				Hashtable hashtable = new Hashtable();
				IDList s_CurrentList = GUIUtility.s_CurrentList;
				foreach (_Window value in _WindowList.instance.windows.Values)
				{
					if (value.used)
					{
						if (value.forceRect)
						{
							GUILayoutOption[] options = new GUILayoutOption[2]
							{
								GUILayout.Width(value.rect.width),
								GUILayout.Height(value.rect.height)
							};
							GUILayoutUtility.BeginWindow(value.id, value.style, options);
						}
						else
						{
							GUILayoutUtility.BeginWindow(value.id, value.style, null);
						}
						value.SetupGUIValues();
						GUIUtility.SelectIDList(idlist, value.id, isWindow: true, Event.current.type == EventType.Layout, resetIDListCursor: true);
						value.Do();
						GUILayoutUtility.Layout();
						hashtable[value.id] = value;
					}
				}
				GUIUtility.SelectIDList(s_CurrentList, 0, isWindow: false, Event.current.type == EventType.Layout, resetIDListCursor: false);
				_WindowList.instance.windows = hashtable;
				break;
			}
			case EventType.Repaint:
			{
				ArrayList sortedWindowList = GetSortedWindowList();
				sortedWindowList.Reverse();
				IDList s_CurrentList = GUIUtility.s_CurrentList;
				foreach (_Window item in sortedWindowList)
				{
					item.SetupGUIValues();
					if (item.style != GUIStyle.none)
					{
						item.style.Draw(item.rect, item.title, item.rect.Contains(Event.current.mousePosition), isActive: false, focusedWindow == item.id, hasKeyboardFocus: false);
					}
					if (item.rect.Contains(Event.current.mousePosition))
					{
						GUIUtility.mouseUsed = true;
					}
					GUIUtility.SelectIDList(idlist, item.id, isWindow: true, Event.current.type == EventType.Layout, resetIDListCursor: true);
					item.Do();
				}
				GUIUtility.SelectIDList(s_CurrentList, 0, isWindow: false, Event.current.type == EventType.Layout, resetIDListCursor: false);
				break;
			}
			}
			GUILayoutUtility.current.topLevel = topLevel;
			GUILayoutUtility.current.layoutGroups = layoutGroups;
			GUILayoutUtility.current.windows = windows;
		}

		internal static void DoEndWindows(IDList idlist)
		{
			EndWindows(idlist);
		}
	}
}
