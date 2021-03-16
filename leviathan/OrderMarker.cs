using System;
using UnityEngine;

public class OrderMarker : MonoBehaviour
{
	public GameObject m_selectionMarker;

	public GameObject m_moveForwardMarker;

	public GameObject m_moveReverseMarker;

	public GameObject m_firePointMarker;

	public GameObject m_fireAreaMarker;

	public GameObject m_fireLineMarker;

	public GameObject m_lookMarker;

	private Material[] m_originalMaterial;

	private Renderer m_activeMarker;

	private Order m_order;

	private int m_lineID = -1;

	private void Start()
	{
	}

	public void Setup(Order order)
	{
		m_order = order;
		UpdateModel();
	}

	public void Update()
	{
		Camera camera = Camera.main;
		if (camera == null)
		{
			return;
		}
		float displayRadius = m_order.GetDisplayRadius();
		if (displayRadius != 0f)
		{
			LineDrawer component = camera.GetComponent<LineDrawer>();
			if (component != null)
			{
				if (m_lineID == -1)
				{
					m_lineID = component.GetTypeID("attackArea");
				}
				component.DrawXZCircle(base.transform.position + new Vector3(0f, 3f, 0f), displayRadius, 40, m_lineID, 0.15f);
			}
		}
		float num = Vector3.Distance(camera.transform.position, base.transform.position);
		float num2 = Mathf.Tan((float)Math.PI / 180f * camera.fieldOfView * 0.5f) * 0.04f * num;
		if (m_order.m_type != Order.Type.MoveRotate)
		{
			num2 *= 0.8f;
		}
		base.transform.localScale = new Vector3(num2, num2, num2);
	}

	public void UpdateModel()
	{
		m_lookMarker.SetActiveRecursively(state: false);
		m_selectionMarker.SetActiveRecursively(state: false);
		m_moveForwardMarker.SetActiveRecursively(state: false);
		m_moveReverseMarker.SetActiveRecursively(state: false);
		m_firePointMarker.SetActiveRecursively(state: false);
		m_fireAreaMarker.SetActiveRecursively(state: false);
		m_fireLineMarker.SetActiveRecursively(state: false);
		if (m_order.m_type == Order.Type.MoveForward || m_order.m_type == Order.Type.MoveBackward || m_order.m_type == Order.Type.MoveRotate)
		{
			if (m_order.m_type == Order.Type.MoveForward)
			{
				m_moveForwardMarker.SetActiveRecursively(state: true);
			}
			else if (m_order.m_type == Order.Type.MoveBackward)
			{
				m_moveReverseMarker.SetActiveRecursively(state: true);
			}
			if (m_order.HaveFacing())
			{
				m_lookMarker.SetActiveRecursively(state: true);
				m_lookMarker.transform.rotation = Quaternion.LookRotation(m_order.GetFacing(), new Vector3(0f, 1f, 0f));
			}
		}
		else if (m_order.m_type == Order.Type.Fire)
		{
			switch (m_order.m_fireVisual)
			{
			case Order.FireVisual.Point:
				m_firePointMarker.SetActiveRecursively(state: true);
				m_activeMarker = m_firePointMarker.renderer;
				break;
			case Order.FireVisual.Line:
				m_fireLineMarker.SetActiveRecursively(state: true);
				UpdateFireLineMarker();
				m_activeMarker = m_fireLineMarker.renderer;
				break;
			case Order.FireVisual.Area:
				m_fireAreaMarker.SetActiveRecursively(state: true);
				m_activeMarker = m_fireAreaMarker.renderer;
				break;
			}
		}
	}

	public void OnInFiringConeChanged()
	{
		UpdateMaterial();
	}

	private void UpdateMaterial()
	{
		if (m_activeMarker == null)
		{
			return;
		}
		if (m_order.IsInFiringCone())
		{
			if (m_originalMaterial != null)
			{
				m_activeMarker.materials = m_originalMaterial;
				m_originalMaterial = null;
			}
		}
		else if (m_originalMaterial == null)
		{
			m_originalMaterial = m_activeMarker.sharedMaterials;
			m_activeMarker.materials[0].color = Color.red;
		}
	}

	private void UpdateFireLineMarker()
	{
		NetObj netObj = m_order.GetOwner() as NetObj;
		Vector3 normalized = (base.transform.position - netObj.transform.position).normalized;
		m_fireLineMarker.transform.rotation = Quaternion.LookRotation(normalized, new Vector3(0f, 1f, 0f));
	}

	public Order GetOrder()
	{
		return m_order;
	}

	public void SetSelected(bool selected)
	{
		m_selectionMarker.SetActiveRecursively(selected);
	}

	public void OnPositionChanged()
	{
		base.transform.position = m_order.GetPos();
		if (m_order.m_type == Order.Type.Fire && m_order.m_fireVisual == Order.FireVisual.Line)
		{
			UpdateFireLineMarker();
		}
	}
}
