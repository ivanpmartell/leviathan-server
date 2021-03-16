using System.Collections.Generic;

namespace PTech
{
	public class SectionDef
	{
		public string m_prefab;

		public List<ModuleDef> m_modules = new List<ModuleDef>();

		public void RemoveModule(int battery, Vector2i pos)
		{
			foreach (ModuleDef module in m_modules)
			{
				if (module.m_battery == battery && module.m_pos.x == pos.x && module.m_pos.y == pos.y)
				{
					m_modules.Remove(module);
					break;
				}
			}
		}

		public List<string> GetHardpointNames()
		{
			List<string> list = new List<string>();
			foreach (ModuleDef module in m_modules)
			{
				list.Add(module.m_prefab);
			}
			return list;
		}
	}
}
