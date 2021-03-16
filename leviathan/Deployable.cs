using System.IO;

public class Deployable : NetObj
{
	protected int m_gunID;

	public virtual void Setup(int ownerID, int gunID, bool visible, int seenByMask, int damage, int ap, float splashRadius, float splashDmgFactor)
	{
		SetOwner(ownerID);
		m_gunID = gunID;
		SetVisible(visible);
		SetSeenByMask(seenByMask);
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_gunID);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_gunID = reader.ReadInt32();
	}
}
