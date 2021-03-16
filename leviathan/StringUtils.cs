using System.Text.RegularExpressions;

public class StringUtils
{
	public static void TryRemoveCopyText(ref string fromString)
	{
		string text = ExtractCopyNumber(fromString, includeParantheses: true);
		if (!string.IsNullOrEmpty(text))
		{
			fromString = fromString.Replace(text, string.Empty);
		}
	}

	public static bool ContainsParanthesesAndNumber(string fromString)
	{
		return !string.IsNullOrEmpty(ExtractCopyNumber(fromString, includeParantheses: false));
	}

	public static string ExtractCopyNumber(string fromString, bool includeParantheses)
	{
		string text = fromString.Trim();
		int num = text.IndexOf('(');
		if (num < 0)
		{
			return string.Empty;
		}
		int num2 = text.LastIndexOf(')');
		if (num2 < 0)
		{
			return string.Empty;
		}
		int num3 = num2 - num - 1;
		if (num3 < 0)
		{
			return string.Empty;
		}
		string text2 = text.Substring(num + 1, num3);
		int result = 0;
		if (int.TryParse(text2, out result))
		{
			return ((!includeParantheses) ? string.Empty : "(") + text2 + ((!includeParantheses) ? string.Empty : ")");
		}
		return string.Empty;
	}

	public static void TryRemoveNonNumbers(ref string text)
	{
		text = Regex.Replace(text, "\\D", string.Empty);
	}

	public static void TryRemoveNumbers(ref string text)
	{
		text = Regex.Replace(text, "\\d", string.Empty);
	}
}
