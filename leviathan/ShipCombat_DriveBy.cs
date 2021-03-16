using UnityEngine;

internal class ShipCombat_DriveBy : ShipCombat_Base
{
	public override string DebugString(Ship owner)
	{
		return base.DebugString(owner);
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
	}

	private Vector3 GetBehindPosition(Ship ship)
	{
		return ship.transform.position - ship.transform.forward * 50f;
	}

	private bool GetDriveByPosition(Unit target, Ship ship, out Vector3 pos)
	{
		float num = 100f;
		float f = PRand.Range(0, 360);
		Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
		pos = target.transform.position + vector * num;
		if (ship.IsWater(pos))
		{
			return true;
		}
		return false;
	}

	private void UpdateMovement(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		if (owner.IsBlocked())
		{
			owner.ClearMoveOrders();
		}
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		if (SwitchState(owner, sm))
		{
			return;
		}
		Unit target = VerifyTarget(owner);
		float optimalAttackRange = owner.GetShipAi().GetOptimalAttackRange(UnitAi.AttackDirection.None);
		float num = RangeToTarget(owner);
		float num2 = Mathf.Abs(num - optimalAttackRange);
		if (num2 < 30f)
		{
			sm.ChangeState("c_turnandfire");
			return;
		}
		UpdateMovement(owner, sm, dt);
		if (!owner.IsOrdersEmpty())
		{
			return;
		}
		if (true)
		{
			Vector3 pos = default(Vector3);
			if (GetDriveByPosition(target, owner, out pos))
			{
				owner.SetOrdersTo(pos);
			}
		}
		else
		{
			owner.GetAi().m_targetId = 0;
			sm.PopState();
		}
	}
}
