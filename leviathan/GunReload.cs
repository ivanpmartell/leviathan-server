using System.IO;

internal class GunReload : AIState<Gun>
{
	private float m_timeLeft;

	private float m_reloadTime;

	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		m_reloadTime = owner.GetReloadTime();
		m_timeLeft = m_reloadTime;
	}

	public override string GetStatusText()
	{
		return "Reloading " + m_timeLeft.ToString("F1");
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		m_timeLeft -= dt;
		if (owner.GetAmmo() == 0)
		{
			sm.PopState();
		}
		else if (m_timeLeft <= 0f)
		{
			owner.LoadGun();
			sm.PopState();
		}
	}

	public override void GetCharageLevel(out float i, out float time)
	{
		i = 1f - m_timeLeft / m_reloadTime;
		time = m_timeLeft;
	}

	public override void Save(BinaryWriter writer)
	{
		writer.Write(m_timeLeft);
		writer.Write(m_reloadTime);
	}

	public override void Load(BinaryReader reader)
	{
		m_timeLeft = reader.ReadSingle();
		m_reloadTime = reader.ReadSingle();
	}
}
