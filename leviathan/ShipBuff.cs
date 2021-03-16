#define DEBUG
public abstract class ShipBuff
{
	public string m_iconPath;

	public string m_name;

	public string m_description;

	public int m_cost;

	protected void AssertValidate()
	{
		DebugUtils.Assert(Validate());
	}

	protected virtual bool Validate()
	{
		return !string.IsNullOrEmpty(m_iconPath) && !string.IsNullOrEmpty(m_name) && !string.IsNullOrEmpty(m_description) && m_cost >= 1;
	}

	protected static string FloatVarToDescription(float f, bool isPercent, string varName)
	{
		string text = ((f < 0f) ? "-" : ((!(f > 0f)) ? string.Empty : "+"));
		return string.Format("{0}{1}{2} {3}", text, f.ToString("F2"), (!isPercent) ? string.Empty : "%", varName.Trim().ToUpper());
	}
}
