using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LOD_DEBUGGER : MonoBehaviour
{
	public enum ExtendDir_X
	{
		Left,
		Right
	}

	public enum ExtendDir_Z
	{
		Up,
		Down
	}

	public ExtendDir_X m_extendDir_X = ExtendDir_X.Right;

	public ExtendDir_Z m_extendDir_Z = ExtendDir_Z.Down;

	public float m_padding = 15f;

	public int m_ObjectsPerRow = 3;

	public int m_repeatCollection = 1;

	[SerializeField]
	public List<GameObject> m_prefabs;

	private void Start()
	{
		if (m_prefabs == null || m_prefabs.Count == 0)
		{
			return;
		}
		float num = ((m_extendDir_X != ExtendDir_X.Right) ? (-1f) : 1f);
		float num2 = ((m_extendDir_Z != 0) ? (-1f) : 1f);
		Vector3 position = base.transform.position;
		Vector3 position2 = position;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < m_repeatCollection; i++)
		{
			for (int j = 0; j < m_prefabs.Count; j++)
			{
				if (num3 == m_ObjectsPerRow)
				{
					num4++;
					num3 = 0;
					position2 = position + new Vector3(0f, 0f, m_padding * (float)num4 * num2);
				}
				UnityEngine.Object.Instantiate(m_prefabs[j], position2, Quaternion.identity);
				position2 += new Vector3(m_padding * num, 0f, 0f);
				num3++;
			}
		}
	}

	private void Update()
	{
	}
}
