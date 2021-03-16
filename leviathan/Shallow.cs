using UnityEngine;

public class Shallow : MonoBehaviour
{
	private void OnTriggerStay(Collider other)
	{
		if (other.attachedRigidbody != null)
		{
			Ship component = other.attachedRigidbody.GetComponent<Ship>();
			if (!(component != null))
			{
			}
		}
	}
}
