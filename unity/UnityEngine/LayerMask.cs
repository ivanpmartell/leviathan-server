using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public struct LayerMask
	{
		private int m_Mask;

		public int value
		{
			get
			{
				return m_Mask;
			}
			set
			{
				m_Mask = value;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern string LayerToName(int layer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public static extern int NameToLayer(string layerName);

		public static implicit operator int(LayerMask mask)
		{
			return mask.m_Mask;
		}

		public static implicit operator LayerMask(int intVal)
		{
			LayerMask result = default(LayerMask);
			result.m_Mask = intVal;
			return result;
		}
	}
}
