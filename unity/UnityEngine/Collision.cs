using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public class Collision
	{
		private Vector3 m_RelativeVelocity;

		private Rigidbody m_Rigidbody;

		private Collider m_Collider;

		private ContactPoint[] m_Contacts;

		public Vector3 relativeVelocity => m_RelativeVelocity;

		public Rigidbody rigidbody => m_Rigidbody;

		public Collider collider => m_Collider;

		public Transform transform => (!(rigidbody != null)) ? collider.transform : rigidbody.transform;

		public GameObject gameObject => (!(m_Rigidbody != null)) ? m_Collider.gameObject : m_Rigidbody.gameObject;

		public ContactPoint[] contacts => m_Contacts;

		public Vector3 impactForceSum => relativeVelocity;

		public Vector3 frictionForceSum => Vector3.zero;

		[Obsolete("Please use Collision.rigidbody, Collision.transform or Collision.collider instead")]
		public Component other => (!(m_Rigidbody != null)) ? ((Component)m_Collider) : ((Component)m_Rigidbody);

		public virtual IEnumerator GetEnumerator()
		{
			return contacts.GetEnumerator();
		}
	}
}
