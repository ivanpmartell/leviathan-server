using UnityEngine;

internal class PRand
{
	private static int m_seed;

	public static int GetSeed()
	{
		return m_seed;
	}

	public static void SetSeed(int seed)
	{
		m_seed = seed;
	}

	public static int Range(int min, int max)
	{
		Random.seed = m_seed;
		int result = Random.Range(min, max);
		m_seed = Random.seed;
		return result;
	}

	public static float Range(float min, float max)
	{
		Random.seed = m_seed;
		float result = Random.Range(min, max);
		m_seed = Random.seed;
		return result;
	}

	public static float Value()
	{
		Random.seed = m_seed;
		float value = Random.value;
		m_seed = Random.seed;
		return value;
	}
}
