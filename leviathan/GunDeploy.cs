using System.IO;
using UnityEngine;

internal class GunDeploy : AIState<Gun>
{
	private float m_timeLeft;

	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		m_timeLeft = owner.GetPreFireTime();
	}

	public override string GetStatusText()
	{
		return "Prepare deployment " + m_timeLeft.ToString("F1");
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		Vector3 vector = owner.transform.position + owner.transform.forward * (owner.m_aim.m_minRange + owner.m_aim.m_maxRange) * 0.5f;
		if (owner.AimAt(vector))
		{
			m_timeLeft -= dt;
			if (m_timeLeft <= 0f)
			{
				GunTarget target = new GunTarget(vector);
				owner.SetTarget(target);
				sm.ChangeState("fire");
				owner.SetDeploy(deploy: false);
			}
		}
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
