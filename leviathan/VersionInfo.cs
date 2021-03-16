internal struct VersionInfo
{
	public static string m_minorVersion = "1.2.11741";

	public static int m_majorVersion = 3;

	public static int m_alternativeMajorVersion = 3;

	public static string GetFullVersionString()
	{
		return m_minorVersion + "(" + m_majorVersion + ")";
	}

	public static string GetMajorVersionString()
	{
		return m_majorVersion.ToString();
	}

	public static bool VerifyVersion(string versionString)
	{
		if (versionString == GetMajorVersionString() || versionString == GetFullVersionString())
		{
			return true;
		}
		if (versionString == m_alternativeMajorVersion.ToString())
		{
			return true;
		}
		return false;
	}
}
