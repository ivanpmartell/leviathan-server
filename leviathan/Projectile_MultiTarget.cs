using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Projectiles/Projectile_MultiTarget")]
public class Projectile_MultiTarget : Projectile
{
	protected List<Vector3> targets;

	protected int currentTargetIndex;

	private float m_velocity;

	public float m_acceleration = 10f;

	public float m_turnspeed = 45f;

	public Vector3 CurrentTarget
	{
		get
		{
			if (targets == null || targets.Count == 0)
			{
				return base.transform.position;
			}
			if (targets.Count > currentTargetIndex)
			{
				return targets[currentTargetIndex];
			}
			Debug.LogWarning($"Projectile_MultiTarget.CurrentTarget can not be found, invalid currentTargetIndex of {currentTargetIndex}");
			return base.transform.position;
		}
		private set
		{
			if (targets.Count > currentTargetIndex)
			{
				targets[currentTargetIndex] = value;
			}
			else
			{
				targets.Add(value);
			}
		}
	}

	public Vector3 NextTarget
	{
		get
		{
			if (targets.Count > currentTargetIndex + 1)
			{
				return targets[currentTargetIndex + 1];
			}
			Debug.LogWarning($"Trying to get next pos. current = {currentTargetIndex}, next = {currentTargetIndex + 1}, count = {targets.Count}");
			return CurrentTarget;
		}
	}

	public Projectile_MultiTarget()
	{
		targets = new List<Vector3>();
	}

	public Projectile_MultiTarget(List<Vector3> inTargets)
	{
		foreach (Vector3 inTarget in inTargets)
		{
			AddTarget(inTarget);
		}
	}

	public void Start()
	{
		base.transform.LookAt(NextTarget);
	}

	public void AddTarget(GameObject inTarget)
	{
		AddTarget(inTarget.transform.position);
	}

	public void AddTarget(Vector3 inTarget)
	{
		if (targets == null)
		{
			targets = new List<Vector3>();
		}
		targets.Add(inTarget);
	}

	public void ClearTargets()
	{
		if (targets != null)
		{
			targets.Clear();
		}
	}

	public void SmoothPath(int numNew)
	{
		SmoothPath(numNew, Vector3.zero);
	}

	protected static List<Vector3> AddPointsBetween(Vector3 start, Vector3 end, int numNew, Vector3 randomAmmount)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3 vector = end - start;
		Vector3 normalized = vector.normalized;
		float num = vector.magnitude * (1f / (float)numNew);
		for (int i = 0; i < numNew; i++)
		{
			Vector3 item = start + normalized * (num * (float)i);
			float x = (Random.value - 0.5f) * randomAmmount.x;
			float y = (Random.value - 0.5f) * randomAmmount.y;
			float z = (Random.value - 0.5f) * randomAmmount.z;
			item += new Vector3(x, y, z);
			list.Add(item);
		}
		return list;
	}

	public void SmoothPath(int numNew, Vector3 randomAmmount)
	{
		if (currentTargetIndex >= targets.Count - 1)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		if (currentTargetIndex > 0)
		{
			for (int i = 0; i < currentTargetIndex; i++)
			{
				list.Add(targets[i]);
			}
		}
		for (int j = currentTargetIndex; j < targets.Count - 2; j++)
		{
			Vector3 vector = targets[j];
			list.Add(vector);
			Vector3 end = targets[j + 1];
			list.AddRange(AddPointsBetween(vector, end, numNew, randomAmmount));
		}
		list.AddRange(AddPointsBetween(targets[targets.Count - 2], targets[targets.Count - 1], numNew, randomAmmount));
		list.Add(targets[targets.Count - 1]);
		targets.Clear();
		targets.AddRange(list);
		list.Clear();
	}

	protected override void SpecializedFixedUpdate()
	{
		if (targets == null || targets.Count == 0)
		{
			return;
		}
		m_velocity += m_acceleration * Time.fixedDeltaTime;
		if (m_velocity > m_originalPower)
		{
			m_velocity = m_originalPower;
		}
		Vector3 vector = CurrentTarget - base.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		Vector3 vector2 = Vector3.Normalize(vector);
		float num = m_velocity * Time.fixedDeltaTime;
		base.transform.position += vector2 * num;
		Quaternion to = Quaternion.LookRotation(vector);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, m_turnspeed * Time.fixedDeltaTime);
		if (sqrMagnitude <= 5f + Time.fixedDeltaTime)
		{
			currentTargetIndex++;
			if (currentTargetIndex >= targets.Count - 1)
			{
				HitEffect(base.transform.position, HitType.Normal);
				DoSplashDamage(base.transform.position, null, m_damage);
				Object.Destroy(base.gameObject);
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_velocity);
		if (targets == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(targets.Count);
		}
		if (targets != null && targets.Count > 0)
		{
			foreach (Vector3 target in targets)
			{
				writer.Write(target.x);
				writer.Write(target.y);
				writer.Write(target.z);
			}
		}
		writer.Write(currentTargetIndex);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_velocity = reader.ReadSingle();
		int num = reader.ReadInt32();
		targets = new List<Vector3>();
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				float z = reader.ReadSingle();
				targets.Add(new Vector3(x, y, z));
			}
		}
		currentTargetIndex = reader.ReadInt32();
	}
}
