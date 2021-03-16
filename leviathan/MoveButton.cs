using UnityEngine;

internal class MoveButton
{
	public enum MoveType
	{
		Forward,
		Reverse,
		Rotate
	}

	public MoveType m_moveType;

	public GameObject m_button;

	public MoveButton(MoveType type, GameObject guiCamera, EZDragDropDelegate onDraged)
	{
		m_moveType = type;
		switch (type)
		{
		case MoveType.Forward:
			m_button = GuiUtils.CreateGui("IngameGui/FlowerButtonForward", guiCamera);
			break;
		case MoveType.Reverse:
			m_button = GuiUtils.CreateGui("IngameGui/FlowerButtonReverse", guiCamera);
			break;
		case MoveType.Rotate:
			m_button = GuiUtils.CreateGui("IngameGui/FlowerButtonRotate", guiCamera);
			break;
		}
		m_button.GetComponent<UIButton>().SetDragDropDelegate(onDraged);
	}

	public bool MouseOver()
	{
		return GuiUtils.HasPointerRecursive(UIManager.instance, m_button);
	}

	public void UpdatePosition(float guiScale, Ship ship, Camera guiCamera, Camera gameCamera, ref float lowestScreenPos)
	{
		float num = Mathf.Clamp(guiScale, 1f, FlowerMenu.m_maxGuiScale);
		float num2 = (ship.GetLength() / 2f + 6f) * num;
		Vector3 vector = ship.transform.position + new Vector3(0f, ship.m_deckHeight, 0f);
		Vector3 pos = Vector3.zero;
		switch (m_moveType)
		{
		case MoveType.Forward:
			pos = vector + ship.transform.forward * num2;
			break;
		case MoveType.Reverse:
			pos = vector - ship.transform.forward * num2;
			break;
		case MoveType.Rotate:
			pos = vector + ship.transform.forward * num2 - ship.transform.forward * num * 7f;
			break;
		}
		Vector3 vector2 = GuiUtils.WorldToGuiPos(gameCamera, guiCamera, vector);
		Vector3 vector3 = GuiUtils.WorldToGuiPos(gameCamera, guiCamera, pos);
		m_button.transform.position = vector3;
		if (m_moveType == MoveType.Forward || m_moveType == MoveType.Reverse)
		{
			Vector3 normalized = (vector3 - vector2).normalized;
			Quaternion localRotation = Quaternion.LookRotation(normalized, new Vector3(0f, 0f, -1f));
			m_button.transform.localRotation = localRotation;
		}
		if (m_button.transform.position.y - 19f < lowestScreenPos)
		{
			lowestScreenPos = m_button.transform.position.y - 19f;
		}
	}
}
