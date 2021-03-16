using System;
using System.Collections.Generic;
using UnityEngine;

public class VOCollection : MonoBehaviour
{
	[Serializable]
	public class VOEvent
	{
		public string m_name = string.Empty;

		public GameObject m_effects;

		public bool m_oncePerTurn;
	}

	public List<VOEvent> m_voEvents = new List<VOEvent>();
}
