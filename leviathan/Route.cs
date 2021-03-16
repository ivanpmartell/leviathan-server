using System;
using System.Collections.Generic;
using UnityEngine;

public class Route
{
	public class Waypoint
	{
		public Vector3 m_pos;

		public Vector3 m_direction;

		public bool m_havePosition;

		public bool m_haveDirection;

		public bool m_reverse;

		public float m_time;

		public float m_speed;

		public Waypoint(Vector3 point, Vector3 direction, bool reverse, bool havePosition, bool haveDirection)
		{
			m_pos = point;
			m_direction = direction;
			m_reverse = reverse;
			m_haveDirection = haveDirection;
			m_havePosition = havePosition;
		}

		public void Update(Waypoint wp)
		{
			m_pos = wp.m_pos;
			m_direction = wp.m_direction;
			m_haveDirection = wp.m_haveDirection;
			m_havePosition = wp.m_havePosition;
			m_reverse = wp.m_reverse;
		}
	}

	private struct PathNode
	{
		public Vector3 point;

		public float time;
	}

	private List<Waypoint> m_waypoints = new List<Waypoint>();

	private float m_totalLength;

	public static bool m_drawGui = true;

	private float m_pathUpdateTimer = -1f;

	private List<PathNode> m_predictedRoute;

	private List<Vector3> m_closePoints;

	private List<Vector3> m_farPoints;

	public void SetWaypoints(List<Waypoint> newWPs)
	{
		if (m_waypoints.Count != newWPs.Count)
		{
			m_waypoints = newWPs;
		}
		else
		{
			for (int i = 0; i < m_waypoints.Count; i++)
			{
				m_waypoints[i].Update(newWPs[i]);
			}
		}
		UpdateLength();
		MakeDirtyPath();
	}

	public void PopWaypoint()
	{
		if (m_waypoints.Count > 0)
		{
			m_waypoints.RemoveAt(0);
		}
		MakeDirtyPath();
	}

	private void MakeDirtyPath()
	{
		if (m_pathUpdateTimer < 0f)
		{
			m_pathUpdateTimer = 0f;
		}
	}

	public float GetTotalLength()
	{
		return m_totalLength;
	}

	public int NrOfWaypoints()
	{
		return m_waypoints.Count;
	}

	public Waypoint GetNextWaypoint()
	{
		if (m_waypoints.Count > 0)
		{
			return m_waypoints[0];
		}
		return null;
	}

	private void UpdateLength()
	{
		m_totalLength = CalculateTotalLength();
	}

	public void OnGUI()
	{
		if (!m_drawGui)
		{
			return;
		}
		foreach (Waypoint waypoint in m_waypoints)
		{
			if (waypoint.m_havePosition)
			{
				Vector2 vector = Utils.ScreenToGUIPos(Camera.main.WorldToScreenPoint(waypoint.m_pos));
				if (!(waypoint.m_time >= 100f))
				{
					GUI.Label(text: (!(waypoint.m_time < 10f)) ? ((int)waypoint.m_time).ToString() : (" " + (int)waypoint.m_time), position: new Rect(vector.x - 8f, vector.y - 11f, 100f, 30f));
				}
			}
		}
	}

	public void Draw(Vector3 firstPoint, LineDrawer lineDrawer, int materialID, int predictMaterialID, int predictCloseMaterialID, Vector3 initialPos, Quaternion initialRot, Vector3 initialVel, float initialRotVel, float maxTime, float stepSize, float width, float maxTurnSpeed, float maxSpeed, float maxReverseSpeed, float acceleration, float reverseAcceleration, float breakAcceleration, float forwardFriction, float sidewayFriction, float rotationFriction)
	{
		if (m_waypoints.Count == 0)
		{
			return;
		}
		if (m_pathUpdateTimer >= 0f)
		{
			m_pathUpdateTimer -= Time.deltaTime;
			if (m_pathUpdateTimer < 0f)
			{
				GeneratePredictedPath(initialPos, initialRot, initialVel, initialRotVel, maxTime, stepSize, width, maxTurnSpeed, maxSpeed, maxReverseSpeed, acceleration, reverseAcceleration, breakAcceleration, forwardFriction, sidewayFriction, rotationFriction, lineDrawer);
				GenerateLines();
			}
		}
		Vector3 start = firstPoint;
		foreach (Waypoint waypoint in m_waypoints)
		{
			Vector3 pos = waypoint.m_pos;
			pos.y += 1f;
			lineDrawer.DrawLine(start, pos, materialID, 0.2f);
			start = pos;
		}
		if (m_closePoints != null)
		{
			lineDrawer.DrawLine(m_closePoints, predictCloseMaterialID, 3f);
		}
		if (m_farPoints != null)
		{
			lineDrawer.DrawLine(m_farPoints, predictMaterialID, 3f);
		}
	}

	private void GenerateLines()
	{
		if (m_closePoints == null)
		{
			m_closePoints = new List<Vector3>();
		}
		else
		{
			m_closePoints.Clear();
		}
		if (m_farPoints == null)
		{
			m_farPoints = new List<Vector3>();
		}
		else
		{
			m_farPoints.Clear();
		}
		for (int i = 0; i < m_predictedRoute.Count; i += 10)
		{
			PathNode pathNode = m_predictedRoute[i];
			if (pathNode.time < 10f)
			{
				m_closePoints.Add(pathNode.point);
				continue;
			}
			if (m_farPoints.Count == 0)
			{
				m_closePoints.Add(pathNode.point);
			}
			m_farPoints.Add(pathNode.point);
		}
	}

	private bool GetRotVelTowards(ref float rotVel, Vector3 dir, bool reverse, Quaternion realRot, float maxTurnSpeed)
	{
		float y = Quaternion.LookRotation(dir).eulerAngles.y;
		float num = realRot.eulerAngles.y;
		if (reverse)
		{
			num += 180f;
		}
		float num2 = Mathf.DeltaAngle(num, y);
		float num3 = Mathf.Clamp(num2, -85f, 85f) / 85f;
		rotVel += num3 * 40f * Time.fixedDeltaTime;
		if (Mathf.Abs(rotVel) > maxTurnSpeed)
		{
			rotVel = ((!(rotVel > 0f)) ? (0f - maxTurnSpeed) : maxTurnSpeed);
		}
		return Mathf.Abs(num2) < 2f && Mathf.Abs(rotVel) < 1f;
	}

	private bool ReachedWP(Vector3 realPos, Vector3 forward, Vector3 right, Vector3 wpPos, float width)
	{
		Vector3 rhs = wpPos - realPos;
		float f = Vector3.Dot(forward, rhs);
		if (Mathf.Abs(f) > 4f)
		{
			return false;
		}
		float f2 = Vector3.Dot(right, rhs);
		return Mathf.Abs(f2) < width / 2f;
	}

	private void GeneratePredictedPath(Vector3 initialPos, Quaternion initialRot, Vector3 initialVel, float initialRotVel, float maxTime, float stepSize, float width, float maxTurnSpeed, float maxSpeed, float maxReverseSpeed, float acceleration, float reverseAcceleration, float breakAcceleration, float forwardFriction, float sideWayFriction, float rotationFriction, LineDrawer lineDrawer)
	{
		if (m_predictedRoute != null)
		{
			m_predictedRoute.Clear();
		}
		else
		{
			m_predictedRoute = new List<PathNode>();
		}
		int count = m_waypoints.Count;
		int num = 0;
		float num2 = 0f;
		bool flag = false;
		Vector3 vector = initialPos;
		Quaternion q = initialRot;
		Vector3 vector2 = initialVel;
		float rotVel = initialRotVel;
		bool flag2 = false;
		while (num < count && num2 < maxTime)
		{
			Vector3 vector3 = q * Vector3.forward;
			Vector3 vector4 = q * Vector3.right;
			float num3 = Vector3.Dot(vector3, vector2);
			Waypoint waypoint = m_waypoints[num];
			flag = waypoint.m_reverse;
			float num4 = 0f;
			if (waypoint.m_havePosition)
			{
				if (!flag2)
				{
					if (ReachedWP(vector, vector3, vector4, waypoint.m_pos, width))
					{
						flag2 = true;
					}
					float speedFactor = GetSpeedFactor(num, vector, vector3, vector4, breakAcceleration, maxTurnSpeed, maxSpeed, num3);
					num4 = ((!flag) ? (speedFactor * maxSpeed) : (speedFactor * (0f - maxReverseSpeed)));
				}
			}
			else
			{
				flag2 = true;
			}
			Vector3 dir = ((!flag2 || !waypoint.m_haveDirection) ? (waypoint.m_pos - vector).normalized : waypoint.m_direction);
			bool rotVelTowards = GetRotVelTowards(ref rotVel, dir, waypoint.m_reverse, q, maxTurnSpeed);
			if (!flag)
			{
				if (num4 > num3)
				{
					vector2 += vector3 * acceleration * stepSize;
				}
				else if (num4 < num3)
				{
					vector2 -= vector3 * breakAcceleration * stepSize;
				}
			}
			else if (num4 < num3)
			{
				vector2 += vector3 * (0f - reverseAcceleration) * stepSize;
			}
			else if (num4 > num3)
			{
				vector2 -= vector3 * (0f - breakAcceleration) * stepSize;
			}
			Vector3 vector5 = Utils.Project(vector2, vector3);
			Vector3 vector6 = Utils.Project(vector2, vector4);
			vector2 -= vector5 * forwardFriction;
			vector2 -= vector6 * sideWayFriction;
			rotVel -= rotVel * rotationFriction;
			vector += vector2 * stepSize;
			q *= Quaternion.Euler(new Vector3(0f, rotVel * stepSize, 0f));
			Utils.NormalizeQuaternion(ref q);
			vector2.y = 0f;
			vector.y = 0f;
			if (flag2 && (!waypoint.m_haveDirection || rotVelTowards))
			{
				waypoint.m_time = num2;
				waypoint.m_speed = vector2.magnitude;
				num++;
				flag2 = false;
			}
			PathNode item = default(PathNode);
			item.point = vector;
			item.point.y += 0.2f;
			item.time = num2;
			m_predictedRoute.Add(item);
			num2 += stepSize;
		}
	}

	private float CalculateTotalLength()
	{
		float num = 0f;
		for (int i = 0; i < m_waypoints.Count - 1; i++)
		{
			float num2 = Vector3.Distance(m_waypoints[i].m_pos, m_waypoints[i + 1].m_pos);
			num += num2;
		}
		return num;
	}

	private float GetTurnTime(Vector3 currentDir, Vector3 targetDir, float maxTurnSpeed, bool reverse)
	{
		if (reverse)
		{
			currentDir = -currentDir;
		}
		float num = Vector3.Angle(currentDir, targetDir);
		return num / maxTurnSpeed;
	}

	private float GetEta(float distance, float forwardSpeed, bool reverse)
	{
		if (reverse)
		{
			return (!(forwardSpeed < 0f)) ? 0f : (distance / (0f - forwardSpeed));
		}
		return (!(forwardSpeed > 0f)) ? 0f : (distance / forwardSpeed);
	}

	public List<Vector3> GetPositions()
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < m_waypoints.Count; i++)
		{
			list.Add(m_waypoints[i].m_pos);
		}
		return list;
	}

	public float GetSpeedFactor(int startWP, Vector3 currentPos, Vector3 currentDir, Vector3 right, float breakAcceleration, float maxTurnSpeed, float maxForwardSpeed, float currentForwardSpeed)
	{
		if (Mathf.Abs(currentForwardSpeed) < 0.5f)
		{
			return 1f;
		}
		bool reverse = m_waypoints[0].m_reverse;
		float num = ((!reverse) ? currentForwardSpeed : (0f - currentForwardSpeed));
		Vector3 normalized = (m_waypoints[startWP].m_pos - currentPos).normalized;
		float distance = Vector3.Distance(currentPos, m_waypoints[startWP].m_pos);
		float turnTime = GetTurnTime(currentDir, normalized, maxTurnSpeed, reverse);
		float eta = GetEta(distance, currentForwardSpeed, reverse);
		if (IsPointInsideTurnRadius(num + 1f, maxTurnSpeed * 0.5f, currentPos, right, m_waypoints[startWP].m_pos, null))
		{
			return 0f;
		}
		Vector3 a = currentPos;
		float num2 = 0f;
		for (int i = startWP; i < m_waypoints.Count; i++)
		{
			Waypoint waypoint = m_waypoints[i];
			num2 += Vector3.Distance(a, waypoint.m_pos);
			if (i == m_waypoints.Count - 1)
			{
				float eta2 = GetEta(num2, currentForwardSpeed, reverse);
				float num3 = Mathf.Sqrt(num2 / breakAcceleration);
				if (num3 > eta2)
				{
					return 0f;
				}
			}
			else
			{
				Waypoint waypoint2 = m_waypoints[i + 1];
				Vector3 normalized2 = (waypoint2.m_pos - waypoint.m_pos).normalized;
				if (IsPointInsideTurnRadius(right: new Vector3(normalized2.z, 0f, 0f - normalized2.x), vel: num + 1f, turnVel: maxTurnSpeed * 0.5f, center: waypoint.m_pos, point: waypoint2.m_pos, lineDrawer: null))
				{
					return 0f;
				}
			}
			a = waypoint.m_pos;
		}
		return 1f;
	}

	private bool IsPointInsideTurnRadius(float vel, float turnVel, Vector3 center, Vector3 right, Vector3 point, LineDrawer lineDrawer)
	{
		float num = TurnDiameter(vel, turnVel);
		float num2 = num;
		Vector3 vector = center + right * num2;
		Vector3 vector2 = center - right * num2;
		if (lineDrawer != null)
		{
			lineDrawer.DrawXZCircle(vector, num2, 30, 1, 0.1f);
			lineDrawer.DrawXZCircle(vector2, num2, 30, 2, 0.1f);
		}
		if (Vector3.Distance(vector, point) < num2)
		{
			return true;
		}
		if (Vector3.Distance(vector2, point) < num2)
		{
			return true;
		}
		return false;
	}

	public bool IsReverse()
	{
		if (m_waypoints.Count == 0)
		{
			return false;
		}
		return m_waypoints[0].m_reverse;
	}

	private float TurnDiameter(float vel, float maxTurnSpeed)
	{
		float f = (float)Math.PI / 180f * maxTurnSpeed;
		return vel / Mathf.Tan(f);
	}
}
