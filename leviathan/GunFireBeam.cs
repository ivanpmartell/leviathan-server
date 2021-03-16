using System.IO;

internal class GunFireBeam : AIState<Gun>
{
	public float m_timeLeft;

	private float m_updateOrderTimer;

	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		PLog.Log("enter fire beam");
		m_timeLeft = owner.GetPreFireTime();
	}

	public override void Exit(Gun owner)
	{
		PLog.Log("exit firebeam");
		if (owner.IsFiring())
		{
			owner.StopFiring();
		}
	}

	public void SetTimeLeft(float timeLeft)
	{
		m_timeLeft = timeLeft;
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
		m_timeLeft -= dt;
		if (m_timeLeft > 0f)
		{
			return;
		}
		if (m_timeLeft < 0f)
		{
			m_timeLeft = 0f;
		}
		GunTarget target = owner.GetTarget();
		if (target != null && target.GetTargetWorldPos(out var worldPos, owner.GetOwnerTeam()))
		{
			if (!owner.InFiringCone(worldPos))
			{
				PLog.Log("target is out of firing cone");
				sm.PopState();
				return;
			}
			if (owner.AimAt(worldPos))
			{
				m_updateOrderTimer -= dt;
				if (m_updateOrderTimer < 0f)
				{
					m_updateOrderTimer = 0.25f;
					UpdateTarget(owner);
				}
			}
			if (!owner.IsFiring())
			{
				owner.StartFiring();
			}
			else if (owner.GetLoadedSalvo() <= 0f)
			{
				sm.ChangeState("reload");
			}
		}
		else
		{
			sm.ChangeState("reload");
		}
	}

	private bool UpdateTarget(Gun owner)
	{
		Order firstOrder = owner.GetFirstOrder();
		if (firstOrder != null && firstOrder.m_type == Order.Type.Fire)
		{
			GunTarget target = ((!firstOrder.IsStaticTarget()) ? new GunTarget(firstOrder.GetTargetID(), firstOrder.GetLocalTargetPos()) : new GunTarget(firstOrder.GetLocalTargetPos()));
			if (owner.InFiringCone(target))
			{
				owner.SetTarget(target);
				owner.RemoveFirstOrder();
				return true;
			}
		}
		return false;
	}

	public override void Save(BinaryWriter writer)
	{
		writer.Write(m_timeLeft);
	}

	public override void Load(BinaryReader reader)
	{
		m_timeLeft = reader.ReadSingle();
	}
}
