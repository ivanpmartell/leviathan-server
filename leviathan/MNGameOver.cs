using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNGameOver")]
public class MNGameOver : MNode
{
	public override void Awake()
	{
		base.Awake();
	}

	public override void DoAction()
	{
		PLog.Log("End Game" + TurnMan.instance.ToString());
		TurnMan.instance.m_endGame = GameOutcome.Victory;
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
