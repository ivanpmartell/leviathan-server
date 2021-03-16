#define DEBUG
using System;
using System.IO;
using PTech;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
	[Serializable]
	public class GameModeScript
	{
		public GameType m_type;

		public GameObject m_gameObject;
	}

	public int m_mapSize = 500;

	public GameObject m_border500;

	public GameObject m_border1000;

	public GameObject m_border2000;

	public GameObject m_border750;

	public GameObject m_waterSurface;

	public GameModeScript[] m_gameModes;

	public string m_music = "planning";

	private GameMode m_gameModeScript;

	private void Awake()
	{
		WaterSurface component = m_waterSurface.GetComponent<WaterSurface>();
		if (component != null)
		{
			SetupJazzy();
			component.SetMapSize(m_mapSize);
		}
		else
		{
			PLog.LogError("Failed to find water");
		}
	}

	private void SetupJazzy()
	{
		GameType gameType = ClientGame.instance.GetGameType();
		if ((gameType == GameType.Assassination || gameType == GameType.Points) && PlayerPrefs.GetInt("JazzyMode", 0) == 1)
		{
			WaterSurface component = m_waterSurface.GetComponent<WaterSurface>();
			Material material = Resources.Load("WaterMaterial jazzy") as Material;
			DebugUtils.Assert(material != null);
			component.GetComponent<MeshRenderer>().sharedMaterial = material;
			Light light = base.gameObject.transform.FindChild("Directional light").light;
			light.transform.rotation = Quaternion.Euler(new Vector3(19.86757f, 126.9407f, 215.5935f));
			light.color = new Color(154f / 255f, 104f / 255f, 137f / 255f, 255f);
			light.intensity = 1f;
		}
	}

	private void Start()
	{
		SetupBorder();
	}

	private void OnDestroy()
	{
		if (m_gameModeScript != null)
		{
			m_gameModeScript.Close();
		}
	}

	public void SetupGameMode(GameSettings gameSettings)
	{
		if (m_gameModeScript != null)
		{
			m_gameModeScript.Close();
			m_gameModeScript = null;
		}
		if (m_gameModes != null)
		{
			GameModeScript[] gameModes = m_gameModes;
			foreach (GameModeScript gameModeScript in gameModes)
			{
				DebugUtils.Assert(gameModeScript.m_gameObject != null, "GameModeScript is not attached to an GameObject");
				gameModeScript.m_gameObject.SetActiveRecursively(state: false);
			}
			GameModeScript[] gameModes2 = m_gameModes;
			foreach (GameModeScript gameModeScript2 in gameModes2)
			{
				if (gameModeScript2.m_type == gameSettings.m_gameType)
				{
					gameModeScript2.m_gameObject.SetActiveRecursively(state: true);
					m_gameModeScript = gameModeScript2.m_gameObject.GetComponent<GameMode>();
					DebugUtils.Assert(m_gameModeScript != null, "game mode object missing GameMode-script");
					m_gameModeScript.Setup(gameSettings);
				}
			}
		}
		if (m_gameModeScript == null)
		{
			PLog.LogError("Missing game mode object in levelobject for game mode: " + gameSettings.m_gameType);
		}
	}

	public void SimulationUpdate(float dt)
	{
		DebugUtils.Assert(m_gameModeScript != null);
		m_gameModeScript.SimulationUpdate(dt);
	}

	private void SetupBorder()
	{
		switch (m_mapSize)
		{
		case 500:
			UnityEngine.Object.Instantiate(m_border500);
			break;
		case 750:
			UnityEngine.Object.Instantiate(m_border750);
			break;
		case 1000:
			UnityEngine.Object.Instantiate(m_border1000);
			break;
		case 2000:
			UnityEngine.Object.Instantiate(m_border2000);
			break;
		}
	}

	public int GetMapSize()
	{
		return m_mapSize;
	}

	public void SaveState(BinaryWriter writer)
	{
		DebugUtils.Assert(m_gameModeScript != null);
		if (m_waterSurface != null)
		{
			m_waterSurface.GetComponent<WaterSurface>().SaveState(writer);
		}
		m_gameModeScript.SaveState(writer);
	}

	public void LoadState(BinaryReader reader)
	{
		DebugUtils.Assert(m_gameModeScript != null);
		if (m_waterSurface != null)
		{
			m_waterSurface.GetComponent<WaterSurface>().LoadState(reader);
		}
		m_gameModeScript.LoadState(reader);
	}

	public void SetSimulating(bool simulating)
	{
		if (m_waterSurface != null)
		{
			m_waterSurface.GetComponent<WaterSurface>().SetSimulating(simulating);
		}
	}

	public void DEBUG_DisableWater()
	{
		if (m_waterSurface != null)
		{
			m_waterSurface.renderer.enabled = false;
		}
	}

	public GameMode GetGameModeScript()
	{
		return m_gameModeScript;
	}
}
