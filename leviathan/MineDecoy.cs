using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Weapons/MineDecoy")]
public class MineDecoy : Mine
{
	public float m_decoySize = 20f;

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
		base.FixedUpdate();
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_decoySize);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_decoySize = reader.ReadSingle();
		base.transform.GetChild(0).animation.CrossFadeQueued("idle", 0f, QueueMode.PlayNow);
	}

	protected override void OnTimeout()
	{
		Explode();
	}

	protected override void OnDeploy()
	{
		base.transform.GetChild(0).animation.CrossFadeQueued("spawn", 0.2f, QueueMode.PlayNow);
		base.transform.GetChild(0).animation.CrossFadeQueued("idle", 0.2f, QueueMode.CompleteOthers);
	}

	public override Vector3[] GetTargetPoints()
	{
		Vector3[] array = new Vector3[1] { base.transform.position };
		array[0].y += 1.5f;
		return array;
	}

	public override void SetOwner(int owner)
	{
		base.SetOwner(owner);
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		Color primaryColor = Color.white;
		if (TurnMan.instance != null)
		{
			TurnMan.instance.GetPlayerColors(GetOwner(), out primaryColor);
		}
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.material.SetColor("_TeamColor0", primaryColor);
		}
	}

	public override float GetLength()
	{
		return m_decoySize;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (NetObj.m_simulating)
		{
			Section component = other.GetComponent<Section>();
			if (!component && other.gameObject.transform.parent != null)
			{
				component = other.gameObject.transform.parent.GetComponent<Section>();
			}
			if (component != null)
			{
				Explode();
			}
		}
	}
}
