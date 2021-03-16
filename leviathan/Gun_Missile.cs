using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Modules/Gun_Missile")]
public class Gun_Missile : Gun
{
	public Vector3 m_saltPathRandomness = new Vector3(5f, 1f, 5f);

	public bool m_useSaltPath = true;

	public int m_saltPathDetail = 2;

	public bool m_useSmoothPath = true;

	public float m_smoothPathDetail = 45f;

	public float m_missileCurvature;

	public int m_accuracy = 85;

	public float m_accurateRadius = 80f;

	public float m_accuracyScaleWithLength = 33.33f;

	public Gun_Missile()
	{
		m_aim.m_stance = Stance.HoldFire;
	}

	protected override bool FireProjectile(Vector3 targetPos)
	{
		int num = SetUpProjectiles(GetMuzzlePos(), targetPos, 9999);
		if (num > 0)
		{
			ClearOrders();
			return true;
		}
		return false;
	}

	public override float GetTargetRadius(Vector3 targetPos)
	{
		return m_accurateRadius;
	}

	private void RandomTargetPos(ref Vector3 target)
	{
		float num = PRand.Range(1f, m_accurateRadius);
		float f = PRand.Range(1f, 359f) * ((float)Math.PI / 180f);
		float num2 = Mathf.Cos(f) * num;
		float num3 = Mathf.Sin(f) * num;
		target.x += num2;
		target.z += num3;
	}

	private void ModifyTarget(Vector3 muzzlePos, ref Vector3 targetPos)
	{
		int accuracy = m_accuracy;
		float num = Mathf.Clamp(m_accuracyScaleWithLength, 0f, 100f);
		if (num != 0f)
		{
			float num2 = Mathf.Abs(Vector3.Distance(muzzlePos, targetPos)) / m_aim.m_maxRange;
			accuracy = (int)((float)accuracy - num * num2);
		}
		RandomTargetPos(ref targetPos);
	}

	protected override void UpdateFireOrder(Order o)
	{
		if (o.m_type == Order.Type.Fire)
		{
			Vector3 pos = o.GetPos();
			bool lOSBlocked = false;
			bool inFiringCone = InFiringCone(pos);
			o.SetLOSBlocked(lOSBlocked);
			o.SetInFiringCone(inFiringCone);
		}
	}

	protected int SetUpProjectiles(Vector3 muzzlePos, Vector3 targetPos, int max)
	{
		ModifyTarget(muzzlePos, ref targetPos);
		GameObject gameObject = ObjectFactory.Clone(m_projectile, muzzlePos, Quaternion.identity);
		Vector3 localScale = gameObject.transform.localScale;
		float num = 1.5f + (localScale.x + localScale.y + localScale.z) * 0.333f;
		muzzlePos += Vector3.up * num;
		Projectile_Missile component = gameObject.GetComponent<Projectile_Missile>();
		component.Setup(Vector3.up, GetOwner(), m_muzzleVel, m_gravity, this, GetRandomDamage(), m_Damage.m_armorPiercing, m_Damage.m_splashRadius, m_Damage.m_splashDamageFactor, m_aim.m_minRange, m_aim.m_maxRange);
		component.SetFlightPath(muzzlePos, targetPos, Vector3.up);
		return 1;
	}

	public override bool AimAt(Vector3 target)
	{
		return true;
	}

	public override Dictionary<string, string> GetShipEditorInfo()
	{
		return base.GetShipEditorInfo();
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_saltPathRandomness.x);
		writer.Write(m_saltPathRandomness.y);
		writer.Write(m_saltPathRandomness.z);
		writer.Write(m_useSaltPath);
		writer.Write(m_saltPathDetail);
		writer.Write(m_accuracy);
		writer.Write(m_accurateRadius);
		writer.Write(m_accuracyScaleWithLength);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_saltPathRandomness.x = reader.ReadSingle();
		m_saltPathRandomness.y = reader.ReadSingle();
		m_saltPathRandomness.z = reader.ReadSingle();
		m_useSaltPath = reader.ReadBoolean();
		m_saltPathDetail = reader.ReadInt32();
		m_accuracy = reader.ReadInt32();
		m_accurateRadius = reader.ReadSingle();
		m_accuracyScaleWithLength = reader.ReadSingle();
	}
}
