using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Projectiles/Projectile_Missile")]
public class Projectile_Missile : Projectile_MultiTarget
{
	public TrailRenderer Trail;

	public float PrefferedHeight = 40f;

	protected override void SpecializedFixedUpdate()
	{
		base.SpecializedFixedUpdate();
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		Utils.SaveTrail(Trail, writer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		Utils.LoadTrail(ref Trail, reader);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void SetFlightPath(Vector3 muzzlePos, Vector3 targetPos, Vector3 dir)
	{
		AddTarget(muzzlePos + dir * 10f);
		Vector3 vector = muzzlePos + dir * 10f;
		AddTarget(vector);
		Vector3 vector2 = targetPos - muzzlePos;
		Vector3 inTarget = muzzlePos + vector2 * 0.5f + new Vector3(0f, PrefferedHeight, 0f);
		Vector3 vector3 = muzzlePos + vector2 * 0.25f + new Vector3(0f, PrefferedHeight, 0f);
		Vector3 control = muzzlePos + new Vector3(0f, PrefferedHeight, 0f);
		int num = 30;
		for (int i = 1; i < num; i++)
		{
			float delta = (float)i / (float)(num - 1);
			Vector3 inTarget2 = Utils.Bezier2(vector, control, vector3, delta);
			AddTarget(inTarget2);
		}
		AddTarget(vector3);
		AddTarget(inTarget);
		Vector3 vector4 = muzzlePos + vector2 * 0.75f + new Vector3(0f, PrefferedHeight, 0f);
		AddTarget(vector4);
		control = targetPos + new Vector3(0f, PrefferedHeight, 0f);
		for (int j = 1; j < num; j++)
		{
			float delta2 = (float)j / (float)(num - 1);
			Vector3 inTarget3 = Utils.Bezier2(vector4, control, targetPos, delta2);
			AddTarget(inTarget3);
		}
		AddTarget(targetPos);
		targetPos.y = -50f;
		AddTarget(targetPos);
		targetPos.y = -100f;
		AddTarget(targetPos);
	}
}
