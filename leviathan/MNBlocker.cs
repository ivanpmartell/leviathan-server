using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNBlocker")]
public class MNBlocker : MNRepeater
{
	public override void Awake()
	{
		base.Awake();
	}

	protected override void Destroy()
	{
	}

	protected override void Update()
	{
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
	}

	public override void DoAction()
	{
		for (int i = 0; i < m_repeatTargets.Length; i++)
		{
			GameObject targetObj = GetTargetObj(i);
			if (targetObj != null)
			{
				targetObj.SetActiveRecursively(state: false);
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
	}
}
