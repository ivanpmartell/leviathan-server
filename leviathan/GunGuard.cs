using System.IO;

internal class GunGuard : AIState<Gun>
{
	private readonly float m_resetTowerDelay = 5f;

	private float m_updateTargetTimer;

	private float m_idleTime;

	private bool m_haveResetTower;

	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		m_updateTargetTimer = PRand.Range(0f, 1f);
		m_idleTime = 0f;
		m_haveResetTower = false;
		owner.SetTarget(null);
	}

	public override string GetStatusText()
	{
		return "Looking for target";
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		if (owner.GetFirstOrder() != null || owner.GetDeploy())
		{
			sm.PopState();
			return;
		}
		m_idleTime += dt;
		if (m_idleTime > m_resetTowerDelay && !m_haveResetTower && owner.ResetTower())
		{
			m_haveResetTower = true;
		}
		if ((owner.GetOwner() <= 3 && owner.m_aim.m_noAutotarget) || owner.GetUnit().IsCloaked() || owner.GetUnit().IsDoingMaintenance())
		{
			return;
		}
		m_updateTargetTimer -= dt;
		if (!(m_updateTargetTimer <= 0f))
		{
			return;
		}
		m_updateTargetTimer = 1f;
		if (owner.GetStance() == Gun.Stance.FireAtWill)
		{
			GunTarget gunTarget = owner.FindTarget();
			if (gunTarget != null)
			{
				owner.SetTarget(gunTarget);
				sm.ChangeState("aim");
			}
		}
	}

	public override void Save(BinaryWriter writer)
	{
		writer.Write(m_updateTargetTimer);
		writer.Write(m_idleTime);
		writer.Write(m_haveResetTower);
	}

	public override void Load(BinaryReader reader)
	{
		m_updateTargetTimer = reader.ReadSingle();
		m_idleTime = reader.ReadSingle();
		m_haveResetTower = reader.ReadBoolean();
	}
}
