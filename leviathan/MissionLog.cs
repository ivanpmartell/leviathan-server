using UnityEngine;

public class MissionLog
{
	public GameObject m_gui;

	public MissionLog(GameObject gui, GameObject guiCam)
	{
		m_gui = gui;
		SetupGui(guiCam);
	}

	public void Close()
	{
	}

	public void Hide()
	{
	}

	public void Show()
	{
	}

	public void Toggle()
	{
	}

	private void SetupGui(GameObject guiCam)
	{
		m_gui = GuiUtils.FindChildOf(m_gui.transform, "ObjectivesContainer");
	}

	public void Update(Camera camera, float dt)
	{
		if (!m_gui.active)
		{
			return;
		}
		int num = 1;
		GuiUtils.FindChildOf(m_gui.transform, "Line1").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "Line2").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "Line3").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "Line4").SetActiveRecursively(state: false);
		GuiUtils.FindChildOf(m_gui.transform, "Line5").SetActiveRecursively(state: false);
		UIScrollList component = GuiUtils.FindChildOf(m_gui.transform, "PrimaryObjectivesScrollist").GetComponent<UIScrollList>();
		component.ClearList(destroy: true);
		foreach (TurnMan.MissionObjective missionObjective in TurnMan.instance.m_missionObjectives)
		{
			if (missionObjective.m_status != 0)
			{
				GameObject gameObject = ((missionObjective.m_status == MNAction.ObjectiveStatus.Active) ? GuiUtils.CreateGui("Briefing/PrimaryObjectivesListItem", camera.gameObject) : ((missionObjective.m_status != MNAction.ObjectiveStatus.Done) ? GuiUtils.CreateGui("Briefing/PrimaryObjectivesListItem", camera.gameObject) : GuiUtils.CreateGui("Briefing/PrimaryObjectivesListItem_Done", camera.gameObject)));
				GuiUtils.FindChildOf(gameObject.transform, "PrimaryObjectivesText").GetComponent<SpriteText>().Text = Localize.instance.Translate("$" + missionObjective.m_text);
				component.AddItem(gameObject);
			}
		}
	}
}
