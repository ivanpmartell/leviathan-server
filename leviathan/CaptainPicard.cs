public class CaptainPicard : ShipBuff, IBuff_CrewXPGain, IBuff_HullIntegrity
{
	private float m_hullIntegretyPercentage = -1f;

	private float m_crewXPGainPercentage = 1.5f;

	public CaptainPicard()
	{
		m_name = "PICARD";
		m_description = ShipBuff.FloatVarToDescription(m_crewXPGainPercentage, isPercent: true, "crew experience gain");
		m_description = m_description + "\n" + ShipBuff.FloatVarToDescription(m_hullIntegretyPercentage, isPercent: true, "hull integrity");
		m_cost = 10;
		m_iconPath = "Assets\\Textures\\Gui\\Buff_Icon_Captains";
	}

	void IBuff_HullIntegrity.AppendToHullIntegrity(ref float hullIntegrity)
	{
		hullIntegrity *= m_hullIntegretyPercentage / 100f;
	}

	void IBuff_CrewXPGain.AppendToCrewXP(ref float crewXPRate)
	{
		crewXPRate += m_crewXPGainPercentage;
	}
}
