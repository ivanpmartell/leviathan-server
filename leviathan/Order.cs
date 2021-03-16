using System.IO;
using UnityEngine;

public class Order
{
	public enum Type
	{
		None,
		MoveForward,
		MoveBackward,
		MoveRotate,
		Fire
	}

	public enum FireVisual
	{
		Point,
		Line,
		Area
	}

	public GameObject m_marker;

	public Type m_type;

	public FireVisual m_fireVisual;

	private IOrderable m_owner;

	private Vector3 m_pos = new Vector3(0f, 0f, 0f);

	private int m_targetNetID;

	private bool m_reachedPosition;

	private bool m_haveFacing;

	private Vector3 m_facing;

	private float m_displayRadius;

	private bool m_staticTargetOnly;

	private bool m_blockedLOS;

	private bool m_inFiringCone = true;

	public Order(IOrderable owner, Type type, int targetNetID, Vector3 localPos)
	{
		m_owner = owner;
		m_type = type;
		m_targetNetID = targetNetID;
		m_pos = localPos;
		m_displayRadius = 0f;
	}

	public Order(IOrderable owner, Type type, Vector3 pos)
	{
		m_owner = owner;
		m_type = type;
		m_pos = pos;
	}

	public Order(IOrderable owner, BinaryReader stream)
	{
		m_owner = owner;
		Load(stream);
	}

	public void SetMarkerEnabled(bool enabled, GameObject orderMarkerPrefab)
	{
		if (enabled)
		{
			Enable(orderMarkerPrefab);
		}
		else
		{
			Disable();
		}
	}

	private void Enable(GameObject orderMarkerPrefab)
	{
		if (!(m_marker != null))
		{
			Vector3 pos = GetPos();
			m_marker = Object.Instantiate(orderMarkerPrefab, pos, Quaternion.identity) as GameObject;
			OrderMarker component = m_marker.GetComponent<OrderMarker>();
			component.Setup(this);
		}
	}

	private void Disable()
	{
		if ((bool)m_marker)
		{
			Object.DestroyObject(m_marker);
			m_marker = null;
		}
	}

	public Vector3 GetPos()
	{
		if (m_targetNetID != 0)
		{
			NetObj byID = NetObj.GetByID(m_targetNetID);
			if (byID != null)
			{
				return byID.transform.TransformPoint(m_pos);
			}
		}
		return m_pos;
	}

	public Vector3 GetLocalTargetPos()
	{
		return m_pos;
	}

	public void Save(BinaryWriter stream)
	{
		stream.Write((byte)m_type);
		stream.Write((byte)m_fireVisual);
		stream.Write(m_pos.x);
		stream.Write(m_pos.y);
		stream.Write(m_pos.z);
		stream.Write(m_haveFacing);
		stream.Write(m_facing.x);
		stream.Write(m_facing.y);
		stream.Write(m_facing.z);
		stream.Write(m_displayRadius);
		stream.Write(m_blockedLOS);
		stream.Write(m_inFiringCone);
		stream.Write(m_staticTargetOnly);
		stream.Write(m_reachedPosition);
		stream.Write(m_targetNetID);
	}

	public void Load(BinaryReader stream)
	{
		m_type = (Type)stream.ReadByte();
		m_fireVisual = (FireVisual)stream.ReadByte();
		m_pos.x = stream.ReadSingle();
		m_pos.y = stream.ReadSingle();
		m_pos.z = stream.ReadSingle();
		m_haveFacing = stream.ReadBoolean();
		m_facing.x = stream.ReadSingle();
		m_facing.y = stream.ReadSingle();
		m_facing.z = stream.ReadSingle();
		m_displayRadius = stream.ReadSingle();
		m_blockedLOS = stream.ReadBoolean();
		m_inFiringCone = stream.ReadBoolean();
		m_staticTargetOnly = stream.ReadBoolean();
		m_reachedPosition = stream.ReadBoolean();
		m_targetNetID = stream.ReadInt32();
		if (m_type == Type.MoveRotate)
		{
			Unit unit = GetOwner() as Unit;
			if (unit != null)
			{
				m_pos = unit.transform.position;
			}
		}
	}

	public void SetTarget(Vector3 pos)
	{
		m_targetNetID = 0;
		m_pos = pos;
		m_reachedPosition = false;
		UpdateMarker();
		OnChanged();
	}

	public void SetTarget(int unitID, Vector3 localPos)
	{
		m_targetNetID = unitID;
		m_pos = localPos;
		m_reachedPosition = false;
		UpdateMarker();
		OnChanged();
	}

	private void UpdateMarker()
	{
		if (m_marker != null)
		{
			m_marker.GetComponent<OrderMarker>().OnPositionChanged();
		}
	}

	public bool IsStaticTarget()
	{
		return m_targetNetID == 0;
	}

	public bool IsLOSBlocked()
	{
		return m_blockedLOS;
	}

	public bool IsInFiringCone()
	{
		return m_inFiringCone;
	}

	public void SetInFiringCone(bool inCone)
	{
		m_inFiringCone = inCone;
		if (m_marker != null)
		{
			m_marker.GetComponent<OrderMarker>().OnInFiringConeChanged();
		}
	}

	public void SetLOSBlocked(bool blocked)
	{
		m_blockedLOS = blocked;
	}

	public NetObj GetTargetObj()
	{
		if (m_targetNetID == 0)
		{
			return null;
		}
		return NetObj.GetByID(m_targetNetID);
	}

	public int GetTargetID()
	{
		return m_targetNetID;
	}

	public void SetFacing(Vector3 facing)
	{
		if (!(facing.magnitude < 0.01f))
		{
			facing.y = 0f;
			facing.Normalize();
			m_haveFacing = true;
			m_facing = facing;
			if (m_marker != null)
			{
				m_marker.GetComponent<OrderMarker>().UpdateModel();
			}
			OnChanged();
		}
	}

	public void ResetFacing()
	{
		m_haveFacing = false;
		if (m_marker != null)
		{
			m_marker.GetComponent<OrderMarker>().UpdateModel();
		}
		OnChanged();
	}

	public bool HaveFacing()
	{
		return m_haveFacing;
	}

	public Vector3 GetFacing()
	{
		return m_facing;
	}

	public IOrderable GetOwner()
	{
		return m_owner;
	}

	public GameObject GetMarker()
	{
		return m_marker;
	}

	private void OnChanged()
	{
		m_owner.OnOrdersChanged();
	}

	public void SetDisplayRadius(float radius)
	{
		m_displayRadius = radius;
	}

	public float GetDisplayRadius()
	{
		return m_displayRadius;
	}

	public void SetStaticTargetOnly(bool enabled)
	{
		m_staticTargetOnly = enabled;
	}

	public bool GetStaticTargetOnly()
	{
		return m_staticTargetOnly;
	}

	public bool HasReachedPosition()
	{
		if (m_type == Type.MoveForward || m_type == Type.MoveBackward)
		{
			return m_reachedPosition;
		}
		return true;
	}

	public void SetReachedPosition(bool reached)
	{
		m_reachedPosition = reached;
	}
}
