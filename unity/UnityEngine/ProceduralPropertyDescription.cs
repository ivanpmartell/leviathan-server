using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class ProceduralPropertyDescription
	{
		public string name;

		public string group;

		public ProceduralPropertyType type;

		public bool hasRange;

		public float minimum;

		public float maximum;

		public float step;

		public string[] enumOptions;
	}
}
