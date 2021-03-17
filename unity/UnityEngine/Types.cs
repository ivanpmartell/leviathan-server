using System;
using System.Reflection;

namespace UnityEngine
{
	public static class Types
	{
		public static Type GetType(string typeName, string assemblyName)
		{
			//Discarded unreachable code: IL_0012, IL_001f
			try
			{
				return Assembly.Load(assemblyName).GetType(typeName);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
