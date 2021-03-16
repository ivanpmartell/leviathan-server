#define DEBUG
using System;
using System.Diagnostics;

public class DebugUtils
{
	[Conditional("DEBUG")]
	public static void Assert(bool condition)
	{
		Assert(condition, string.Empty);
	}

	[Conditional("DEBUG")]
	public static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			throw new Exception(message);
		}
	}

	public static void PrintCallstack()
	{
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		StackFrame[] array = frames;
		foreach (StackFrame stackFrame in array)
		{
			PLog.Log(stackFrame.GetMethod().Name);
		}
	}
}
