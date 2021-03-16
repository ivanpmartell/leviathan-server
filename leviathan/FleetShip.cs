using PTech;
using UnityEngine;

public class FleetShip
{
	public Vector3 m_basePosition = default(Vector3);

	public Vector3 m_shipPosition = default(Vector3);

	public int m_cost;

	public ShipDef m_definition;

	public string m_name = "Undefined";

	public GameObject m_ship;

	public GameObject m_floatingInfo;

	public float m_width;

	public float m_length;

	public int m_maxHardpoints;

	public void Destroy()
	{
		if ((bool)m_ship)
		{
			Object.Destroy(m_ship);
		}
		if ((bool)m_floatingInfo)
		{
			Object.Destroy(m_floatingInfo);
		}
		m_ship = null;
		m_floatingInfo = null;
	}

	public void Update(GameObject sceneCamera, GameObject guiCamera)
	{
		if (!(m_floatingInfo == null))
		{
			Vector3 position = sceneCamera.camera.WorldToScreenPoint(m_basePosition);
			Vector3 position2 = guiCamera.camera.ScreenToWorldPoint(position);
			position2.z = -2f;
			m_floatingInfo.transform.position = position2;
			GuiUtils.FindChildOf(m_floatingInfo, "FloatingInfoCostLabel").GetComponent<SpriteText>().Text = Localize.instance.Translate(m_cost + " $label_pointssmall");
			GuiUtils.FindChildOf(m_floatingInfo, "FloatingInfoNameLabel").GetComponent<SpriteText>().Text = m_definition.m_name;
			string text = m_definition.NumberOfHardpoints() + "/" + m_maxHardpoints + " " + Localize.instance.Translate("$arms");
			GuiUtils.FindChildOf(m_floatingInfo, "FloatingInfoArmsLabel").GetComponent<SpriteText>().Text = text;
		}
	}
}
