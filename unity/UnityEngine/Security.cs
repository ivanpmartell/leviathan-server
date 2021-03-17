using System;
using System.Reflection;
using System.Security;

namespace UnityEngine
{
	public sealed class Security
	{
		private static MethodInfo GetUnityCrossDomainHelperMethod(string methodname)
		{
			Type type = Types.GetType("UnityEngine.UnityCrossDomainHelper", "CrossDomainPolicyParser, Version=1.0.0.0, Culture=neutral");
			if (type == null)
			{
				throw new SecurityException("Cant find type UnityCrossDomainHelper");
			}
			MethodInfo method = type.GetMethod(methodname);
			if (method == null)
			{
				throw new SecurityException("Cant find " + methodname);
			}
			return method;
		}

		public static bool PrefetchSocketPolicy(string ip, int atPort)
		{
			int timeout = 3000;
			return PrefetchSocketPolicy(ip, atPort, timeout);
		}

		public static bool PrefetchSocketPolicy(string ip, int atPort, int timeout)
		{
			MethodInfo unityCrossDomainHelperMethod = GetUnityCrossDomainHelperMethod("PrefetchSocketPolicy");
			object obj = unityCrossDomainHelperMethod.Invoke(null, new object[3] { ip, atPort, timeout });
			return (bool)obj;
		}
	}
}
