using UnityEngine;

public class UnitSelectionMarker : MonoBehaviour
{
	public GameObject[] m_corners = new GameObject[4];

	public float m_scaleFactor = 1f;

	public float m_maxScale = 1f;

	public void Setup(Vector3 size, Color color)
	{
		Vector3 vector = size * 0.5f;
		float num = Mathf.Min(m_maxScale, m_scaleFactor * Mathf.Min(size.x, size.z));
		GameObject[] corners = m_corners;
		foreach (GameObject gameObject in corners)
		{
			gameObject.transform.localScale = new Vector3(num, num, num);
		}
		m_corners[0].transform.localPosition = new Vector3(0f - vector.x, 0f, vector.z);
		m_corners[1].transform.localPosition = new Vector3(vector.x, 0f, vector.z);
		m_corners[2].transform.localPosition = new Vector3(0f - vector.x, 0f, 0f - vector.z);
		m_corners[3].transform.localPosition = new Vector3(vector.x, 0f, 0f - vector.z);
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.material.color = color;
		}
	}
}
