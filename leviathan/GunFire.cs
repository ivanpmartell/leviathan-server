using System.IO;

internal class GunFire : AIState<Gun>
{
	public float m_timeLeft;

	public float m_prefireTime;

	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		m_timeLeft = owner.GetPreFireTime();
		m_prefireTime = m_timeLeft;
		owner.PreFireGun();
	}

	public override void Exit(Gun owner)
	{
		owner.StopPreFire();
	}

	public void SetTimeLeft(float timeLeft)
	{
		m_timeLeft = timeLeft;
		m_prefireTime = timeLeft;
	}

	public override void GetCharageLevel(out float i, out float time)
	{
		i = 1f - m_timeLeft / m_prefireTime;
		time = m_timeLeft;
	}

	public override string GetStatusText()
	{
		if (m_timeLeft > 0f)
		{
			return "Firing in " + m_timeLeft.ToString("F1");
		}
		return "Firing";
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		if (owner.GetLoadedSalvo() == 0f)
		{
			if (owner.GetAmmo() != 0)
			{
				sm.PushState("reload");
			}
			else
			{
				sm.PopState();
			}
			return;
		}
		m_timeLeft -= dt;
		if (m_timeLeft < 0f)
		{
			if (owner.FireGun() && owner.GetLoadedSalvo() > 0f)
			{
				GunFire gunFire = sm.ChangeState("fire") as GunFire;
				gunFire.SetTimeLeft(owner.GetSalvoDelay());
			}
			else
			{
				sm.ChangeState("reload");
			}
		}
	}

	public override void Save(BinaryWriter writer)
	{
		writer.Write(m_timeLeft);
		writer.Write(m_prefireTime);
	}

	public override void Load(BinaryReader reader)
	{
		m_timeLeft = reader.ReadSingle();
		m_prefireTime = reader.ReadSingle();
	}
}
