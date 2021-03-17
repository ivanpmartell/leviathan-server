using System;

namespace UnityEngine
{
	public sealed class AddComponentMenu : Attribute
	{
		private string m_AddComponentMenu;

		public string componentMenu => m_AddComponentMenu;

		public AddComponentMenu(string menuName)
		{
			m_AddComponentMenu = menuName;
		}
	}
}
