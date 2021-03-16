using System;
using UnityEngine;

public class PLog
{
	public static void Log(object o)
	{
	}

	public static void LogError(object o)
	{
		Debug.LogError(string.Concat(DateTime.Now.ToString(), ": ", o, "\n"));
	}

	public static void LogWarning(object o)
	{
		Debug.LogWarning(string.Concat(DateTime.Now.ToString(), ": ", o, "\n"));
	}
}
