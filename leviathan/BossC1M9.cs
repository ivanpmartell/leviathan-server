using System;
using System.IO;
using UnityEngine;

public class BossC1M9 : NetObj
{
	private bool b_startOfTurn = true;

	private bool m_isHeadOpen;

	private bool m_isJawOpen;

	private bool m_toggleHead;

	private bool m_toggleJaw;

	private bool m_fireCannon;

	private int m_bossTurn = -1;

	private float m_timeInTurn;

	private Vector3 m_cannonTarget = default(Vector3);

	private float m_cannonAngle;

	private float m_beamAngle;

	private Platform m_weaponPlatform;

	private Platform m_weakspotPlatform;

	private GameObject m_bossForehead;

	private GameObject m_bossJaw;

	public GameObject m_rayPrefab;

	private GameObject m_rayVisualizer;

	public int m_cannonDamage = 250;

	public int m_cannonArmorPiercing = 25;

	private float m_rayVisTime;

	private Vector3 m_rayTargetPos;

	public float m_rayWidth = 1f;

	public float m_rayFadeTime = 0.5f;

	public GameObject m_muzzleEffect;

	public GameObject m_muzzleEffectLow;

	public GameObject m_hitEffectLowPrefab;

	public GameObject m_hitEffectHiPrefab;

	public override void Awake()
	{
		base.Awake();
	}

	public void Start()
	{
		m_bossForehead = GameObject.Find("BossC1M9_forehead").transform.GetChild(0).gameObject;
		m_bossJaw = GameObject.Find("BossC1M9_jaw").transform.GetChild(0).gameObject;
		m_weaponPlatform = GameObject.Find("bossc1m9_platform").GetComponent<Platform>();
		m_weakspotPlatform = GameObject.Find("bossc1m9_weakspot").GetComponent<Platform>();
		StateSetup();
	}

	private void StateSetup()
	{
		if (m_isHeadOpen)
		{
			m_bossForehead.animation.Play("opened_forehead_idle");
			m_weakspotPlatform.m_immuneToDamage = false;
			m_weakspotPlatform.m_allowAutotarget = true;
		}
		else
		{
			m_bossForehead.animation.Play("closed_forehead_idle");
			m_weakspotPlatform.m_immuneToDamage = true;
			m_weakspotPlatform.m_allowAutotarget = false;
		}
		if (m_isJawOpen)
		{
			m_bossJaw.animation.Play("open_jaw_idle");
		}
		else
		{
			m_bossJaw.animation.Play("closed_jaw_idle");
		}
	}

	public void Update()
	{
		if (!NetObj.m_simulating)
		{
		}
	}

	private string StateDebug()
	{
		string empty = string.Empty;
		empty = empty + " Phase: " + m_bossTurn;
		string text = empty;
		empty = text + " HeadOpen: " + m_isHeadOpen + "/" + m_toggleHead;
		text = empty;
		empty = text + " JawOpen: " + m_isJawOpen + "/" + m_toggleJaw;
		return empty + " UseCann: " + m_fireCannon;
	}

	private void UpdateRay()
	{
		if (m_rayVisualizer != null)
		{
			m_rayVisTime += Time.fixedDeltaTime;
			float num = Mathf.Clamp(m_rayVisTime / m_rayFadeTime, 0f, 1f);
			if (num >= 1f)
			{
				DisableRayVisualizer();
			}
			else if (m_rayVisualizer.renderer.material.HasProperty("_TintColor"))
			{
				Color color = m_rayVisualizer.renderer.material.GetColor("_TintColor");
				color.a = 1f - num;
				m_rayVisualizer.renderer.material.SetColor("_TintColor", color);
			}
		}
	}

	protected void FixedUpdate()
	{
		if (NetObj.m_simulating)
		{
			UpdateRay();
			m_timeInTurn += Time.fixedDeltaTime;
			if (b_startOfTurn)
			{
				NextTurn();
				PLog.Log("Start Turn: " + StateDebug());
				b_startOfTurn = false;
			}
			HeadChangeState(!m_isHeadOpen);
			JawChangeState(!m_isJawOpen);
			if (m_fireCannon && m_timeInTurn > 4f)
			{
				FireMainCannon();
				m_fireCannon = false;
			}
		}
	}

	private void HeadChangeState(bool openIt)
	{
		if (!m_toggleHead)
		{
			return;
		}
		float num = 10f - m_bossForehead.animation["open_forehead"].length;
		if (!(m_timeInTurn < num))
		{
			m_toggleHead = false;
			if (openIt)
			{
				m_bossForehead.animation.Play("open_forehead");
				m_weakspotPlatform.m_immuneToDamage = false;
			}
			else
			{
				m_bossForehead.animation.Play("close_forehead");
				m_weakspotPlatform.m_immuneToDamage = true;
			}
			m_isHeadOpen = openIt;
		}
	}

	private void JawChangeState(bool openIt)
	{
		if (!m_toggleJaw)
		{
			return;
		}
		float num = 10f - m_bossJaw.animation["open_jaw"].length;
		if (!(m_timeInTurn < num))
		{
			m_toggleJaw = false;
			if (openIt)
			{
				m_bossJaw.animation.Play("open_jaw");
			}
			else
			{
				m_bossJaw.animation.Play("close_jaw");
			}
			m_isJawOpen = openIt;
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_timeInTurn);
		writer.Write(m_bossTurn);
		writer.Write(m_isHeadOpen);
		writer.Write(m_isJawOpen);
		writer.Write(m_cannonTarget.x);
		writer.Write(m_cannonTarget.y);
		writer.Write(m_cannonTarget.z);
		writer.Write(m_cannonAngle);
		writer.Write(m_beamAngle);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_timeInTurn = reader.ReadSingle();
		m_bossTurn = reader.ReadInt32();
		m_isHeadOpen = reader.ReadBoolean();
		m_isJawOpen = reader.ReadBoolean();
		Vector3 vector = default(Vector3);
		vector.x = reader.ReadSingle();
		vector.y = reader.ReadSingle();
		vector.z = reader.ReadSingle();
		m_cannonTarget = vector;
		m_cannonAngle = reader.ReadSingle();
		m_beamAngle = reader.ReadSingle();
		vector = default(Vector3);
		vector.x = reader.ReadSingle();
		vector.y = reader.ReadSingle();
		vector.z = reader.ReadSingle();
		base.transform.position = vector;
		Quaternion rotation = default(Quaternion);
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.rotation = rotation;
	}

	private void NextTurn()
	{
		m_bossTurn++;
		m_timeInTurn = 0f;
		if (m_bossTurn == 5)
		{
			m_bossTurn = 1;
		}
		if (m_bossTurn == 1)
		{
			if (m_isJawOpen)
			{
				m_bossJaw.animation.Play("close_jaw");
				m_isJawOpen = false;
			}
			m_toggleHead = true;
		}
		if (m_bossTurn == 2)
		{
			GetMineLauncher();
			m_toggleJaw = true;
		}
		if (m_bossTurn == 3)
		{
			m_toggleHead = true;
		}
		if (m_bossTurn == 4)
		{
			m_fireCannon = true;
		}
	}

	private void GetLinks()
	{
		Platform platform = UnityEngine.Object.FindObjectOfType(typeof(Platform)) as Platform;
	}

	private Ship GetMainTarget()
	{
		Ship[] array = UnityEngine.Object.FindObjectsOfType(typeof(Ship)) as Ship[];
		if (array.Length == 0)
		{
			return null;
		}
		int num = PRand.Range(0, array.Length - 1);
		return array[num];
	}

	private float DistanceToLine(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}

	private bool InFiringCone(Unit unit, Ray ray)
	{
		Vector3[] targetPoints = unit.GetTargetPoints();
		Vector3[] array = targetPoints;
		foreach (Vector3 point in array)
		{
			float num = DistanceToLine(ray, point);
			if (num < m_rayWidth / 2f)
			{
				return true;
			}
		}
		return false;
	}

	private void FireMainCannon()
	{
		GameObject gameObject = GuiUtils.FindChildOf(m_weaponPlatform.transform, "bossc1m9_cannon");
		Gun_Railgun component = gameObject.GetComponent<Gun_Railgun>();
		Vector3 position = gameObject.transform.position;
		Vector3 normalized = (m_cannonTarget - position).normalized;
		Vector3 targetPos = position + normalized * 3000f;
		position.y = 1f;
		targetPos.y = 1f;
		Ray ray = new Ray(position, normalized);
		Ship[] array = UnityEngine.Object.FindObjectsOfType(typeof(Ship)) as Ship[];
		Ship[] array2 = array;
		foreach (Ship ship in array2)
		{
			if (InFiringCone(ship, ray))
			{
				ship.Damage(new Hit(component, m_cannonDamage, m_cannonArmorPiercing, ship.transform.position, new Vector3(1f, 0f, 0f)), ship.GetSectionTop());
				DoHitEffect(ship.transform.position);
			}
		}
		Vector3 position2 = gameObject.transform.position;
		Quaternion rot = Quaternion.LookRotation(normalized, new Vector3(0f, 1f, 0f));
		AnimateFire(position2, rot);
		EnableRayVisualizer(position, targetPos);
	}

	private bool GetMinePositions(out Vector3 target1, out Vector3 target2)
	{
		float f = (float)Math.PI / 180f * (float)PRand.Range(0, 360);
		Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
		float num = PRand.Range(0, 30) + 20;
		target1 = m_cannonTarget + vector * num;
		target2 = m_cannonTarget + vector * (0f - num);
		return false;
	}

	private void FireMines()
	{
		GetMinePositions(out var target, out var target2);
		GameObject gameObject = GuiUtils.FindChildOf(m_weaponPlatform.transform, "bossc1m9_mine");
		Gun_AutoCannon component = gameObject.GetComponent<Gun_AutoCannon>();
		Order order = new Order(component, Order.Type.Fire, target);
		Order order2 = new Order(component, Order.Type.Fire, target2);
		component.ClearOrders();
		component.AddOrder(order);
		component.AddOrder(order2);
	}

	private GameObject GetMineLauncher()
	{
		Ship mainTarget = GetMainTarget();
		if (mainTarget == null)
		{
			return null;
		}
		m_cannonTarget = mainTarget.transform.position;
		FireMines();
		return null;
	}

	private void EnableRayVisualizer(Vector3 muzzlePos, Vector3 targetPos)
	{
		muzzlePos.y = 5f;
		targetPos.y = 5f;
		m_rayVisTime = 0f;
		m_rayTargetPos = targetPos;
		if (m_rayVisualizer == null)
		{
			m_rayVisualizer = UnityEngine.Object.Instantiate(m_rayPrefab) as GameObject;
		}
		Vector3 position = (muzzlePos + targetPos) * 0.5f;
		float num = Vector3.Distance(muzzlePos, targetPos);
		m_rayVisualizer.transform.position = position;
		m_rayVisualizer.transform.localScale = new Vector3(m_rayWidth, 1f, num);
		m_rayVisualizer.transform.rotation = Quaternion.LookRotation(targetPos - muzzlePos, Vector3.up);
		float y = num / 8f;
		m_rayVisualizer.renderer.material.mainTextureScale = new Vector2(1f, y);
	}

	private void DisableRayVisualizer()
	{
		if (m_rayVisualizer != null)
		{
			UnityEngine.Object.Destroy(m_rayVisualizer);
			m_rayVisualizer = null;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		DisableRayVisualizer();
	}

	private void AnimateFire(Vector3 pos, Quaternion rot)
	{
		if (m_muzzleEffect != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_muzzleEffect, pos, rot) as GameObject;
			gameObject.transform.parent = base.transform;
		}
	}

	private void DoHitEffect(Vector3 pos)
	{
		if (m_hitEffectHiPrefab != null)
		{
			UnityEngine.Object.Instantiate(m_hitEffectHiPrefab, pos, Quaternion.identity);
		}
	}
}
