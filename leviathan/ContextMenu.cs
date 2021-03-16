using System.Collections.Generic;
using UnityEngine;

public class ContextMenu
{
	private enum ButtonType
	{
		DragButton,
		ClickButton,
		ToggleButton
	}

	private class ButtonData
	{
		public ButtonType m_type;

		public Texture m_image;

		public string m_tooltip;

		public Vector3 m_pos;

		public ButtonHandler m_handler;

		public bool m_small;
	}

	public delegate void ButtonHandler();

	private const float m_buttonSize = 40f;

	private static int lastFrameHack;

	private float m_mouseOver = 10f;

	private int m_grabbed = -1;

	private Vector2 m_grabPoint;

	private bool m_firstFrameHack = true;

	private List<ButtonData> m_buttons = new List<ButtonData>();

	private Camera m_guiCamera;

	public ContextMenu(Camera guiCamera)
	{
		m_guiCamera = guiCamera;
	}

	public void AddDragButton(Texture image, string tooltip, Vector3 pos, ButtonHandler handler)
	{
		ButtonData buttonData = new ButtonData();
		buttonData.m_type = ButtonType.DragButton;
		buttonData.m_image = image;
		buttonData.m_tooltip = tooltip;
		buttonData.m_pos = pos;
		buttonData.m_handler = handler;
		m_buttons.Add(buttonData);
	}

	public void AddClickButton(Texture image, string tooltip, Vector3 pos, bool small, ButtonHandler handler)
	{
		ButtonData buttonData = new ButtonData();
		buttonData.m_type = ButtonType.ClickButton;
		buttonData.m_image = image;
		buttonData.m_tooltip = tooltip;
		buttonData.m_pos = pos;
		buttonData.m_handler = handler;
		buttonData.m_small = small;
		m_buttons.Add(buttonData);
	}

	public void DrawGui(Camera camera)
	{
		float num = m_guiCamera.orthographicSize * 2f;
		float num2 = (float)Screen.height / num;
		bool flag = lastFrameHack != Time.frameCount;
		lastFrameHack = Time.frameCount;
		bool flag2 = false;
		Vector2 vector = Utils.ScreenToGUIPos(Input.mousePosition);
		for (int i = 0; i < m_buttons.Count; i++)
		{
			ButtonData buttonData = m_buttons[i];
			float num3 = ((!buttonData.m_small) ? 40f : 26.666666f) * num2;
			Vector2 vector2 = Utils.ScreenToGUIPos(camera.WorldToScreenPoint(buttonData.m_pos)) - new Vector2(num3 / 2f, num3 / 2f);
			Rect position = new Rect(vector2.x, vector2.y, num3, num3);
			if (buttonData.m_type == ButtonType.DragButton)
			{
				GUI.DrawTexture(position, buttonData.m_image);
				if (position.Contains(vector))
				{
					m_mouseOver = 0f;
					GUI.Label(new Rect(vector2.x - 20f, vector2.y - 30f, 85f, 20f), buttonData.m_tooltip);
					if (Input.GetMouseButton(0))
					{
						flag2 = true;
						if (m_grabbed != i)
						{
							m_grabbed = i;
							m_grabPoint = vector;
						}
					}
				}
			}
			if (buttonData.m_type != ButtonType.ClickButton)
			{
				continue;
			}
			GUI.DrawTexture(position, buttonData.m_image);
			if (position.Contains(vector))
			{
				m_mouseOver = 0f;
				GUI.Label(new Rect(vector2.x - 20f, vector2.y - 30f, 85f, 20f), buttonData.m_tooltip);
				if (Input.GetMouseButtonUp(0) && flag && !m_firstFrameHack)
				{
					buttonData.m_handler();
				}
			}
		}
		if (m_grabbed != -1 && Vector2.Distance(m_grabPoint, vector) > 5f)
		{
			m_buttons[m_grabbed].m_handler();
		}
		if (!flag2)
		{
			m_grabbed = -1;
		}
		m_firstFrameHack = false;
	}

	public void Update(float dt)
	{
		m_mouseOver += dt;
	}

	public bool IsMouseOver()
	{
		return (double)m_mouseOver < 0.1;
	}
}
