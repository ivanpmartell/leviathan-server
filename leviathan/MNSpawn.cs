using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNSpawn")]
public class MNSpawn : MNode
{
	public enum PlayerAmount
	{
		Always,
		OnePlayer,
		TwoPlayers,
		ThreePlayers,
		FourPlayers,
		OnePlayerOrMore,
		TwoPlayerOrMore,
		ThreePlayerOrMore,
		FourPlayerOrMore
	}

	public enum AiState
	{
		Inactive,
		Guard,
		Patrol,
		BossC1M3
	}

	public string m_spawnShip = "Phantom";

	public string m_group = string.Empty;

	public bool m_atStartup;

	public bool m_onlyIfInGame;

	public PlayerAmount m_whenNumPlayers;

	public string m_area;

	public string m_name;

	public ShipAISettings m_aiSettings = new ShipAISettings();

	private int m_spawnedId = -1;

	public override void Awake()
	{
		base.Awake();
	}

	private void FixedUpdate()
	{
		if (NetObj.m_simulating && m_atStartup)
		{
			DoAction();
			m_atStartup = false;
		}
	}

	public virtual void OnDrawGizmosSelected()
	{
		m_aiSettings.OnDrawGizmosSelected(base.gameObject);
	}

	public bool ShouldSpawn()
	{
		if (m_onlyIfInGame && !TurnMan.instance.IsHuman((int)m_aiSettings.m_targetOwner))
		{
			return false;
		}
		int nrOfPlayers = TurnMan.instance.GetNrOfPlayers();
		if (m_whenNumPlayers != 0)
		{
			if (m_whenNumPlayers == PlayerAmount.OnePlayer && nrOfPlayers != 1)
			{
				return false;
			}
			if (m_whenNumPlayers == PlayerAmount.TwoPlayers && nrOfPlayers != 2)
			{
				return false;
			}
			if (m_whenNumPlayers == PlayerAmount.ThreePlayers && nrOfPlayers != 3)
			{
				return false;
			}
			if (m_whenNumPlayers == PlayerAmount.FourPlayers && nrOfPlayers != 4)
			{
				return false;
			}
			if (m_whenNumPlayers >= PlayerAmount.OnePlayerOrMore)
			{
				int num = (int)(m_whenNumPlayers - 4);
				if (TurnMan.instance.GetNrOfPlayers() < num)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void DoAction()
	{
		if (!ShouldSpawn())
		{
			return;
		}
		if (!ShipFactory.instance.ShipExist(m_spawnShip))
		{
			PLog.LogError("Could not find ship " + m_spawnShip + " in factory :(");
			return;
		}
		ShipAISettings.PlayerId playerId = m_aiSettings.m_targetOwner;
		if (playerId == ShipAISettings.PlayerId.NoChange)
		{
			PLog.Log("NoChange owner can not be used on spawning ship. Will spawn as Neutral. Spawner NetId: " + GetNetID());
			playerId = ShipAISettings.PlayerId.Neutral;
		}
		GameObject gameObject = ShipFactory.instance.CreateShip(m_spawnShip, base.transform.position, base.transform.rotation, (int)playerId);
		gameObject.GetComponent<Unit>().SetGroup(m_group);
		gameObject.GetComponent<Ship>().SetAiSettings(m_aiSettings);
		m_spawnedId = gameObject.GetComponent<NetObj>().GetNetID();
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_group);
		writer.Write(m_spawnShip);
		writer.Write(m_atStartup);
		writer.Write(m_onlyIfInGame);
		writer.Write((int)m_whenNumPlayers);
		writer.Write(m_area);
		writer.Write(m_name);
		writer.Write(m_spawnedId);
		m_aiSettings.SaveState(writer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_group = reader.ReadString();
		m_spawnShip = reader.ReadString();
		m_atStartup = reader.ReadBoolean();
		m_onlyIfInGame = reader.ReadBoolean();
		m_whenNumPlayers = (PlayerAmount)reader.ReadInt32();
		m_area = reader.ReadString();
		m_name = reader.ReadString();
		m_spawnedId = reader.ReadInt32();
		m_aiSettings.LoadState(reader);
	}

	public void SetObjective(Unit.ObjectiveTypes objectivety)
	{
		GameObject spawnedShip = GetSpawnedShip();
		if (!(spawnedShip == null))
		{
			Unit component = spawnedShip.GetComponent<Unit>();
			if (component != null)
			{
				component.SetObjective(objectivety);
			}
		}
	}

	public GameObject GetSpawnedShip()
	{
		if (m_spawnedId == -1)
		{
			return null;
		}
		NetObj byID = NetObj.GetByID(m_spawnedId);
		if (byID == null)
		{
			return null;
		}
		return byID.gameObject;
	}

	public bool SpawnedBeenDestroyed()
	{
		if (m_spawnedId == -1)
		{
			return false;
		}
		NetObj byID = NetObj.GetByID(m_spawnedId);
		if (byID == null)
		{
			return true;
		}
		Unit unit = byID as Unit;
		if (unit == null)
		{
			return true;
		}
		return unit.IsDead();
	}
}
