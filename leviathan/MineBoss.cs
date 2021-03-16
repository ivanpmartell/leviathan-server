using UnityEngine;

[AddComponentMenu("Scripts/Weapons/MineBoss")]
public class MineBoss : MineExplode
{
	public override void Setup(int ownerID, int unitID, bool visible, int seenByMask, int damage, int ap, float splashRadius, float splashDmgFactor)
	{
		base.Setup(ownerID, unitID, visible, seenByMask, damage, ap, splashRadius, splashDmgFactor);
		m_radiusMesh.renderer.enabled = false;
	}

	public override void Awake()
	{
		base.Awake();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override void Update()
	{
		base.Update();
	}

	public override bool Damage(Hit hit)
	{
		if (GetOwner() == hit.m_dealer.GetOwner())
		{
			return true;
		}
		return base.Damage(hit);
	}
}
