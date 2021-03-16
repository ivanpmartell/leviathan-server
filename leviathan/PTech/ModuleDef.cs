namespace PTech
{
	public class ModuleDef
	{
		public string m_prefab;

		public int m_battery;

		public Vector2i m_pos;

		public Direction m_direction;

		public ModuleDef(string prefab, int battery, Vector2i pos, Direction direction)
		{
			m_prefab = prefab;
			m_battery = battery;
			m_pos = pos;
			m_direction = direction;
		}
	}
}
