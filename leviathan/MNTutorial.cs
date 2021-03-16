using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNTutorial")]
public class MNTutorial : MNode
{
	public string m_levelName = "t1m2";

	private float m_time;

	private int m_nextEvent = 1;

	private bool m_runEvent;

	private bool b_startOfTurn = true;

	private int m_currentTurn;

	private float m_timeInTurn;

	private bool m_hideCommit = true;

	private bool m_allowSelection;

	private bool m_allowFlowerMenu;

	private int m_endTutorialTurn = -1;

	private static bool m_endTutorial;

	private GameObject m_mainObj;

	public override void Awake()
	{
		base.Awake();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void PlayDialog(string name)
	{
		MNAction.MNActionElement[] array = new MNAction.MNActionElement[1]
		{
			new MNAction.MNActionElement()
		};
		array[0].m_type = MNAction.ActionType.ShowTutorial;
		array[0].m_parameter = "leveldata/campaign/tutorial/" + name;
		TurnMan.instance.m_dialog = array;
	}

	private bool DoEventNow(int eventId)
	{
		if (eventId == m_nextEvent)
		{
			m_time = 0f;
			m_runEvent = true;
			return true;
		}
		return false;
	}

	private void PlayDialogPlatform(string name)
	{
		string text = name;
		text = ((Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android) ? (text + "_pc") : (text + "_pad"));
		PlayDialog(text);
	}

	private void UpdateTutorial()
	{
		string text = m_levelName + "/" + m_levelName + "_";
		if (!IsTutorialOver())
		{
			if (DoEventNow(1))
			{
				PlayDialogPlatform(text + m_currentTurn + "_1");
			}
			if (m_runEvent)
			{
				m_nextEvent++;
			}
			m_runEvent = false;
		}
	}

	private void NextTurn()
	{
		if (m_currentTurn == 0)
		{
			m_endTutorial = false;
		}
		m_currentTurn++;
		m_timeInTurn = 0f;
		if (m_endTutorial)
		{
			m_endTutorialTurn = m_currentTurn;
		}
	}

	private void UpdateGui()
	{
		if (m_mainObj == null)
		{
			m_mainObj = GameObject.Find("mainobject");
		}
		if (!m_mainObj)
		{
			return;
		}
		if (m_hideCommit)
		{
			GameObject gameObject = GuiUtils.FindChildOf(m_mainObj.transform, "ControlPanel_Planning");
			if ((bool)gameObject)
			{
				gameObject.SetActiveRecursively(!m_hideCommit);
			}
		}
		GameObject gameObject2 = GuiUtils.FindChildOf(m_mainObj.transform, "Replay_Button");
		if ((bool)gameObject2)
		{
			gameObject2.SetActiveRecursively(state: false);
		}
	}

	protected Ship GetPlayerShip(int player)
	{
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			Ship component = item.GetComponent<Ship>();
			if (component != null && !component.IsDead() && component.GetOwner() == player)
			{
				return component;
			}
		}
		return null;
	}

	public void OnCommand(string command, string parameter, string parameter2)
	{
		if (command == "clearorder")
		{
			int player = int.Parse(parameter);
			Ship playerShip = GetPlayerShip(player);
			playerShip.ClearOrders();
		}
		if (command == "addorder")
		{
			GameObject gameObject = GameObject.Find(parameter);
			Ship playerShip2 = GetPlayerShip(0);
			Order order = new Order(playerShip2, Order.Type.MoveForward, gameObject.transform.position);
			playerShip2.AddOrder(order);
		}
		if (command == "addorderto")
		{
			PLog.Log("addorderto 1");
			int player2 = int.Parse(parameter);
			Ship playerShip3 = GetPlayerShip(player2);
			GameObject gameObject2 = GameObject.Find(parameter2);
			Order order2 = new Order(playerShip3, Order.Type.MoveForward, gameObject2.transform.position);
			playerShip3.AddOrder(order2);
			PLog.Log("addorderto 2");
		}
		if (command == "mine")
		{
			int player3 = int.Parse(parameter);
			Ship playerShip4 = GetPlayerShip(player3);
			GameObject gameObject3 = GameObject.Find(parameter2);
			Gun componentInChildren = playerShip4.gameObject.GetComponentInChildren<Gun>();
			if (componentInChildren == null)
			{
				return;
			}
			PLog.Log("mine: ");
			Order order3 = new Order(componentInChildren, Order.Type.Fire, gameObject3.transform.position);
			componentInChildren.ClearOrders();
			componentInChildren.AddOrder(order3);
		}
		if (command == "attack")
		{
			int player4 = int.Parse(parameter);
			Ship playerShip5 = GetPlayerShip(player4);
			int player5 = int.Parse(parameter2);
			Ship playerShip6 = GetPlayerShip(player5);
			Gun componentInChildren2 = playerShip5.gameObject.GetComponentInChildren<Gun>();
			if (componentInChildren2 == null)
			{
				return;
			}
			PLog.Log("attack: ");
			Order order4 = new Order(componentInChildren2, Order.Type.Fire, playerShip6.transform.position);
			componentInChildren2.ClearOrders();
			componentInChildren2.AddOrder(order4);
		}
		if (command == "stopattack")
		{
			int player6 = int.Parse(parameter);
			Ship playerShip7 = GetPlayerShip(player6);
		}
		if (command == "deploy")
		{
			int player7 = int.Parse(parameter);
			Ship playerShip8 = GetPlayerShip(player7);
			HPModule hPModule = null;
			if (parameter2 == "radar")
			{
				hPModule = playerShip8.gameObject.GetComponentInChildren<Radar>();
			}
			if (parameter2 == "cloak")
			{
				hPModule = playerShip8.gameObject.GetComponentInChildren<Cloak>();
			}
			if (parameter2 == "shield")
			{
				hPModule = playerShip8.gameObject.GetComponentInChildren<Shield>();
			}
			if (hPModule == null)
			{
				return;
			}
			PLog.Log("Deploy: " + parameter2);
			if (parameter2 == "shield")
			{
				hPModule.GetComponent<Shield>().SetDeployShield(Shield.DeployType.Forward);
			}
			else
			{
				hPModule.SetDeploy(deploy: true);
			}
		}
		if (command == "commitoff")
		{
			m_hideCommit = true;
		}
		if (command == "selectionon")
		{
			m_allowSelection = true;
		}
		if (command == "selectionoff")
		{
			m_allowSelection = false;
		}
		if (command == "flowermenuon")
		{
			m_allowFlowerMenu = true;
		}
		if (command == "flowermenuoff")
		{
			m_allowFlowerMenu = false;
		}
		if (command == "commiton")
		{
			m_hideCommit = false;
			UpdateGui();
		}
		if (command == "cameraon")
		{
			GameCamera component = GameObject.Find("GameCamera").GetComponent<GameCamera>();
			component.SetMode(GameCamera.Mode.Active);
		}
		if (command == "cameraoff")
		{
			GameCamera component2 = GameObject.Find("GameCamera").GetComponent<GameCamera>();
			component2.SetMode(GameCamera.Mode.Disabled);
		}
		if (command == "end")
		{
			m_endTutorial = true;
		}
		if (command == "selectplayer")
		{
			int player8 = int.Parse(parameter);
			int num = int.Parse(parameter2);
			Ship playerShip9 = GetPlayerShip(player8);
			GameCamera component3 = GameObject.Find("GameCamera").GetComponent<GameCamera>();
			component3.SetAllowSelection(allow: true);
			component3.SetSelected(playerShip9.gameObject);
			component3.SetAllowSelection(m_allowSelection);
			component3.SetFocus(playerShip9.gameObject.transform.position, num);
		}
		if (command == "focus")
		{
			GameCamera component4 = GameObject.Find("GameCamera").GetComponent<GameCamera>();
			GameObject gameObject4 = GameObject.Find(parameter);
			int num2 = int.Parse(parameter2);
			if ((bool)gameObject4)
			{
				component4.SetFocus(gameObject4.transform.position, num2);
			}
		}
	}

	private bool IsTutorialOver()
	{
		if (m_endTutorialTurn != -1 && m_currentTurn >= m_endTutorialTurn)
		{
			return true;
		}
		return false;
	}

	public void Update()
	{
		if (m_endTutorialTurn != -1 && m_currentTurn >= m_endTutorialTurn)
		{
			m_allowSelection = true;
			m_allowFlowerMenu = true;
			m_hideCommit = false;
		}
		GameCamera component = GameObject.Find("GameCamera").GetComponent<GameCamera>();
		if (!(component == null))
		{
			component.SetAllowSelection(m_allowSelection);
			component.SetAllowFlowerMenu(m_allowFlowerMenu);
		}
	}

	private void FixedUpdate()
	{
		UpdateGui();
		if (NetObj.m_simulating)
		{
			m_timeInTurn += Time.fixedDeltaTime;
			if (b_startOfTurn)
			{
				NextTurn();
				PLog.Log("Starting turn: " + m_currentTurn + " End Turn is: " + m_endTutorialTurn);
				b_startOfTurn = false;
			}
		}
		Camera component = GameObject.Find("GameCamera").GetComponent<Camera>();
		if (!(component == null) && component.enabled && !NetObj.IsSimulating() && !Dialog.IsDialogActive())
		{
			m_time += Time.fixedDeltaTime;
			if (!(m_time < 2f))
			{
				UpdateTutorial();
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_currentTurn);
		writer.Write(m_hideCommit);
		writer.Write(m_levelName);
		writer.Write(m_endTutorialTurn);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_currentTurn = reader.ReadInt32();
		m_hideCommit = reader.ReadBoolean();
		m_levelName = reader.ReadString();
		m_endTutorialTurn = reader.ReadInt32();
	}

	public static bool IsTutorialActive()
	{
		GameObject gameObject = GameObject.Find("tutorial");
		if ((bool)gameObject)
		{
			return true;
		}
		return false;
	}
}
