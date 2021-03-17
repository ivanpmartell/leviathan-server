using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Serialization;

namespace UnityEngine
{
	internal static class ClassLibraryInitializer
	{
		private static void Init()
		{
			UnityLogWriter.Init();
			if (Application.platform.ToString().Contains("WebPlayer"))
			{
				var BF = new BinaryFormatter();
				BF.SurrogateSelector = new UnitySurrogateSelector();
			}
		}
	}
}
