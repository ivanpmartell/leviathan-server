using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Weapons/MineVisibility")]
public class MineVisibility : Mine
{
	public float m_baseSightRange = 50f;

	public override void Setup(int ownerID, int unitID, bool visible, int seenByMask, int damage, int ap, float splashRadius, float splashDmgFactor)
	{
		base.Setup(ownerID, unitID, visible, seenByMask, damage, ap, splashRadius, splashDmgFactor);
		SetOwner(ownerID);
		SetVisible(visible);
		SetSeenByMask(seenByMask);
	}

	public override void Awake()
	{
		base.Awake();
	}

	protected override void FixedUpdate()
	{
		if (IsDeployed())
		{
			SetSightRange(m_baseSightRange);
		}
		else
		{
			SetSightRange(0f);
		}
		base.FixedUpdate();
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_baseSightRange);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_baseSightRange = reader.ReadSingle();
	}

	public override bool TestLOS(NetObj obj)
	{
		if (!IsDeployed())
		{
			return false;
		}
		float num = Vector3.Distance(base.transform.position, obj.transform.position);
		if (num > m_baseSightRange)
		{
			return false;
		}
		return true;
	}
}
