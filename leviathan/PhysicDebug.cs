using UnityEngine;

public class PhysicDebug : MonoBehaviour
{
	private void Awake()
	{
	}

	private void OnCollisionStay(Collision collisionInfo)
	{
		ContactPoint[] contacts = collisionInfo.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			PLog.Log("OnCollisionStay: " + base.gameObject.name);
			if (collisionInfo.rigidbody == null)
			{
				Debug.DrawRay(contactPoint.point, contactPoint.normal * 10f, Color.white, 50f, depthTest: false);
			}
		}
	}

	public static void SetScripts()
	{
		Collider[] array = Object.FindObjectsOfType(typeof(Collider)) as Collider[];
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.GetComponent<PhysicDebug>() == null)
			{
				collider.gameObject.AddComponent<PhysicDebug>();
			}
		}
		Rigidbody[] array3 = Object.FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[];
		Rigidbody[] array4 = array3;
		foreach (Rigidbody rigidbody in array4)
		{
			if (rigidbody.GetComponent<PhysicDebug>() == null)
			{
				rigidbody.gameObject.AddComponent<PhysicDebug>();
			}
		}
	}
}
