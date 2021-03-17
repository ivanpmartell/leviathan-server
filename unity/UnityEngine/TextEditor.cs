using System.Collections;

namespace UnityEngine
{
	public class TextEditor : IKeyboardControl
	{
		public enum DblClickSnapping : byte
		{
			WORDS,
			PARAGRAPHS
		}

		private enum CharacterType
		{
			LetterLike,
			Symbol,
			Symbol2,
			WhiteSpace
		}

		private enum TextEditOp
		{
			MoveLeft,
			MoveRight,
			MoveUp,
			MoveDown,
			MoveLineStart,
			MoveLineEnd,
			MoveTextStart,
			MoveTextEnd,
			MovePageUp,
			MovePageDown,
			MoveGraphicalLineStart,
			MoveGraphicalLineEnd,
			MoveWordLeft,
			MoveWordRight,
			MoveParagraphForward,
			MoveParagraphBackward,
			MoveToStartOfNextWord,
			MoveToEndOfPreviousWord,
			SelectLeft,
			SelectRight,
			SelectUp,
			SelectDown,
			SelectTextStart,
			SelectTextEnd,
			SelectPageUp,
			SelectPageDown,
			ExpandSelectGraphicalLineStart,
			ExpandSelectGraphicalLineEnd,
			SelectGraphicalLineStart,
			SelectGraphicalLineEnd,
			SelectWordLeft,
			SelectWordRight,
			SelectToEndOfPreviousWord,
			SelectToStartOfNextWord,
			SelectParagraphBackward,
			SelectParagraphForward,
			Delete,
			Backspace,
			DeleteWordBack,
			DeleteWordForward,
			Cut,
			Copy,
			Paste,
			SelectAll,
			SelectNone,
			ScrollStart,
			ScrollEnd,
			ScrollPageUp,
			ScrollPageDown
		}

		public int pos;

		public int selectPos;

		public int controlID;

		public GUIContent content = new GUIContent();

		public GUIStyle style = GUIStyle.none;

		public Rect position;

		public bool multiline;

		public bool hasHorizontalCursorPos;

		public bool isPasswordField;

		public Vector2 scrollOffset = Vector2.zero;

		public Vector2 graphicalCursorPos;

		public Vector2 graphicalSelectCursorPos;

		private bool m_MouseDragSelectsWholeWords;

		private int m_DblClickInitPos;

		private DblClickSnapping m_DblClickSnap;

		private bool m_bJustSelected;

		private int m_iAltCursorPos = -1;

		private string oldText;

		private int oldPos;

		private int oldSelectPos;

		private static Hashtable s_Keyactions;

		public bool hasSelection => pos != selectPos;

		public string SelectedText
		{
			get
			{
				int length = content.text.Length;
				if (pos > length)
				{
					pos = length;
				}
				if (selectPos > length)
				{
					selectPos = length;
				}
				if (pos == selectPos)
				{
					return string.Empty;
				}
				if (pos < selectPos)
				{
					return content.text.Substring(pos, selectPos - pos);
				}
				return content.text.Substring(selectPos, pos - selectPos);
			}
		}

		private void ClearCursorPos()
		{
			hasHorizontalCursorPos = false;
			m_iAltCursorPos = -1;
		}

		public void OnFocus()
		{
			if (multiline)
			{
				pos = (selectPos = 0);
			}
			else
			{
				SelectAll();
			}
		}

		public virtual void OnLostFocus()
		{
			scrollOffset = Vector2.zero;
		}

		private void GrabGraphicalCursorPos()
		{
			if (!hasHorizontalCursorPos)
			{
				graphicalCursorPos = style.GetCursorPixelPosition(position, content, pos);
				graphicalSelectCursorPos = style.GetCursorPixelPosition(position, content, selectPos);
				hasHorizontalCursorPos = false;
			}
		}

		public bool HandleKeyEvent(Event e)
		{
			InitKeyActions();
			EventModifiers modifiers = e.modifiers;
			e.modifiers &= ~EventModifiers.CapsLock;
			if (s_Keyactions.Contains(e))
			{
				TextEditOp operation = (TextEditOp)(int)s_Keyactions[e];
				PerformOperation(operation);
				e.modifiers = modifiers;
				return true;
			}
			e.modifiers = modifiers;
			return false;
		}

		public bool DeleteWordBack()
		{
			if (hasSelection)
			{
				DeleteSelection();
				return true;
			}
			int num = FindEndOfPreviousWord(pos);
			if (pos != num)
			{
				content.text = content.text.Remove(num, pos - num);
				selectPos = (pos = num);
				return true;
			}
			return false;
		}

		public bool DeleteWordForward()
		{
			if (hasSelection)
			{
				DeleteSelection();
				return true;
			}
			int num = FindStartOfNextWord(pos);
			if (pos < content.text.Length)
			{
				content.text = content.text.Remove(pos, num - pos);
				return true;
			}
			return false;
		}

		public bool Delete()
		{
			if (hasSelection)
			{
				DeleteSelection();
				return true;
			}
			if (pos < content.text.Length)
			{
				content.text = content.text.Remove(pos, 1);
				return true;
			}
			return false;
		}

		public bool CanPaste()
		{
			return GUIUtility.systemCopyBuffer.Length != 0;
		}

		public bool Backspace()
		{
			if (hasSelection)
			{
				DeleteSelection();
				return true;
			}
			if (pos > 0)
			{
				content.text = content.text.Remove(pos - 1, 1);
				selectPos = --pos;
				ClearCursorPos();
				return true;
			}
			return false;
		}

		public void SelectAll()
		{
			pos = 0;
			selectPos = content.text.Length;
			ClearCursorPos();
		}

		public void SelectNone()
		{
			selectPos = pos;
			ClearCursorPos();
		}

		public bool DeleteSelection()
		{
			int length = content.text.Length;
			if (pos > length)
			{
				pos = length;
			}
			if (selectPos > length)
			{
				selectPos = length;
			}
			if (pos == selectPos)
			{
				return false;
			}
			if (pos < selectPos)
			{
				content.text = content.text.Substring(0, pos) + content.text.Substring(selectPos, content.text.Length - selectPos);
				selectPos = pos;
			}
			else
			{
				content.text = content.text.Substring(0, selectPos) + content.text.Substring(pos, content.text.Length - pos);
				pos = selectPos;
			}
			ClearCursorPos();
			return true;
		}

		public void ReplaceSelection(string replace)
		{
			DeleteSelection();
			content.text = content.text.Insert(pos, replace);
			selectPos = (pos += replace.Length);
			ClearCursorPos();
		}

		public void Insert(char c)
		{
			ReplaceSelection(c.ToString());
		}

		public void MoveSelectionToAltCursor()
		{
			if (m_iAltCursorPos != -1)
			{
				int iAltCursorPos = m_iAltCursorPos;
				string selectedText = SelectedText;
				content.text = content.text.Insert(iAltCursorPos, selectedText);
				if (iAltCursorPos < pos)
				{
					pos += selectedText.Length;
					selectPos += selectedText.Length;
				}
				DeleteSelection();
				selectPos = (pos = iAltCursorPos);
				ClearCursorPos();
			}
		}

		public void MoveRight()
		{
			ClearCursorPos();
			if (selectPos == pos)
			{
				pos++;
				ClampPos();
				selectPos = pos;
			}
			else if (selectPos > pos)
			{
				pos = selectPos;
			}
			else
			{
				selectPos = pos;
			}
		}

		public void MoveLeft()
		{
			if (selectPos == pos)
			{
				pos--;
				if (pos < 0)
				{
					pos = 0;
				}
				selectPos = pos;
			}
			else if (selectPos > pos)
			{
				selectPos = pos;
			}
			else
			{
				pos = selectPos;
			}
			ClearCursorPos();
		}

		public void MoveUp()
		{
			if (selectPos < pos)
			{
				selectPos = pos;
			}
			else
			{
				pos = selectPos;
			}
			GrabGraphicalCursorPos();
			graphicalCursorPos.y -= 1f;
			pos = (selectPos = style.GetCursorStringIndex(position, content, graphicalCursorPos));
			if (pos <= 0)
			{
				ClearCursorPos();
			}
		}

		public void MoveDown()
		{
			if (selectPos > pos)
			{
				selectPos = pos;
			}
			else
			{
				pos = selectPos;
			}
			GrabGraphicalCursorPos();
			graphicalCursorPos.y += style.lineHeight + 5f;
			pos = (selectPos = style.GetCursorStringIndex(position, content, graphicalCursorPos));
			if (pos == content.text.Length)
			{
				ClearCursorPos();
			}
		}

		public void MoveLineStart()
		{
			int num = ((selectPos >= pos) ? pos : selectPos);
			int num2 = num;
			while (num2-- != 0)
			{
				if (content.text[num2] == '\n')
				{
					selectPos = (pos = num2 + 1);
					return;
				}
			}
			selectPos = (pos = 0);
		}

		public void MoveLineEnd()
		{
			int num = ((selectPos <= pos) ? pos : selectPos);
			int i = num;
			int length;
			for (length = content.text.Length; i < length; i++)
			{
				if (content.text[i] == '\n')
				{
					selectPos = (pos = i);
					return;
				}
			}
			selectPos = (pos = length);
		}

		public void MoveGraphicalLineStart()
		{
			pos = (selectPos = GetGraphicalLineStart((pos >= selectPos) ? selectPos : pos));
		}

		public void MoveGraphicalLineEnd()
		{
			pos = (selectPos = GetGraphicalLineEnd((pos <= selectPos) ? selectPos : pos));
		}

		public void MoveTextStart()
		{
			selectPos = (pos = 0);
		}

		public void MoveTextEnd()
		{
			selectPos = (pos = content.text.Length);
		}

		public void MoveParagraphForward()
		{
			pos = ((pos <= selectPos) ? selectPos : pos);
			if (pos < content.text.Length)
			{
				selectPos = (pos = content.text.IndexOf('\n', pos + 1));
				if (pos == -1)
				{
					selectPos = (pos = content.text.Length);
				}
			}
		}

		public void MoveParagraphBackward()
		{
			pos = ((pos >= selectPos) ? selectPos : pos);
			if (pos > 1)
			{
				selectPos = (pos = content.text.LastIndexOf('\n', pos - 2) + 1);
			}
			else
			{
				selectPos = (pos = 0);
			}
		}

		public void MoveCursorToPosition(Vector2 cursorPosition)
		{
			selectPos = style.GetCursorStringIndex(position, content, cursorPosition + scrollOffset);
			if (!Event.current.shift)
			{
				pos = selectPos;
			}
			ClampPos();
		}

		public void MoveAltCursorToPosition(Vector2 cursorPosition)
		{
			m_iAltCursorPos = style.GetCursorStringIndex(position, content, cursorPosition + scrollOffset);
			ClampPos();
		}

		public bool IsOverSelection(Vector2 cursorPosition)
		{
			int cursorStringIndex = style.GetCursorStringIndex(position, content, cursorPosition + scrollOffset);
			return cursorStringIndex < Mathf.Max(pos, selectPos) && cursorStringIndex > Mathf.Min(pos, selectPos);
		}

		public void SelectToPosition(Vector2 cursorPosition)
		{
			if (!m_MouseDragSelectsWholeWords)
			{
				pos = style.GetCursorStringIndex(position, content, cursorPosition + scrollOffset);
				return;
			}
			int num = style.GetCursorStringIndex(position, content, cursorPosition + scrollOffset);
			if (m_DblClickSnap == DblClickSnapping.WORDS)
			{
				if (num < m_DblClickInitPos)
				{
					pos = FindEndOfClassification(num, -1);
					selectPos = FindEndOfClassification(m_DblClickInitPos, 1);
					return;
				}
				if (num >= content.text.Length)
				{
					num = content.text.Length - 1;
				}
				pos = FindEndOfClassification(num, 1);
				selectPos = FindEndOfClassification(m_DblClickInitPos - 1, -1);
				return;
			}
			if (num < m_DblClickInitPos)
			{
				if (num > 0)
				{
					pos = content.text.LastIndexOf('\n', num - 2) + 1;
				}
				else
				{
					pos = 0;
				}
				selectPos = content.text.LastIndexOf('\n', m_DblClickInitPos);
				return;
			}
			if (num < content.text.Length)
			{
				pos = content.text.IndexOf('\n', num + 1) + 1;
				if (pos <= 0)
				{
					pos = content.text.Length;
				}
			}
			else
			{
				pos = content.text.Length;
			}
			selectPos = content.text.LastIndexOf('\n', m_DblClickInitPos - 2) + 1;
		}

		public void SelectLeft()
		{
			if (m_bJustSelected && pos > selectPos)
			{
				int num = pos;
				pos = selectPos;
				selectPos = num;
			}
			m_bJustSelected = false;
			pos--;
			if (pos < 0)
			{
				pos = 0;
			}
		}

		public void SelectRight()
		{
			if (m_bJustSelected && pos < selectPos)
			{
				int num = pos;
				pos = selectPos;
				selectPos = num;
			}
			m_bJustSelected = false;
			pos++;
			int length = content.text.Length;
			if (pos > length)
			{
				pos = length;
			}
		}

		public void SelectUp()
		{
			GrabGraphicalCursorPos();
			graphicalCursorPos.y -= 1f;
			pos = style.GetCursorStringIndex(position, content, graphicalCursorPos);
		}

		public void SelectDown()
		{
			GrabGraphicalCursorPos();
			graphicalCursorPos.y += style.lineHeight + 5f;
			pos = style.GetCursorStringIndex(position, content, graphicalCursorPos);
		}

		public void SelectTextEnd()
		{
			pos = content.text.Length;
		}

		public void SelectTextStart()
		{
			pos = 0;
		}

		public void MouseDragSelectsWholeWords(bool on)
		{
			m_MouseDragSelectsWholeWords = on;
			m_DblClickInitPos = pos;
		}

		public void DblClickSnap(DblClickSnapping snapping)
		{
			m_DblClickSnap = snapping;
		}

		private int GetGraphicalLineStart(int p)
		{
			Vector2 cursorPixelPosition = style.GetCursorPixelPosition(position, content, p);
			cursorPixelPosition.x = 0f;
			return style.GetCursorStringIndex(position, content, cursorPixelPosition);
		}

		private int GetGraphicalLineEnd(int p)
		{
			Vector2 cursorPixelPosition = style.GetCursorPixelPosition(position, content, p);
			cursorPixelPosition.x += 5000f;
			return style.GetCursorStringIndex(position, content, cursorPixelPosition);
		}

		private int FindNextSeperator(int startPos)
		{
			int length = content.text.Length;
			while (startPos < length && !isLetterLikeChar(content.text[startPos]))
			{
				startPos++;
			}
			while (startPos < length && isLetterLikeChar(content.text[startPos]))
			{
				startPos++;
			}
			return startPos;
		}

		private static bool isLetterLikeChar(char c)
		{
			return char.IsLetterOrDigit(c) || c == '\'';
		}

		private int FindPrevSeperator(int startPos)
		{
			startPos--;
			while (startPos > 0 && !isLetterLikeChar(content.text[startPos]))
			{
				startPos--;
			}
			while (startPos >= 0 && isLetterLikeChar(content.text[startPos]))
			{
				startPos--;
			}
			return startPos + 1;
		}

		public void MoveWordRight()
		{
			pos = ((pos <= selectPos) ? selectPos : pos);
			pos = (selectPos = FindNextSeperator(pos));
			ClearCursorPos();
		}

		public void MoveToStartOfNextWord()
		{
			ClearCursorPos();
			if (pos != selectPos)
			{
				MoveRight();
			}
			else
			{
				pos = (selectPos = FindStartOfNextWord(pos));
			}
		}

		public void MoveToEndOfPreviousWord()
		{
			ClearCursorPos();
			if (pos != selectPos)
			{
				MoveLeft();
			}
			else
			{
				pos = (selectPos = FindEndOfPreviousWord(pos));
			}
		}

		public void SelectToStartOfNextWord()
		{
			ClearCursorPos();
			pos = FindStartOfNextWord(pos);
		}

		public void SelectToEndOfPreviousWord()
		{
			ClearCursorPos();
			pos = FindEndOfPreviousWord(pos);
		}

		private CharacterType ClassifyChar(char c)
		{
			if (char.IsWhiteSpace(c))
			{
				return CharacterType.WhiteSpace;
			}
			if (char.IsLetterOrDigit(c) || c == '\'')
			{
				return CharacterType.LetterLike;
			}
			return CharacterType.Symbol;
		}

		public int FindStartOfNextWord(int p)
		{
			int length = content.text.Length;
			if (p == length)
			{
				return p;
			}
			char c = content.text[p];
			CharacterType characterType = ClassifyChar(c);
			if (characterType != CharacterType.WhiteSpace)
			{
				p++;
				while (p < length && ClassifyChar(content.text[p]) == characterType)
				{
					p++;
				}
			}
			else if (c == '\t' || c == '\n')
			{
				return p + 1;
			}
			if (p == length)
			{
				return p;
			}
			c = content.text[p];
			if (c == ' ')
			{
				while (p < length && char.IsWhiteSpace(content.text[p]))
				{
					p++;
				}
			}
			else if (c == '\t' || c == '\n')
			{
				return p;
			}
			return p;
		}

		private int FindEndOfPreviousWord(int p)
		{
			if (p == 0)
			{
				return p;
			}
			p--;
			while (p > 0 && content.text[p] == ' ')
			{
				p--;
			}
			CharacterType characterType = ClassifyChar(content.text[p]);
			if (characterType != CharacterType.WhiteSpace)
			{
				while (p > 0 && ClassifyChar(content.text[p - 1]) == characterType)
				{
					p--;
				}
			}
			return p;
		}

		public void MoveWordLeft()
		{
			pos = ((pos >= selectPos) ? selectPos : pos);
			pos = FindPrevSeperator(pos);
			selectPos = pos;
		}

		public void SelectWordRight()
		{
			ClearCursorPos();
			int num = selectPos;
			if (pos < selectPos)
			{
				selectPos = pos;
				MoveWordRight();
				selectPos = num;
				pos = ((pos >= selectPos) ? selectPos : pos);
			}
			else
			{
				selectPos = pos;
				MoveWordRight();
				selectPos = num;
			}
		}

		public void SelectWordLeft()
		{
			ClearCursorPos();
			int num = selectPos;
			if (pos > selectPos)
			{
				selectPos = pos;
				MoveWordLeft();
				selectPos = num;
				pos = ((pos <= selectPos) ? selectPos : pos);
			}
			else
			{
				selectPos = pos;
				MoveWordLeft();
				selectPos = num;
			}
		}

		public void ExpandSelectGraphicalLineStart()
		{
			ClearCursorPos();
			if (pos < selectPos)
			{
				pos = GetGraphicalLineStart(pos);
				return;
			}
			int num = pos;
			pos = GetGraphicalLineStart(selectPos);
			selectPos = num;
		}

		public void ExpandSelectGraphicalLineEnd()
		{
			ClearCursorPos();
			if (pos > selectPos)
			{
				pos = GetGraphicalLineEnd(pos);
				return;
			}
			int num = pos;
			pos = GetGraphicalLineEnd(selectPos);
			selectPos = num;
		}

		public void SelectGraphicalLineStart()
		{
			ClearCursorPos();
			pos = GetGraphicalLineStart(pos);
		}

		public void SelectGraphicalLineEnd()
		{
			ClearCursorPos();
			pos = GetGraphicalLineEnd(pos);
		}

		public void SelectParagraphForward()
		{
			ClearCursorPos();
			bool flag = pos < selectPos;
			if (pos < content.text.Length)
			{
				pos = content.text.IndexOf('\n', pos + 1);
				if (pos == -1)
				{
					pos = content.text.Length;
				}
				if (flag && pos > selectPos)
				{
					pos = selectPos;
				}
			}
		}

		public void SelectParagraphBackward()
		{
			ClearCursorPos();
			bool flag = pos > selectPos;
			if (pos > 1)
			{
				pos = content.text.LastIndexOf('\n', pos - 2) + 1;
				if (flag && pos < selectPos)
				{
					pos = selectPos;
				}
			}
			else
			{
				selectPos = (pos = 0);
			}
		}

		public void SelectCurrentWord()
		{
			ClearCursorPos();
			int length = content.text.Length;
			selectPos = pos;
			if (length != 0)
			{
				if (pos >= length)
				{
					pos = length - 1;
				}
				if (selectPos >= length)
				{
					selectPos--;
				}
				if (pos < selectPos)
				{
					pos = FindEndOfClassification(pos, -1);
					selectPos = FindEndOfClassification(selectPos, 1);
				}
				else
				{
					pos = FindEndOfClassification(pos, 1);
					selectPos = FindEndOfClassification(selectPos, -1);
				}
				m_bJustSelected = true;
			}
		}

		private int FindEndOfClassification(int p, int dir)
		{
			int length = content.text.Length;
			if (p >= length || p < 0)
			{
				return p;
			}
			CharacterType characterType = ClassifyChar(content.text[p]);
			do
			{
				p += dir;
				if (p < 0)
				{
					return 0;
				}
				if (p >= length)
				{
					return length;
				}
			}
			while (ClassifyChar(content.text[p]) == characterType);
			if (dir == 1)
			{
				return p;
			}
			return p + 1;
		}

		public void SelectCurrentParagraph()
		{
			ClearCursorPos();
			int length = content.text.Length;
			if (pos < length)
			{
				pos = content.text.IndexOf('\n', pos);
				if (pos == -1)
				{
					pos = content.text.Length;
				}
				else
				{
					pos++;
				}
			}
			if (selectPos != 0)
			{
				selectPos = content.text.LastIndexOf('\n', selectPos - 1) + 1;
			}
		}

		public void DrawCursor(string text)
		{
			string text2 = content.text;
			if (Input.compositionString.Length > 0)
			{
				content.text = text.Substring(0, pos) + Input.compositionString + text.Substring(selectPos);
			}
			else
			{
				content.text = text;
			}
			graphicalCursorPos = style.GetCursorPixelPosition(new Rect(0f, 0f, position.width, position.height), content, pos);
			Input.compositionCursorPos = graphicalCursorPos + new Vector2(position.x, position.y + style.lineHeight);
			Rect rect = style.padding.Remove(position);
			Vector2 vector = style.CalcSize(content);
			if (vector.x < rect.width)
			{
				scrollOffset.x = 0f;
			}
			else
			{
				if (graphicalCursorPos.x + 1f > scrollOffset.x + rect.width)
				{
					scrollOffset.x = graphicalCursorPos.x - rect.width;
				}
				if (graphicalCursorPos.x < scrollOffset.x + (float)style.padding.left)
				{
					scrollOffset.x = graphicalCursorPos.x - (float)style.padding.left;
				}
			}
			if (vector.y < rect.height)
			{
				scrollOffset.y = 0f;
			}
			else
			{
				if (graphicalCursorPos.y + style.lineHeight > scrollOffset.y + rect.height + (float)style.padding.top)
				{
					scrollOffset.y = graphicalCursorPos.y - rect.height - (float)style.padding.top + style.lineHeight;
				}
				if (graphicalCursorPos.y < scrollOffset.y + (float)style.padding.top)
				{
					scrollOffset.y = graphicalCursorPos.y - (float)style.padding.top;
				}
			}
			if (scrollOffset.y > 0f && vector.y - scrollOffset.y < rect.height)
			{
				scrollOffset.y = vector.y - rect.height - (float)style.padding.top - (float)style.padding.bottom;
			}
			scrollOffset.y = ((!(scrollOffset.y < 0f)) ? scrollOffset.y : 0f);
			style.contentOffset = -scrollOffset;
			style.Internal_clipOffset = scrollOffset;
			if (Input.compositionString.Length > 0)
			{
				style.DrawWithTextSelection(position, content, controlID, pos, pos + Input.compositionString.Length, drawSelectionAsComposition: true);
			}
			else
			{
				style.DrawWithTextSelection(position, content, controlID, pos, selectPos);
			}
			if (m_iAltCursorPos != -1)
			{
				style.DrawCursor(position, content, controlID, m_iAltCursorPos);
			}
			style.contentOffset = Vector2.zero;
			style.Internal_clipOffset = Vector2.zero;
			content.text = text2;
		}

		private bool PerformOperation(TextEditOp operation)
		{
			switch (operation)
			{
			case TextEditOp.MoveLeft:
				MoveLeft();
				break;
			case TextEditOp.MoveRight:
				MoveRight();
				break;
			case TextEditOp.MoveUp:
				MoveUp();
				break;
			case TextEditOp.MoveDown:
				MoveDown();
				break;
			case TextEditOp.MoveLineStart:
				MoveLineStart();
				break;
			case TextEditOp.MoveLineEnd:
				MoveLineEnd();
				break;
			case TextEditOp.MoveWordRight:
				MoveWordRight();
				break;
			case TextEditOp.MoveToStartOfNextWord:
				MoveToStartOfNextWord();
				break;
			case TextEditOp.MoveToEndOfPreviousWord:
				MoveToEndOfPreviousWord();
				break;
			case TextEditOp.MoveWordLeft:
				MoveWordLeft();
				break;
			case TextEditOp.MoveTextStart:
				MoveTextStart();
				break;
			case TextEditOp.MoveTextEnd:
				MoveTextEnd();
				break;
			case TextEditOp.MoveParagraphForward:
				MoveParagraphForward();
				break;
			case TextEditOp.MoveParagraphBackward:
				MoveParagraphBackward();
				break;
			case TextEditOp.MoveGraphicalLineStart:
				MoveGraphicalLineStart();
				break;
			case TextEditOp.MoveGraphicalLineEnd:
				MoveGraphicalLineEnd();
				break;
			case TextEditOp.SelectLeft:
				SelectLeft();
				break;
			case TextEditOp.SelectRight:
				SelectRight();
				break;
			case TextEditOp.SelectUp:
				SelectUp();
				break;
			case TextEditOp.SelectDown:
				SelectDown();
				break;
			case TextEditOp.SelectWordRight:
				SelectWordRight();
				break;
			case TextEditOp.SelectWordLeft:
				SelectWordLeft();
				break;
			case TextEditOp.SelectToEndOfPreviousWord:
				SelectToEndOfPreviousWord();
				break;
			case TextEditOp.SelectToStartOfNextWord:
				SelectToStartOfNextWord();
				break;
			case TextEditOp.SelectTextStart:
				SelectTextStart();
				break;
			case TextEditOp.SelectTextEnd:
				SelectTextEnd();
				break;
			case TextEditOp.ExpandSelectGraphicalLineStart:
				ExpandSelectGraphicalLineStart();
				break;
			case TextEditOp.ExpandSelectGraphicalLineEnd:
				ExpandSelectGraphicalLineEnd();
				break;
			case TextEditOp.SelectParagraphForward:
				SelectParagraphForward();
				break;
			case TextEditOp.SelectParagraphBackward:
				SelectParagraphBackward();
				break;
			case TextEditOp.SelectGraphicalLineStart:
				SelectGraphicalLineStart();
				break;
			case TextEditOp.SelectGraphicalLineEnd:
				SelectGraphicalLineEnd();
				break;
			case TextEditOp.Delete:
				return Delete();
			case TextEditOp.Backspace:
				return Backspace();
			case TextEditOp.Cut:
				return Cut();
			case TextEditOp.Copy:
				Copy();
				break;
			case TextEditOp.Paste:
				return Paste();
			case TextEditOp.SelectAll:
				SelectAll();
				break;
			case TextEditOp.SelectNone:
				SelectNone();
				break;
			case TextEditOp.DeleteWordBack:
				return DeleteWordBack();
			case TextEditOp.DeleteWordForward:
				return DeleteWordForward();
			default:
				Debug.Log("Unimplemented: " + operation);
				break;
			}
			return false;
		}

		public void SaveBackup()
		{
			oldText = content.text;
			oldPos = pos;
			oldSelectPos = selectPos;
		}

		public void Undo()
		{
			content.text = oldText;
			pos = oldPos;
			selectPos = oldSelectPos;
		}

		public bool Cut()
		{
			if (isPasswordField)
			{
				return false;
			}
			Copy();
			return DeleteSelection();
		}

		public void Copy()
		{
			if (selectPos != pos && !isPasswordField)
			{
				string text2 = (GUIUtility.systemCopyBuffer = ((pos >= selectPos) ? content.text.Substring(selectPos, pos - selectPos) : content.text.Substring(pos, selectPos - pos)));
			}
		}

		public bool Paste()
		{
			string systemCopyBuffer = GUIUtility.systemCopyBuffer;
			if (systemCopyBuffer != string.Empty)
			{
				ReplaceSelection(systemCopyBuffer);
				return true;
			}
			return false;
		}

		private static void MapKey(string key, TextEditOp action)
		{
			s_Keyactions[Event.KeyboardEvent(key)] = action;
		}

		private void InitKeyActions()
		{
			if (s_Keyactions == null)
			{
				s_Keyactions = new Hashtable();
				MapKey("left", TextEditOp.MoveLeft);
				MapKey("right", TextEditOp.MoveRight);
				MapKey("up", TextEditOp.MoveUp);
				MapKey("down", TextEditOp.MoveDown);
				MapKey("#left", TextEditOp.SelectLeft);
				MapKey("#right", TextEditOp.SelectRight);
				MapKey("#up", TextEditOp.SelectUp);
				MapKey("#down", TextEditOp.SelectDown);
				MapKey("delete", TextEditOp.Delete);
				MapKey("backspace", TextEditOp.Backspace);
				MapKey("#backspace", TextEditOp.Backspace);
				if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsWebPlayer && Application.platform != RuntimePlatform.WindowsEditor)
				{
					MapKey("^left", TextEditOp.MoveGraphicalLineStart);
					MapKey("^right", TextEditOp.MoveGraphicalLineEnd);
					MapKey("&left", TextEditOp.MoveWordLeft);
					MapKey("&right", TextEditOp.MoveWordRight);
					MapKey("&up", TextEditOp.MoveParagraphBackward);
					MapKey("&down", TextEditOp.MoveParagraphForward);
					MapKey("%left", TextEditOp.MoveGraphicalLineStart);
					MapKey("%right", TextEditOp.MoveGraphicalLineEnd);
					MapKey("%up", TextEditOp.MoveTextStart);
					MapKey("%down", TextEditOp.MoveTextEnd);
					MapKey("#home", TextEditOp.SelectTextStart);
					MapKey("#end", TextEditOp.SelectTextEnd);
					MapKey("#^left", TextEditOp.ExpandSelectGraphicalLineStart);
					MapKey("#^right", TextEditOp.ExpandSelectGraphicalLineEnd);
					MapKey("#^up", TextEditOp.SelectParagraphBackward);
					MapKey("#^down", TextEditOp.SelectParagraphForward);
					MapKey("#&left", TextEditOp.SelectWordLeft);
					MapKey("#&right", TextEditOp.SelectWordRight);
					MapKey("#&up", TextEditOp.SelectParagraphBackward);
					MapKey("#&down", TextEditOp.SelectParagraphForward);
					MapKey("#%left", TextEditOp.ExpandSelectGraphicalLineStart);
					MapKey("#%right", TextEditOp.ExpandSelectGraphicalLineEnd);
					MapKey("#%up", TextEditOp.SelectTextStart);
					MapKey("#%down", TextEditOp.SelectTextEnd);
					MapKey("%a", TextEditOp.SelectAll);
					MapKey("%x", TextEditOp.Cut);
					MapKey("%c", TextEditOp.Copy);
					MapKey("%v", TextEditOp.Paste);
					MapKey("^d", TextEditOp.Delete);
					MapKey("^h", TextEditOp.Backspace);
					MapKey("^b", TextEditOp.MoveLeft);
					MapKey("^f", TextEditOp.MoveRight);
					MapKey("^a", TextEditOp.MoveLineStart);
					MapKey("^e", TextEditOp.MoveLineEnd);
					MapKey("&delete", TextEditOp.DeleteWordForward);
					MapKey("&backspace", TextEditOp.DeleteWordBack);
				}
				else
				{
					MapKey("home", TextEditOp.MoveGraphicalLineStart);
					MapKey("end", TextEditOp.MoveGraphicalLineEnd);
					MapKey("%left", TextEditOp.MoveWordLeft);
					MapKey("%right", TextEditOp.MoveWordRight);
					MapKey("%up", TextEditOp.MoveParagraphBackward);
					MapKey("%down", TextEditOp.MoveParagraphForward);
					MapKey("^left", TextEditOp.MoveToEndOfPreviousWord);
					MapKey("^right", TextEditOp.MoveToStartOfNextWord);
					MapKey("^up", TextEditOp.MoveParagraphBackward);
					MapKey("^down", TextEditOp.MoveParagraphForward);
					MapKey("#^left", TextEditOp.SelectToEndOfPreviousWord);
					MapKey("#^right", TextEditOp.SelectToStartOfNextWord);
					MapKey("#^up", TextEditOp.SelectParagraphBackward);
					MapKey("#^down", TextEditOp.SelectParagraphForward);
					MapKey("#home", TextEditOp.SelectGraphicalLineStart);
					MapKey("#end", TextEditOp.SelectGraphicalLineEnd);
					MapKey("^delete", TextEditOp.DeleteWordForward);
					MapKey("^backspace", TextEditOp.DeleteWordBack);
					MapKey("^a", TextEditOp.SelectAll);
					MapKey("^x", TextEditOp.Cut);
					MapKey("^c", TextEditOp.Copy);
					MapKey("^v", TextEditOp.Paste);
					MapKey("#delete", TextEditOp.Cut);
					MapKey("^insert", TextEditOp.Copy);
					MapKey("#insert", TextEditOp.Paste);
				}
			}
		}

		public void ClampPos()
		{
			if (pos < 0)
			{
				pos = 0;
			}
			else if (pos > content.text.Length)
			{
				pos = content.text.Length;
			}
			if (selectPos < 0)
			{
				selectPos = 0;
			}
			else if (selectPos > content.text.Length)
			{
				selectPos = content.text.Length;
			}
			if (m_iAltCursorPos > content.text.Length)
			{
				m_iAltCursorPos = content.text.Length;
			}
		}
	}
}
