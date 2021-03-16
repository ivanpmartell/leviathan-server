using System;
using UnityEngine;

[Serializable]
public sealed class LODInstruction : IComparable
{
	public GameObject m_target;

	public float m_minDist;

	public float m_maxDist = 1f;

	public bool m_useAllRenderers = true;

	public LODInstruction()
		: this(null, 0f, 1f)
	{
	}

	public LODInstruction(GameObject target, float minDist, float maxDist)
		: this(target, minDist, maxDist, useAll: true, isPrefab: false)
	{
	}

	public LODInstruction(GameObject target, float minDist, float maxDist, bool useAll, bool isPrefab)
	{
		m_target = target;
		SafeSet_MinDist(minDist);
		SafeSet_MaxDist(maxDist);
		m_useAllRenderers = useAll;
	}

	public int CompareTo(object obj)
	{
		if (obj is LODInstruction)
		{
			return ((obj as LODInstruction).m_minDist > m_minDist) ? (-1) : (((obj as LODInstruction).m_minDist != m_minDist) ? 1 : 0);
		}
		return 1;
	}

	public override bool Equals(object obj)
	{
		if (obj is LODInstruction)
		{
			return (obj as LODInstruction).m_minDist == m_minDist && (obj as LODInstruction).m_maxDist == m_maxDist && (obj as LODInstruction).m_target == m_target;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public void SafeSet_MinDist(float f)
	{
		m_minDist = f;
		if (m_minDist < 0f)
		{
			m_minDist = 0f;
		}
	}

	public void SafeSet_MaxDist(float f)
	{
		m_maxDist = f;
		if (m_maxDist <= m_minDist)
		{
			m_maxDist = m_minDist + 1f;
		}
	}
}
