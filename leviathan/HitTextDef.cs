using UnityEngine;

internal class HitTextDef
{
	public enum FontSize
	{
		Small,
		Medium,
		Large
	}

	public FontSize m_fontSize;

	public Color m_color;

	public string m_prefix = string.Empty;

	public string m_postfix = string.Empty;

	public HitTextDef(FontSize fontSize, Color color)
	{
		m_fontSize = fontSize;
		m_color = color;
	}

	public HitTextDef(FontSize fontSize, Color color, string prefix, string postfix)
	{
		m_fontSize = fontSize;
		m_color = color;
		m_prefix = prefix;
		m_postfix = postfix;
	}
}
