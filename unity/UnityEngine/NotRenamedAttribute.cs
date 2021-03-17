using System;
using System.Diagnostics;

namespace UnityEngine
{
	[Conditional("UNITY_FLASH")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface)]
	public sealed class NotRenamedAttribute : Attribute
	{
	}
}
