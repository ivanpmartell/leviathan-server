using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class ControllerColliderHit
	{
		private CharacterController m_Controller;

		private Collider m_Collider;

		private Vector3 m_Point;

		private Vector3 m_Normal;

		private Vector3 m_MoveDirection;

		private float m_MoveLength;

		private int m_Push;

		public CharacterController controller => m_Controller;

		public Collider collider => m_Collider;

		public Rigidbody rigidbody => m_Collider.attachedRigidbody;

		public GameObject gameObject => m_Collider.gameObject;

		public Transform transform => m_Collider.transform;

		public Vector3 point => m_Point;

		public Vector3 normal => m_Normal;

		public Vector3 moveDirection => m_MoveDirection;

		public float moveLength => m_MoveLength;

		private bool push
		{
			get
			{
				return m_Push != 0;
			}
			set
			{
				m_Push = (value ? 1 : 0);
			}
		}
	}
}
