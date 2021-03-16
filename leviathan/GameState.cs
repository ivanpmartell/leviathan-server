#define DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using PTech;
using UnityEngine;

internal class GameState
{
	public delegate void SetupCompleteHandler();

	public delegate void SimulationCompleteHandler();

	public SetupCompleteHandler m_onSetupComplete;

	public SimulationCompleteHandler m_onSimulationComplete;

	private GameSettings m_gameSettings;

	private LevelScript m_levelScript;

	private GameCamera m_gameCamera;

	private GameObject m_guiCamera;

	private TurnMan m_turnMan;

	private float m_lineOfSightUpdateTimer;

	private int m_updateLOSIndex;

	private int m_LOSUpdatesPerFrame = 2;

	private int m_localPlayerID = -1;

	private int m_frames;

	private bool m_gameHasEnded;

	private TurnPhase m_phase;

	private bool m_fastSimulation;

	private byte[] m_state;

	private bool m_setupComplete;

	private bool m_levelWasLoaded;

	private int m_currentFrame;

	public GameState(GameObject guiCamera, GameSettings gameSettings, TurnMan turnMan, TurnPhase phase, bool fastSimulation, byte[] state, int localPlayerID, int frames, SetupCompleteHandler setupStatecomplete, SimulationCompleteHandler simulationComplete)
	{
		m_gameSettings = gameSettings;
		m_turnMan = turnMan;
		m_phase = phase;
		m_fastSimulation = fastSimulation;
		m_guiCamera = guiCamera;
		m_localPlayerID = localPlayerID;
		m_state = state;
		m_frames = frames;
		m_onSetupComplete = setupStatecomplete;
		m_onSimulationComplete = simulationComplete;
		NetObj.SetLocalPlayer(localPlayerID);
		NetObj.SetPhase(m_phase);
		TurnMan.instance.ResetTurnStats();
		if (m_state == null)
		{
			NetObj.ResetObjectDB();
			NetObj.SetNextNetID(1);
		}
		if (Application.loadedLevelName == gameSettings.m_mapInfo.m_scene)
		{
			m_levelWasLoaded = true;
			return;
		}
		PLog.Log("GameState: queueing load scene " + gameSettings.m_mapInfo.m_scene);
		main.LoadLevel(gameSettings.m_mapInfo.m_scene, showLoadScreen: false);
	}

	public void FixedUpdate()
	{
		if (m_levelWasLoaded && !m_setupComplete)
		{
			DoSetup();
			m_setupComplete = true;
		}
		if (!IsSimulating())
		{
			return;
		}
		m_currentFrame++;
		DoIntervalLineOfSightUpdate();
		m_levelScript.SimulationUpdate(Time.fixedDeltaTime);
		if (m_levelScript.GetGameModeScript().GetOutcome() != 0 && !m_gameHasEnded)
		{
			m_gameHasEnded = true;
			if (m_gameSettings.m_gameType == GameType.Campaign)
			{
				m_frames = m_currentFrame;
			}
			else
			{
				m_frames = m_currentFrame + 150;
			}
			PLog.Log("game has ended");
		}
		if (m_currentFrame >= m_frames)
		{
			UpdateLineOfSightAll();
			SetSimulating(enabled: false);
			m_levelScript.GetGameModeScript().OnSimulationComplete();
			if (m_onSimulationComplete != null)
			{
				m_onSimulationComplete();
			}
		}
	}

	public void OnLevelWasLoaded()
	{
		m_levelWasLoaded = true;
	}

	private void DoSetup()
	{
		m_levelScript = GameObject.Find("LevelObject").GetComponent<LevelScript>();
		DebugUtils.Assert(m_levelScript != null);
		m_turnMan.SetTurnMusic(m_levelScript.m_music);
		m_levelScript.SetupGameMode(m_gameSettings);
		if (m_state != null)
		{
			SetState(m_state);
			m_levelScript.GetGameModeScript().OnStateLoaded();
		}
		else
		{
			PRand.SetSeed(DateTime.Now.Second * 10);
		}
		m_gameCamera = m_levelScript.transform.FindChild("GameCamera").GetComponent<GameCamera>();
		m_gameCamera.Setup(m_localPlayerID, m_levelScript.GetMapSize(), m_guiCamera);
		SetSimulating(enabled: false);
		if (m_onSetupComplete != null)
		{
			m_onSetupComplete();
		}
	}

	private void SetState(byte[] data)
	{
		NetObj[] array = UnityEngine.Object.FindObjectsOfType(typeof(NetObj)) as NetObj[];
		NetObj[] array2 = array;
		foreach (NetObj netObj in array2)
		{
			if (netObj != null)
			{
				UnityEngine.Object.DestroyImmediate(netObj.gameObject);
			}
		}
		Resources.UnloadUnusedAssets();
		GC.Collect();
		NetObj.ResetObjectDB();
		MemoryStream memoryStream = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		PRand.SetSeed(binaryReader.ReadInt32());
		m_levelScript.LoadState(binaryReader);
		m_turnMan.Load(binaryReader);
		int num = binaryReader.ReadInt32();
		for (int j = 0; j < num; j++)
		{
			string text = binaryReader.ReadString();
			GameObject gameObject = ObjectFactory.instance.Create(text);
			DebugUtils.Assert(gameObject, "Faield to create object instance " + text);
			long position = memoryStream.Position;
			NetObj component = gameObject.GetComponent<NetObj>();
			component.LoadState(binaryReader);
			long num2 = memoryStream.Position - position;
		}
		NetObj.SetNextNetID(binaryReader.ReadInt32());
		UpdateLineOfSightAll();
		if (m_localPlayerID >= 0)
		{
			UpdateVisability(m_localPlayerID);
		}
	}

	public byte[] GetState()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(PRand.GetSeed());
		m_levelScript.SaveState(binaryWriter);
		m_turnMan.Save(binaryWriter);
		NetObj[] allToSave = NetObj.GetAllToSave();
		binaryWriter.Write(allToSave.Length);
		NetObj[] array = allToSave;
		foreach (NetObj netObj in array)
		{
			binaryWriter.Write(netObj.name);
			netObj.SaveState(binaryWriter);
		}
		binaryWriter.Write(NetObj.GetNextNetID());
		return memoryStream.ToArray();
	}

	public void SetOrders(int playerID, byte[] orders)
	{
		MemoryStream input = new MemoryStream(orders);
		BinaryReader binaryReader = new BinaryReader(input);
		for (int num = binaryReader.ReadInt32(); num != 0; num = binaryReader.ReadInt32())
		{
			Unit unit = NetObj.GetByID(num) as Unit;
			if (unit == null)
			{
				PLog.LogError("Could not find unit " + num + " in state");
			}
			if (!MNTutorial.IsTutorialActive() && playerID != -1 && unit.GetOwner() != playerID)
			{
				PLog.LogError("Player " + playerID + " gave order to unit " + num + " owned by " + unit.GetOwner());
			}
			unit.LoadOrders(binaryReader);
		}
	}

	public void ClearNonLocalOrders()
	{
		if (m_localPlayerID == -1)
		{
			return;
		}
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			Unit unit = item as Unit;
			if (unit != null && unit.GetOwner() != m_localPlayerID)
			{
				unit.ClearOrders();
			}
		}
	}

	public byte[] GetOrders(int playerID)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		NetObj[] allToSave = NetObj.GetAllToSave();
		NetObj[] array = allToSave;
		foreach (NetObj netObj in array)
		{
			Unit unit = netObj as Unit;
			if (unit != null && (playerID == -1 || unit.GetOwner() == playerID || MNTutorial.IsTutorialActive()))
			{
				binaryWriter.Write(unit.GetNetID());
				unit.SaveOrders(binaryWriter);
			}
		}
		binaryWriter.Write(0);
		return memoryStream.ToArray();
	}

	public int GetCurrentFrame()
	{
		return m_currentFrame;
	}

	public int GetTotalFrames()
	{
		return m_frames;
	}

	private void UpdateLineOfSightAll()
	{
		UpdateLineOfSightIterative(1000000);
	}

	private void UpdateLineOfSightIterative(int toUpdate)
	{
		List<NetObj> all = NetObj.GetAll();
		if (m_updateLOSIndex >= all.Count)
		{
			m_updateLOSIndex = 0;
		}
		int num = m_updateLOSIndex + toUpdate;
		if (num > all.Count)
		{
			num = all.Count;
		}
		for (int i = m_updateLOSIndex; i < num; i++)
		{
			NetObj netObj = all[i];
			if (!netObj.GetUpdateSeenBy())
			{
				continue;
			}
			int num2 = 0;
			int owner = netObj.GetOwner();
			int playerTeam = m_turnMan.GetPlayerTeam(owner);
			foreach (NetObj item in all)
			{
				Unit unit = item as Unit;
				if (unit != null && unit.CanLOS())
				{
					int owner2 = unit.GetOwner();
					int playerTeam2 = m_turnMan.GetPlayerTeam(owner2);
					if (CheatMan.instance.GetNoFogOfWar() && m_turnMan.IsHuman(owner2))
					{
						num2 |= 1 << (playerTeam2 & 0x1F);
					}
					else if (owner2 == owner)
					{
						num2 |= 1 << (playerTeam2 & 0x1F);
					}
					else if (playerTeam2 == playerTeam)
					{
						num2 |= 1 << (playerTeam2 & 0x1F);
					}
					else if (unit.TestLOS(netObj))
					{
						num2 |= 1 << (playerTeam2 & 0x1F);
					}
				}
			}
			netObj.UpdateSeenByMask(num2);
		}
		m_updateLOSIndex = num;
	}

	private void DoIntervalLineOfSightUpdate()
	{
		m_lineOfSightUpdateTimer -= Time.fixedDeltaTime;
		if (m_lineOfSightUpdateTimer <= 0f)
		{
			m_lineOfSightUpdateTimer += 0.01f;
			UpdateLineOfSightIterative(m_LOSUpdatesPerFrame);
			if (m_localPlayerID >= 0)
			{
				UpdateVisability(m_localPlayerID);
			}
		}
	}

	private void UpdateVisability(int localPlayerID)
	{
		int playerTeam = m_turnMan.GetPlayerTeam(localPlayerID);
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			if (item.IsSeenByTeam(playerTeam))
			{
				item.SetVisible(visible: true);
			}
			else
			{
				item.SetVisible(visible: false);
			}
		}
	}

	public void SetSimulating(bool enabled)
	{
		if (enabled && m_currentFrame >= m_frames)
		{
			PLog.LogWarning("GameState: tried to enable simulation at end of simulation");
			return;
		}
		NetObj.SetSimulating(enabled);
		if (m_levelScript != null)
		{
			m_levelScript.SetSimulating(enabled);
		}
		if (enabled && m_fastSimulation)
		{
			Time.timeScale = 99f;
			Time.captureFramerate = 1;
		}
		else
		{
			Time.timeScale = 1f;
			Time.captureFramerate = 0;
		}
	}

	public bool IsSimulating()
	{
		return NetObj.IsSimulating();
	}

	public GameCamera GetGameCamera()
	{
		return m_gameCamera;
	}

	public GameMode GetGameModeScript()
	{
		if (m_levelScript == null)
		{
			return null;
		}
		return m_levelScript.GetGameModeScript();
	}

	public LevelScript GetLevelScript()
	{
		return m_levelScript;
	}

	public TurnMan GetTurnMan()
	{
		return m_turnMan;
	}
}
