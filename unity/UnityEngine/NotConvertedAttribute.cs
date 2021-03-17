using System;
using System.Diagnostics;

namespace UnityEngine
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	[Conditional("UNITY_FLASH")]
	public sealed class NotConvertedAttribute : Attribute
	{
	}
}
