public class DefaultSections
{
	public string m_front = string.Empty;

	public string m_mid = string.Empty;

	public string m_rear = string.Empty;

	public string m_top = string.Empty;

	public bool IsValid()
	{
		if (m_front.Length == 0)
		{
			return false;
		}
		if (m_mid.Length == 0)
		{
			return false;
		}
		if (m_rear.Length == 0)
		{
			return false;
		}
		if (m_top.Length == 0)
		{
			return false;
		}
		return true;
	}

	public string ErrorMessage()
	{
		string text = string.Empty;
		if (m_front.Length == 0)
		{
			text += " Front";
		}
		if (m_mid.Length == 0)
		{
			text += " Mid";
		}
		if (m_rear.Length == 0)
		{
			text += " Rear";
		}
		if (m_top.Length == 0)
		{
			text += " Top";
		}
		return text;
	}
}
