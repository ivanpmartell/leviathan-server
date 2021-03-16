#define DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using PTech;
using UnityEngine;

[AddComponentMenu("Scripts/Units/Ship")]
public class Ship : Unit
{
	private enum SinkStyle
	{
		None,
		RollLeft,
		RollRight,
		RollForward,
		RollBackward
	}

	[Serializable]
	public class BaseSettings
	{
		public float m_speed = 10f;

		public float m_reverseSpeed = 5f;

		public float m_acceleration = 1f;

		public float m_reverseAcceleration = 0.5f;

		public float m_turnSpeed = 10f;

		public float m_sightRange = 50f;

		public float m_breakAcceleration = 7f;

		public int m_maxHealth = 1;
	}

	[Serializable]
	public class EmitterData
	{
		public ParticleSystem m_ps;

		public float m_maxEmission;

		public float m_maxSpeed;
	}

	public Texture2D m_icon;

	public bool m_editByPlayer = true;

	public BaseSettings m_baseSettings;

	public string m_series = string.Empty;

	public string m_displayClassName = "Unkown";

	public int m_sinkTreshold = 100;

	public float m_sinkDelay = 30f;

	public bool m_deepKeel;

	public float m_Width = 7f;

	public float m_length = 23f;

	public float m_deckHeight = 2f;

	public float m_mass = 100f;

	public int m_autoRepairAmount = 2;

	public float m_autoRepairTreshold = 0.4f;

	public float m_sideWayFriction = 0.015f;

	public float m_forwardFriction = 0.001f;

	public float m_rotationFriction = 0.01f;

	public bool m_systemFailuresEnabled = true;

	public GameObject m_hitEffect;

	public GameObject[] m_forwardEmitters;

	private EmitterData[] m_forwardEmittersData;

	public GameObject[] m_engineParticles;

	private EmitterData[] m_engineParticlesData;

	private float m_maxSpeed;

	private float m_maxReverseSpeed;

	private float m_acceleration;

	private float m_reverseAcceleration;

	private float m_maxTurnSpeed;

	private float m_breakAcceleration;

	private int m_maxHealth;

	private int m_health;

	private bool m_requestedMaintenanceMode;

	private bool m_maintenanceMode;

	private float m_maintenanceTimer = -1f;

	private float m_maintenanceHealTimer;

	private Vector3 m_realPos;

	private Quaternion m_realRot;

	private Vector3 m_velocity;

	private float m_rotVelocity;

	private float m_rockAngleX;

	private float m_rockAngleZ;

	private float m_rockVelocityX;

	private float m_rockVelocityZ;

	private float m_maxRockAngleZ = 30f;

	private float m_maxRockAngleX = 15f;

	private float m_maxRockVel = 15f;

	private bool m_reverse;

	private SinkStyle m_sinking;

	private bool m_selfDestruct;

	private float m_collisionDamageTimer;

	private int m_sightRayMask;

	private int m_shallowRayMask;

	private Material m_shipMaterial;

	protected LineDrawer m_lineDrawer;

	protected int m_moveOrderLineMaterialID = -1;

	protected int m_forwardLineMaterialID = -1;

	protected int m_forwardCloseLineMaterialID = -1;

	protected int m_reverseLineMaterialID = -1;

	protected int m_reverseCloseLineMaterialID = -1;

	protected int m_blockedLineMaterialID = -1;

	private Route m_route = new Route();

	private List<Section> m_sections = new List<Section>();

	private DamageMan m_damageMan;

	private WaterSurface m_waterSurface;

	private float[] m_waterHeight = new float[4];

	private float[] m_waterVel = new float[4];

	private float m_shallowTestTimer;

	private float m_shallowHitTimer;

	private float m_groundedTimer;

	private float m_suppliedTimer;

	private float m_autoRepairTimer;

	private float m_damageEffectTimer;

	private float m_sinkTimer = -1f;

	private float m_engineDamagedTimer = -1f;

	private float m_bridgeDamagedTimer = -1f;

	private float m_outOfControlTimer = -1f;

	private float m_inMonsterMineTimer = -1f;

	private float m_monsterMineDamageTimer = 2f;

	public static GenericFactory<AIState<Ship>> m_aiStateFactory = new GenericFactory<AIState<Ship>>();

	private AIStateMachine<Ship> m_stateMachine;

	public GameObject m_path;

	public int m_pathNetID;

	private ShipAISettings m_aiSettings = new ShipAISettings();

	private GameObject m_monsterMine;

	public int m_maxHardpoints = 8;

	public Ship()
	{
		m_Ai = new ShipAi
		{
			m_ship = this
		};
	}

	public static void RegisterAIStates()
	{
		m_aiStateFactory.Register<ShipInactive>("inactive");
		m_aiStateFactory.Register<ShipPatrol>("patrol");
		m_aiStateFactory.Register<ShipGuard>("guard");
		m_aiStateFactory.Register<ShipHuman>("human");
		m_aiStateFactory.Register<ShipFollow>("follow");
		m_aiStateFactory.Register<ShipThink>("think");
		m_aiStateFactory.Register<ShipCombat>("combat");
		m_aiStateFactory.Register<ShipCombat_TurnAndFire>("c_turnandfire");
		m_aiStateFactory.Register<ShipCombat_DriveBy>("c_driveby");
		m_aiStateFactory.Register<ShipCombat_ChickenRace>("c_chicken");
		m_aiStateFactory.Register<ShipCombat_Surround>("c_surround");
		m_aiStateFactory.Register<ShipAttack>("attack");
		m_aiStateFactory.Register<ShipBossC1m3>("BossC1M3");
	}

	public override void Awake()
	{
		base.Awake();
		m_realPos = base.transform.position;
		m_realRot = base.transform.rotation;
		m_sightRayMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("blocksight"));
		m_shallowRayMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("shallow")) | (1 << LayerMask.NameToLayer("beach"));
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.isKinematic = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		for (int i = 0; i < 4; i++)
		{
			m_sections.Add(null);
		}
		m_waterSurface = GameObject.Find("WaterSurface").GetComponent<WaterSurface>();
		m_damageMan = new DamageMan(base.gameObject);
		SaveParticleData(m_forwardEmitters, out m_forwardEmittersData);
		SaveParticleData(m_engineParticles, out m_engineParticlesData);
		SpawnShipTrigger();
		ResetAiState();
	}

	private void SpawnShipTrigger()
	{
		GameObject gameObject = new GameObject("egg");
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		gameObject.layer = 19;
		gameObject.AddComponent("SphereCollider");
	}

	public void GetAllHPModules(ref List<HPModule> modules)
	{
		foreach (Section section in m_sections)
		{
			section.GetAllHPModules(ref modules);
		}
	}

	public void ResetAiState()
	{
		m_stateMachine = new AIStateMachine<Ship>(this, m_aiStateFactory);
		m_stateMachine.PushState("think");
	}

	private void SaveParticleData(GameObject[] objects, out EmitterData[] emitterData)
	{
		emitterData = new EmitterData[objects.Length];
		for (int i = 0; i < objects.Length; i++)
		{
			EmitterData emitterData2 = new EmitterData();
			emitterData[i] = emitterData2;
			emitterData2.m_ps = objects[i].GetComponent<ParticleSystem>();
			emitterData2.m_maxEmission = emitterData2.m_ps.emissionRate;
			emitterData2.m_maxSpeed = emitterData2.m_ps.startSpeed;
			emitterData2.m_ps.emissionRate = 0f;
			emitterData2.m_ps.startSpeed = 0f;
		}
	}

	public override void Update()
	{
		base.Update();
		if (NetObj.m_drawOrders)
		{
			DrawOrders();
		}
	}

	protected virtual bool SetupLineDrawer()
	{
		if (m_lineDrawer == null)
		{
			m_lineDrawer = Camera.main.GetComponent<LineDrawer>();
			if (m_lineDrawer == null)
			{
				return false;
			}
			m_moveOrderLineMaterialID = m_lineDrawer.GetTypeID("moveOrder");
			m_forwardLineMaterialID = m_lineDrawer.GetTypeID("moveForward");
			m_forwardCloseLineMaterialID = m_lineDrawer.GetTypeID("moveForwardClose");
			m_reverseLineMaterialID = m_lineDrawer.GetTypeID("moveReverse");
			m_reverseCloseLineMaterialID = m_lineDrawer.GetTypeID("moveReverseClose");
			m_blockedLineMaterialID = m_lineDrawer.GetTypeID("moveBlocked");
			DebugUtils.Assert(m_forwardLineMaterialID >= 0 && m_blockedLineMaterialID >= 0);
		}
		return true;
	}

	private void OnGUI()
	{
		if (NetObj.m_drawOrders && GetOwner() == NetObj.m_localPlayerID)
		{
			m_route.OnGUI();
		}
	}

	private void DrawOrders()
	{
		if (GetOwner() == NetObj.m_localPlayerID && SetupLineDrawer() && m_orders.Count > 0)
		{
			int predictMaterialID = m_forwardLineMaterialID;
			int predictCloseMaterialID = m_forwardCloseLineMaterialID;
			if (m_route.IsReverse())
			{
				predictMaterialID = m_reverseLineMaterialID;
				predictCloseMaterialID = m_reverseCloseLineMaterialID;
			}
			float maxTime = 120f;
			float fixedDeltaTime = Time.fixedDeltaTime;
			m_route.Draw(base.transform.position, m_lineDrawer, m_moveOrderLineMaterialID, predictMaterialID, predictCloseMaterialID, m_realPos, m_realRot, m_velocity, m_rotVelocity, maxTime, fixedDeltaTime, m_Width, m_maxTurnSpeed, m_maxSpeed, m_maxReverseSpeed, m_acceleration, m_reverseAcceleration, m_breakAcceleration, m_forwardFriction, m_sideWayFriction, m_rotationFriction);
		}
	}

	protected override void FixedUpdate()
	{
		DebugDrawRoute();
		base.FixedUpdate();
		if (!NetObj.m_simulating)
		{
			return;
		}
		if (!m_dead)
		{
			UpdateSink(Time.fixedDeltaTime);
			if (m_selfDestruct)
			{
				Explode();
			}
			UpdateAutoRepair(Time.fixedDeltaTime);
			UpdateMaintenance(Time.fixedDeltaTime);
		}
		UpdateMotion();
		UpdateSpeedParticles();
		UpdateShallowTest(Time.fixedDeltaTime);
		UpdateMonsterMines(Time.fixedDeltaTime);
		m_collisionDamageTimer -= Time.fixedDeltaTime;
		m_shallowHitTimer -= Time.fixedDeltaTime;
		m_groundedTimer -= Time.fixedDeltaTime;
		m_suppliedTimer -= Time.fixedDeltaTime;
		m_engineDamagedTimer -= Time.fixedDeltaTime;
		m_bridgeDamagedTimer -= Time.fixedDeltaTime;
		m_outOfControlTimer -= Time.fixedDeltaTime;
		m_damageEffectTimer -= Time.fixedDeltaTime;
		m_inMonsterMineTimer -= Time.fixedDeltaTime;
		m_stateMachine.Update(Time.fixedDeltaTime);
	}

	protected override void OnSetSimulating(bool enabled)
	{
		base.OnSetSimulating(enabled);
		if (enabled)
		{
			EmitterData[] forwardEmittersData = m_forwardEmittersData;
			foreach (EmitterData emitterData in forwardEmittersData)
			{
				emitterData.m_ps.Play();
			}
		}
		else
		{
			EmitterData[] forwardEmittersData2 = m_forwardEmittersData;
			foreach (EmitterData emitterData2 in forwardEmittersData2)
			{
				emitterData2.m_ps.Pause();
			}
		}
		m_damageMan.SetSimulating(enabled);
		foreach (Section section in m_sections)
		{
			section.OnSetSimulating(enabled);
		}
	}

	private void UpdateMonsterMines(float dt)
	{
		if (!IsDead() && !(m_inMonsterMineTimer <= 0f))
		{
			m_monsterMineDamageTimer -= dt;
			if (!(m_monsterMineDamageTimer >= 0f))
			{
				m_monsterMineDamageTimer = 2f;
				Section section = m_sections[PRand.Range(0, 3)];
				section.Damage(new Hit(17, 25));
			}
		}
	}

	private void UpdateShallowTest(float dt)
	{
		if (!m_deepKeel)
		{
			return;
		}
		m_shallowTestTimer += dt;
		if (m_shallowTestTimer > 1f)
		{
			m_shallowTestTimer = 0f;
			Vector3 position = base.transform.position;
			if (Physics.Raycast(position, -base.transform.up, 10f, m_shallowRayMask))
			{
				OnHitShallow();
			}
		}
	}

	public bool IsGrounded()
	{
		return m_groundedTimer > 0f;
	}

	private void OnHitShallow()
	{
		if (!NetObj.m_simulating || !(m_shallowHitTimer <= 0f) || !(m_groundedTimer <= 0f))
		{
			return;
		}
		m_shallowHitTimer = 4f;
		if (m_velocity.magnitude > 0.1f && (double)PRand.Value() < 0.25)
		{
			if (IsVisible())
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipGroundedText);
			}
			if (GetOwner() == NetObj.m_localPlayerID)
			{
				VOSystem.instance.DoEvent("Grounded");
			}
			float num = PRand.Value();
			m_groundedTimer = 1f + num * 9f;
			m_velocity *= 0.15f;
			m_rotVelocity *= 0.15f;
			Section section = m_sections[PRand.Range(0, 3)];
			section.Damage(new Hit((int)((float)m_maxHealth * num * 0.2f), section.m_armorClass));
		}
	}

	private void UpdateSink(float dt)
	{
		if (m_dead || !(m_sinkTimer >= 0f))
		{
			return;
		}
		m_sinkTimer -= dt;
		if (m_sinkTimer < 0f)
		{
			if (IsVisible())
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipSinkingText);
			}
			Sink();
		}
	}

	private void StartToSink()
	{
		if (m_sinkTimer < 0f && m_sinking == SinkStyle.None)
		{
			m_sinkTimer = m_sinkDelay;
			if (IsVisible())
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, m_sinkTimer + "s", Constants.m_shipSinkingWarningText);
			}
		}
	}

	private void Explode()
	{
		foreach (Section section in m_sections)
		{
			section.Explode();
		}
		Sink();
	}

	private void Sink()
	{
		m_sinking = (SinkStyle)PRand.Range(1, 4);
		OnKilled();
	}

	protected override void OnKilled()
	{
		if (m_dead)
		{
			return;
		}
		base.OnKilled();
		foreach (Section section in m_sections)
		{
			section.OnKilled();
		}
		ClearOrders();
		SetSelected(selected: false, explicitSelected: false);
		if (m_lastDamageDealer >= 0)
		{
			TurnMan.instance.AddShipsSunk(m_lastDamageDealer);
		}
		TurnMan.instance.AddShipsLost(GetOwner());
		EmitterData[] engineParticlesData = m_engineParticlesData;
		foreach (EmitterData emitterData in engineParticlesData)
		{
			emitterData.m_ps.emissionRate = 0f;
		}
		DisableAOPlane();
		if (IsKing())
		{
			VOSystem.instance.DoEvent("Flagship sunk");
		}
		else if (GetOwner() == NetObj.m_localPlayerID)
		{
			VOSystem.instance.DoEvent("Player ship sunk");
		}
		else if (NetObj.m_localPlayerID != -1)
		{
			if (TurnMan.instance.IsHostile(NetObj.m_localPlayerID, GetOwner()))
			{
				VOSystem.instance.DoEvent("Enemy ship sunk");
			}
			else
			{
				VOSystem.instance.DoEvent("Allie ship sunk");
			}
		}
	}

	private void DisableAOPlane()
	{
		Transform transform = base.transform.FindChild("ShipAO");
		if (transform != null)
		{
			transform.gameObject.SetActiveRecursively(state: false);
		}
	}

	public Section GetSectionFront()
	{
		return m_sections[0];
	}

	public Section GetSectionMid()
	{
		return m_sections[1];
	}

	public Section GetSectionRear()
	{
		return m_sections[2];
	}

	public Section GetSectionTop()
	{
		return m_sections[3];
	}

	public Section GetSection(Section.SectionType stype)
	{
		return m_sections[(int)stype];
	}

	public override string GetTooltip()
	{
		string text = string.Empty;
		if (IsKing())
		{
			text += "KING \n";
		}
		text += GetName();
		if (CheatMan.instance.DebugAi())
		{
			text = text + " (" + GetNetID() + ")";
		}
		text += "\n";
		string text2 = text;
		text = text2 + "Health: " + m_health + "\n";
		if (IsTakingWater())
		{
			text2 = text;
			text = text2 + "Sinking: " + m_sinkTimer + "s";
		}
		if (CheatMan.instance.DebugAi())
		{
			text += GetAi().ToString();
			text += m_stateMachine.ToString();
		}
		return text;
	}

	private void UpdateMotion()
	{
		if (m_sinking != 0)
		{
			Vector3 position = base.rigidbody.position;
			Quaternion rotation = base.rigidbody.rotation;
			position.y -= Time.fixedDeltaTime * 0.5f;
			Vector3 eulerAngles = rotation.eulerAngles;
			switch (m_sinking)
			{
			case SinkStyle.RollLeft:
				eulerAngles.z -= Time.fixedDeltaTime * 4f;
				break;
			case SinkStyle.RollRight:
				eulerAngles.z += Time.fixedDeltaTime * 4f;
				break;
			case SinkStyle.RollBackward:
				eulerAngles.x -= Time.fixedDeltaTime * 3f;
				break;
			case SinkStyle.RollForward:
				eulerAngles.x += Time.fixedDeltaTime * 3f;
				break;
			}
			rotation.eulerAngles = eulerAngles;
			base.rigidbody.MovePosition(position);
			base.rigidbody.MoveRotation(rotation);
			m_damageMan.OnSinkingUpdate();
			float num = -20f;
			if (position.y <= num)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (!m_dead)
			{
				UpdateEngine();
			}
			UpdateWaterMotionPos(out var visPos, out var visRot);
			UpdateRocking(ref visRot);
			base.rigidbody.MovePosition(visPos);
			base.rigidbody.MoveRotation(visRot);
		}
	}

	private bool GetRotVelTowards(ref float rotVel, Vector3 dir, bool reverse)
	{
		float y = Quaternion.LookRotation(dir).eulerAngles.y;
		float num = m_realRot.eulerAngles.y;
		if (reverse)
		{
			num += 180f;
		}
		float num2 = Mathf.DeltaAngle(num, y);
		float num3 = Mathf.Clamp(num2, -85f, 85f) / 85f;
		rotVel += num3 * 30f * Time.fixedDeltaTime;
		if (Mathf.Abs(rotVel) > m_maxTurnSpeed)
		{
			rotVel = ((!(rotVel > 0f)) ? (0f - m_maxTurnSpeed) : m_maxTurnSpeed);
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

	private void UpdateEngine()
	{
		float num = Mathf.Max(5f, m_length / 2f);
		float num2 = 0f;
		float num3 = m_maxSpeed;
		Vector3 vector = m_realRot * Vector3.forward;
		Vector3 vector2 = m_realRot * Vector3.right;
		float num4 = Vector3.Dot(vector, m_velocity);
		if (m_shallowHitTimer > 0f)
		{
			num3 *= 0.5f;
		}
		if (m_inMonsterMineTimer > 0f)
		{
			num3 *= 0.2f;
		}
		if (m_route.NrOfWaypoints() > 0)
		{
			DebugUtils.Assert(m_orders.Count > 0);
			Order value = m_orders.First.Value;
			Route.Waypoint nextWaypoint = m_route.GetNextWaypoint();
			m_reverse = nextWaypoint.m_reverse;
			if (!value.HasReachedPosition())
			{
				if (ReachedWP(m_realPos, vector, vector2, nextWaypoint.m_pos, m_Width))
				{
					value.SetReachedPosition(reached: true);
				}
				num2 = m_route.GetSpeedFactor(0, m_realPos, vector, vector2, m_breakAcceleration, m_maxTurnSpeed, m_maxSpeed, num4);
			}
			bool flag = false;
			if (m_outOfControlTimer <= 0f && m_engineDamagedTimer <= 0f)
			{
				Vector3 dir = ((!value.HasReachedPosition() || !nextWaypoint.m_haveDirection) ? (nextWaypoint.m_pos - m_realPos).normalized : nextWaypoint.m_direction);
				flag = GetRotVelTowards(ref m_rotVelocity, dir, nextWaypoint.m_reverse);
			}
			if (value.HasReachedPosition() && (!nextWaypoint.m_haveDirection || flag))
			{
				RemoveFirstOrder();
				if (IsOrdersEmpty())
				{
					GetAi().m_goalPosition = null;
					GetAi().m_goalFacing = null;
				}
			}
		}
		float num5 = ((!m_reverse) ? (num2 * num3) : (num2 * (0f - m_maxReverseSpeed)));
		if (m_outOfControlTimer > 0f)
		{
			num5 = num4;
		}
		if (m_engineDamagedTimer > 0f)
		{
			num5 = 0f;
		}
		if (!IsGrounded())
		{
			if (!m_reverse)
			{
				if (num5 > num4)
				{
					m_velocity += vector * m_acceleration * Time.fixedDeltaTime;
				}
				else if (num5 < num4)
				{
					m_velocity -= vector * m_breakAcceleration * Time.fixedDeltaTime;
				}
			}
			else if (num5 < num4)
			{
				m_velocity += vector * (0f - m_reverseAcceleration) * Time.fixedDeltaTime;
			}
			else if (num5 > num4)
			{
				m_velocity -= vector * (0f - m_breakAcceleration) * Time.fixedDeltaTime;
			}
		}
		Vector3 vector3 = Utils.Project(m_velocity, vector);
		Vector3 vector4 = Utils.Project(m_velocity, vector2);
		m_velocity -= vector3 * m_forwardFriction;
		m_velocity -= vector4 * m_sideWayFriction;
		m_rotVelocity -= m_rotVelocity * m_rotationFriction;
		m_realPos += m_velocity * Time.fixedDeltaTime;
		m_realRot *= Quaternion.Euler(new Vector3(0f, m_rotVelocity * Time.fixedDeltaTime, 0f));
		Utils.NormalizeQuaternion(ref m_realRot);
		m_velocity.y = 0f;
		m_realPos.y = 0f;
		if (m_reverse)
		{
			UpdateEngineParticles(num4 / (0f - m_maxReverseSpeed), reverse: true);
		}
		else
		{
			UpdateEngineParticles(num4 / m_maxSpeed, reverse: false);
		}
	}

	private void UpdateEngineParticles(float enginePower, bool reverse)
	{
		float num = enginePower;
		if (reverse)
		{
			num = 0f - num;
		}
		EmitterData[] engineParticlesData = m_engineParticlesData;
		foreach (EmitterData emitterData in engineParticlesData)
		{
			emitterData.m_ps.emissionRate = emitterData.m_maxEmission * enginePower;
			emitterData.m_ps.startSpeed = emitterData.m_maxSpeed * num;
		}
	}

	private void UpdateSpeedParticles()
	{
		float num = Vector3.Dot(m_velocity, base.transform.forward);
		if (m_sinking != 0)
		{
			num = 0f;
		}
		float num2 = Mathf.Clamp((num - 1f) / 9f, 0f, 1f);
		EmitterData[] forwardEmittersData = m_forwardEmittersData;
		foreach (EmitterData emitterData in forwardEmittersData)
		{
			emitterData.m_ps.emissionRate = num2 * emitterData.m_maxEmission;
			emitterData.m_ps.startSpeed = num2 * emitterData.m_maxSpeed;
		}
	}

	private void UpdateRoute()
	{
		List<Route.Waypoint> list = new List<Route.Waypoint>();
		foreach (Order order in m_orders)
		{
			if (order.m_type == Order.Type.MoveForward || order.m_type == Order.Type.MoveBackward || order.m_type == Order.Type.MoveRotate)
			{
				bool reverse = order.m_type == Order.Type.MoveBackward;
				bool havePosition = order.m_type == Order.Type.MoveForward || order.m_type == Order.Type.MoveBackward;
				list.Add(new Route.Waypoint(order.GetPos(), order.GetFacing(), reverse, havePosition, order.HaveFacing()));
			}
		}
		m_route.SetWaypoints(list);
	}

	public override void OnOrdersChanged()
	{
		UpdateRoute();
	}

	private void UpdateRocking(ref Quaternion visRot)
	{
		m_rockVelocityZ -= m_rockAngleZ * Time.fixedDeltaTime * 10f;
		m_rockVelocityZ -= m_rockVelocityZ * Time.fixedDeltaTime * 2f;
		m_rockAngleZ += m_rockVelocityZ * Time.fixedDeltaTime;
		m_rockVelocityX -= m_rockAngleX * Time.fixedDeltaTime * 10f;
		m_rockVelocityX -= m_rockVelocityX * Time.fixedDeltaTime * 2f;
		m_rockAngleX += m_rockVelocityX * Time.fixedDeltaTime;
		m_rockAngleX = Mathf.Clamp(m_rockAngleX, 0f - m_maxRockAngleX, m_maxRockAngleX);
		m_rockAngleZ = Mathf.Clamp(m_rockAngleZ, 0f - m_maxRockAngleZ, m_maxRockAngleZ);
		visRot *= Quaternion.Euler(new Vector3(m_rockAngleX, 0f, m_rockAngleZ));
	}

	private void UpdateWaterMotionPos(out Vector3 visPos, out Quaternion visRot)
	{
		float num = m_Width / 2f;
		float num2 = m_length / 2f;
		Vector3[] array = new Vector3[4]
		{
			new Vector3(0f, 0f, num2),
			new Vector3(0f, 0f, 0f - num2),
			new Vector3(0f - num, 0f, 0f),
			new Vector3(num, 0f, 0f)
		};
		Vector3[] array2 = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			ref Vector3 reference = ref array2[i];
			reference = m_realPos + m_realRot * array[i];
		}
		for (int j = 0; j < 4; j++)
		{
			float worldWaveHeight = m_waterSurface.GetWorldWaveHeight(array2[j]);
			float num3 = worldWaveHeight - m_waterHeight[j];
			if (num3 > 0f)
			{
				m_waterVel[j] += Mathf.Abs(num3) * num3 * 5000f * Time.fixedDeltaTime / m_mass;
			}
			else
			{
				m_waterVel[j] += num3 * 1000f * Time.fixedDeltaTime / m_mass;
			}
			m_waterVel[j] -= m_waterVel[j] * 0.01f;
			m_waterHeight[j] += m_waterVel[j] * Time.fixedDeltaTime;
			array[j].y = m_waterHeight[j];
		}
		Vector3 normalized = (array[0] - array[1]).normalized;
		Vector3 normalized2 = (array[3] - array[2]).normalized;
		Vector3 upwards = Vector3.Cross(normalized, normalized2);
		float y = (array[0].y + array[1].y + array[2].y + array[3].y) / 4f;
		Vector3 vector = new Vector3(m_realPos.x, y, m_realPos.z);
		Quaternion quaternion = m_realRot * Quaternion.LookRotation(normalized, upwards);
		visPos = vector;
		visRot = quaternion;
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write((short)1);
		m_stateMachine.Save(writer);
		writer.Write(m_realPos.x);
		writer.Write(m_realPos.y);
		writer.Write(m_realPos.z);
		writer.Write(m_realRot.x);
		writer.Write(m_realRot.y);
		writer.Write(m_realRot.z);
		writer.Write(m_realRot.w);
		writer.Write(m_reverse);
		writer.Write((int)m_sinking);
		writer.Write(m_velocity.x);
		writer.Write(m_velocity.y);
		writer.Write(m_velocity.z);
		writer.Write(m_rockAngleX);
		writer.Write(m_rockAngleZ);
		writer.Write(m_rockVelocityX);
		writer.Write(m_rockVelocityZ);
		writer.Write(m_rotVelocity);
		writer.Write(m_collisionDamageTimer);
		writer.Write(m_shallowHitTimer);
		writer.Write(m_groundedTimer);
		writer.Write(m_sinkTimer);
		writer.Write(m_engineDamagedTimer);
		writer.Write(m_bridgeDamagedTimer);
		writer.Write(m_outOfControlTimer);
		writer.Write(m_damageEffectTimer);
		writer.Write(m_inMonsterMineTimer);
		writer.Write(m_monsterMineDamageTimer);
		writer.Write(m_maintenanceMode);
		writer.Write(m_maintenanceTimer);
		writer.Write(m_maintenanceHealTimer);
		writer.Write((short)m_health);
		float[] waterHeight = m_waterHeight;
		foreach (float value in waterHeight)
		{
			writer.Write(value);
		}
		float[] waterVel = m_waterVel;
		foreach (float value2 in waterVel)
		{
			writer.Write(value2);
		}
		Trace[] componentsInChildren = base.gameObject.GetComponentsInChildren<Trace>(includeInactive: true);
		Trace[] array = componentsInChildren;
		foreach (Trace trace in array)
		{
			trace.Save(writer);
		}
		Wake[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Wake>(includeInactive: true);
		Wake[] array2 = componentsInChildren2;
		foreach (Wake wake in array2)
		{
			wake.Save(writer);
		}
		m_damageMan.SaveDamageEffects(writer);
		foreach (Section section in m_sections)
		{
			writer.Write(section.gameObject.name);
			section.SaveState(writer);
		}
		m_aiSettings.SaveState(writer);
		if (GetOwner() >= 4)
		{
			if (GetPath() == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(GetPath().GetComponent<NetObj>().GetNetID());
			}
			m_Ai.SaveState(writer);
			SaveOrders(writer);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		short num = reader.ReadInt16();
		m_stateMachine.Load(reader);
		m_realPos.x = reader.ReadSingle();
		m_realPos.y = reader.ReadSingle();
		m_realPos.z = reader.ReadSingle();
		m_realRot.x = reader.ReadSingle();
		m_realRot.y = reader.ReadSingle();
		m_realRot.z = reader.ReadSingle();
		m_realRot.w = reader.ReadSingle();
		Utils.NormalizeQuaternion(ref m_realRot);
		m_reverse = reader.ReadBoolean();
		m_sinking = (SinkStyle)reader.ReadInt32();
		m_velocity.x = reader.ReadSingle();
		m_velocity.y = reader.ReadSingle();
		m_velocity.z = reader.ReadSingle();
		m_rockAngleX = reader.ReadSingle();
		m_rockAngleZ = reader.ReadSingle();
		m_rockVelocityX = reader.ReadSingle();
		m_rockVelocityZ = reader.ReadSingle();
		m_rotVelocity = reader.ReadSingle();
		m_collisionDamageTimer = reader.ReadSingle();
		m_shallowHitTimer = reader.ReadSingle();
		m_groundedTimer = reader.ReadSingle();
		m_sinkTimer = reader.ReadSingle();
		m_engineDamagedTimer = reader.ReadSingle();
		m_bridgeDamagedTimer = reader.ReadSingle();
		m_outOfControlTimer = reader.ReadSingle();
		m_damageEffectTimer = reader.ReadSingle();
		m_inMonsterMineTimer = reader.ReadSingle();
		m_monsterMineDamageTimer = reader.ReadSingle();
		m_maintenanceMode = reader.ReadBoolean();
		m_maintenanceTimer = reader.ReadSingle();
		m_maintenanceHealTimer = reader.ReadSingle();
		m_health = reader.ReadInt16();
		for (int i = 0; i < m_waterHeight.Length; i++)
		{
			m_waterHeight[i] = reader.ReadSingle();
		}
		for (int j = 0; j < m_waterVel.Length; j++)
		{
			m_waterVel[j] = reader.ReadSingle();
		}
		Trace[] componentsInChildren = base.gameObject.GetComponentsInChildren<Trace>(includeInactive: true);
		Trace[] array = componentsInChildren;
		foreach (Trace trace in array)
		{
			trace.Load(reader);
		}
		Wake[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Wake>(includeInactive: true);
		Wake[] array2 = componentsInChildren2;
		foreach (Wake wake in array2)
		{
			wake.Load(reader);
		}
		m_damageMan.LoadDamageEffects(reader);
		for (int m = 0; m < m_sections.Count; m++)
		{
			string text = reader.ReadString();
			Section section = SetSection((Section.SectionType)m, text);
			section.LoadState(reader);
		}
		m_aiSettings.LoadState(reader);
		if (GetOwner() >= 4)
		{
			m_pathNetID = reader.ReadInt32();
			m_Ai.LoadState(reader);
			LoadOrders(reader);
		}
		if (m_dead)
		{
			DisableAOPlane();
		}
	}

	public override void ClearOrders()
	{
		base.ClearOrders();
		foreach (Section section in m_sections)
		{
			section.ClearOrders();
		}
	}

	public override void SaveOrders(BinaryWriter writer)
	{
		base.SaveOrders(writer);
		writer.Write(m_selfDestruct);
		writer.Write(m_requestedMaintenanceMode);
		DebugUtils.Assert(m_sections.Count == 4);
		foreach (Section section in m_sections)
		{
			section.SaveOrders(writer);
		}
	}

	public override void LoadOrders(BinaryReader reader)
	{
		base.LoadOrders(reader);
		m_selfDestruct = reader.ReadBoolean();
		m_requestedMaintenanceMode = reader.ReadBoolean();
		DebugUtils.Assert(m_sections.Count == 4);
		foreach (Section section in m_sections)
		{
			section.LoadOrders(reader);
		}
	}

	public bool Damage(Hit hit, Section hitSection)
	{
		if (m_health <= 0 || m_dead)
		{
			return true;
		}
		if (CheatMan.instance.GetPlayerImmortal() && TurnMan.instance.IsHuman(GetOwner()))
		{
			return true;
		}
		if (CheatMan.instance.GetNoDamage())
		{
			return true;
		}
		int healthDamage;
		GameRules.HitOutcome hitOutcome = GameRules.CalculateDamage(m_health, hitSection.m_armorClass, hit.m_damage, hit.m_armorPiercing, out healthDamage);
		if (healthDamage > 0 && hit.m_havePoint)
		{
			m_damageMan.EnableCloseDamageEffect(hit.m_point, healthDamage);
		}
		m_health -= healthDamage;
		m_damageMan.OnShipHealthChanged((float)m_health / (float)m_maxHealth);
		if (hit.m_dealer != null && healthDamage > 0)
		{
			Unit unit = hit.GetUnit();
			Gun gun = hit.GetGun();
			if (unit != null)
			{
				ShipAi shipAi = GetAi() as ShipAi;
				shipAi.SetTargetId(unit);
				m_lastDamageDealer = unit.GetOwner();
			}
			string gunName = ((!(gun != null)) ? string.Empty : gun.name);
			TurnMan.instance.AddPlayerDamage(hit.m_dealer.GetOwner(), healthDamage, hit.m_dealer.GetOwnerTeam() == GetOwnerTeam(), gunName);
			if (!hit.m_collision && NetObj.m_localPlayerID == hit.m_dealer.GetOwner() && !TurnMan.instance.IsHostile(GetOwner(), hit.m_dealer.GetOwner()))
			{
				VOSystem.instance.DoEvent("Friendly fire");
			}
		}
		if (IsVisible())
		{
			switch (hitOutcome)
			{
			case GameRules.HitOutcome.CritHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipCriticalHit);
				break;
			case GameRules.HitOutcome.PiercedArmor:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipPiercingHit);
				break;
			case GameRules.HitOutcome.GlancingHit:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, healthDamage, Constants.m_shipGlancingHit);
				break;
			case GameRules.HitOutcome.Deflected:
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipDeflectHit);
				break;
			}
			if (m_health <= 0)
			{
				HitText.instance.AddDmgText(GetNetID(), base.transform.position, GetName(), Constants.m_shipDestroyedHit);
			}
		}
		if (m_health > 0)
		{
			ActivateDamageEffect();
		}
		if (m_health <= 0)
		{
			Explode();
		}
		if (hit.m_havePoint && hit.m_dir != Vector3.zero)
		{
			float num = (float)healthDamage * 4f;
			Vector3 worldDir = hit.m_dir * num;
			ApplyImpulse(hit.m_point, worldDir, separate: false);
		}
		if (m_onTakenDamage != null)
		{
			m_onTakenDamage();
		}
		return healthDamage > 0;
	}

	private void ActivateDamageEffect()
	{
		if (!m_systemFailuresEnabled || m_damageEffectTimer > 0f || m_engineDamagedTimer > 0f || m_bridgeDamagedTimer > 0f || m_outOfControlTimer > 0f || m_sinkTimer > 0f)
		{
			return;
		}
		m_damageEffectTimer = 1f;
		switch (PRand.Range(0, 4))
		{
		case 0:
			if (m_engineDamagedTimer <= 0f && (float)m_health < 0.9f * (float)m_maxHealth && PRand.Value() < 0.06f)
			{
				m_engineDamagedTimer = 15f;
				if (IsVisible())
				{
					HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipEngineDamagedText);
				}
				if (GetOwner() == NetObj.m_localPlayerID)
				{
					VOSystem.instance.DoEvent("Engine damage");
				}
			}
			break;
		case 1:
			if (m_bridgeDamagedTimer <= 0f && (float)m_health < 0.75f * (float)m_maxHealth && PRand.Value() < 0.07f)
			{
				m_bridgeDamagedTimer = 15f;
				if (IsVisible())
				{
					HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipBridgeDamagedText);
				}
				if (GetOwner() == NetObj.m_localPlayerID)
				{
					VOSystem.instance.DoEvent("Bridge damaged");
				}
			}
			break;
		case 2:
			if (m_outOfControlTimer <= 0f && (float)m_health < 0.75f * (float)m_maxHealth && PRand.Value() < 0.07f)
			{
				m_outOfControlTimer = 8f;
				if (IsVisible())
				{
					HitText.instance.AddDmgText(GetNetID(), base.transform.position, string.Empty, Constants.m_shipOutOfControlText);
				}
				if (GetOwner() == NetObj.m_localPlayerID)
				{
					VOSystem.instance.DoEvent("Out of control");
				}
			}
			break;
		case 3:
			if (m_sinkTimer <= 0f && (float)m_health < 0.35f * (float)m_maxHealth && PRand.Value() < 0.08f)
			{
				StartToSink();
				if (GetOwner() == NetObj.m_localPlayerID)
				{
					VOSystem.instance.DoEvent("Ship sinking");
				}
			}
			break;
		}
	}

	public void SelfDestruct()
	{
		m_selfDestruct = true;
	}

	protected override void ClearGunOrdersAndTargets()
	{
		foreach (Section section in m_sections)
		{
			section.ClearGunOrdersAndTargets();
		}
	}

	public Section SetSection(Section.SectionType type, string name)
	{
		if (m_sections[(int)type] != null)
		{
			UnityEngine.Object.Destroy(m_sections[(int)type].gameObject);
			m_sections[(int)type] = null;
		}
		GameObject gameObject = ObjectFactory.instance.Create(name, base.transform.position, base.transform.rotation);
		if (gameObject == null)
		{
			PLog.LogError("failed to create sect ion " + name + " for ship " + GetName());
			return null;
		}
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		Section component = gameObject.GetComponent<Section>();
		if (component.m_series != m_series)
		{
			PLog.LogError("Added section " + name + " of series " + component.m_series + " to ship " + GetName() + " of series " + m_series);
		}
		if (component.GetSectionType() != type)
		{
			PLog.LogError(string.Concat("Added section ", name, " of type ", component.GetSectionType(), " as ", type));
		}
		m_sections[(int)type] = component;
		component.Setup(this);
		component.SetOwner(GetOwner());
		UpdateStats();
		if (m_shipMaterial == null)
		{
			m_shipMaterial = component.GetMaterial();
			SetupShipColor();
		}
		component.SetMaterial(m_shipMaterial);
		return component;
	}

	public override bool TestLOS(NetObj obj)
	{
		if (IsInSmoke())
		{
			return false;
		}
		Platform platform = obj as Platform;
		if (platform != null)
		{
			return TestLOS(platform);
		}
		Ship ship = obj as Ship;
		if (ship != null)
		{
			return TestLOS(ship);
		}
		Projectile projectile = obj as Projectile;
		if (projectile != null)
		{
			return TestLOS(projectile);
		}
		MineExplode mineExplode = obj as MineExplode;
		if (mineExplode != null)
		{
			return TestLOS(mineExplode);
		}
		Mine mine = obj as Mine;
		if (mine != null)
		{
			return TestLOS(mine);
		}
		return false;
	}

	private bool TestLOS(Ship othership)
	{
		if (othership.IsCloaked())
		{
			return false;
		}
		float sightRange = GetSightRange();
		float num = Vector3.Distance(base.transform.position, othership.transform.position) - othership.GetLength() / 2f;
		if (!(num < sightRange))
		{
			return false;
		}
		Vector3[] viewPoints = GetViewPoints();
		Vector3[] viewPoints2 = othership.GetViewPoints();
		Vector3[] array = viewPoints2;
		foreach (Vector3 vector in array)
		{
			if (Vector3.Distance(base.transform.position, vector) > sightRange)
			{
				continue;
			}
			Vector3[] array2 = viewPoints;
			foreach (Vector3 start in array2)
			{
				if (!Physics.Linecast(start, vector, m_sightRayMask))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override float GetSightRange()
	{
		if (m_bridgeDamagedTimer > 0f)
		{
			return m_length / 2f;
		}
		return base.GetSightRange();
	}

	private bool TestLOS(Platform platform)
	{
		if (platform.m_alwaysVisible)
		{
			return true;
		}
		if (platform.IsCloaked())
		{
			return false;
		}
		float num = Vector3.Distance(base.transform.position, platform.transform.position);
		if (!(num < GetSightRange()))
		{
			return false;
		}
		return !Physics.Linecast(base.transform.position, platform.transform.position, m_sightRayMask);
	}

	private bool TestLOS(Projectile projectile)
	{
		float num = Vector3.Distance(base.transform.position, projectile.transform.position);
		if (!(num < GetSightRange()))
		{
			return false;
		}
		return !Physics.Linecast(base.transform.position, projectile.transform.position, m_sightRayMask);
	}

	private bool TestLOS(MineExplode mine)
	{
		float num = Vector3.Distance(base.transform.position, mine.transform.position) - m_length / 2f;
		return num < mine.GetVisibleDistance();
	}

	private bool TestLOS(Mine mine)
	{
		Vector3 position = mine.transform.position;
		return TestLOS(position);
	}

	public override bool TestLOS(Vector3 point)
	{
		float num = Vector3.Distance(base.transform.position, point);
		if (num > GetSightRange())
		{
			return false;
		}
		Vector3[] viewPoints = GetViewPoints();
		Vector3[] array = viewPoints;
		foreach (Vector3 start in array)
		{
			if (!Physics.Linecast(start, point, m_sightRayMask))
			{
				return true;
			}
		}
		return false;
	}

	public override int GetTotalValue()
	{
		int num = m_settings.m_value;
		foreach (Section section in m_sections)
		{
			num += section.GetTotalValue();
		}
		return num;
	}

	private void AddSectionTargetPoint(Section.SectionType sectionType, ref List<Vector3> points)
	{
		Section section = m_sections[(int)sectionType];
		if (!section.IsInSmoke())
		{
			Vector3 center = section.GetCenter();
			float num = m_deckHeight * 0.8f;
			if (center.y < num)
			{
				center.y = num;
			}
			points.Add(center);
		}
	}

	public override Vector3[] GetTargetPoints()
	{
		List<Vector3> points = new List<Vector3>();
		switch (PRand.Range(0, 4))
		{
		case 0:
			AddSectionTargetPoint(Section.SectionType.Front, ref points);
			AddSectionTargetPoint(Section.SectionType.Mid, ref points);
			AddSectionTargetPoint(Section.SectionType.Rear, ref points);
			break;
		case 1:
			AddSectionTargetPoint(Section.SectionType.Rear, ref points);
			AddSectionTargetPoint(Section.SectionType.Mid, ref points);
			AddSectionTargetPoint(Section.SectionType.Front, ref points);
			break;
		case 2:
			AddSectionTargetPoint(Section.SectionType.Mid, ref points);
			AddSectionTargetPoint(Section.SectionType.Rear, ref points);
			AddSectionTargetPoint(Section.SectionType.Front, ref points);
			break;
		case 3:
			AddSectionTargetPoint(Section.SectionType.Mid, ref points);
			AddSectionTargetPoint(Section.SectionType.Front, ref points);
			AddSectionTargetPoint(Section.SectionType.Rear, ref points);
			break;
		}
		return points.ToArray();
	}

	public override Vector3[] GetViewPoints()
	{
		List<Vector3> list = new List<Vector3>();
		if (!m_sections[1].IsInSmoke())
		{
			list.Add(m_sections[1].GetCenter());
		}
		if (!m_sections[0].IsInSmoke())
		{
			list.Add(m_sections[0].GetCenter());
		}
		if (!m_sections[2].IsInSmoke())
		{
			list.Add(m_sections[2].GetCenter());
		}
		Vector3[] array = list.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].y < m_deckHeight)
			{
				array[i].y = m_deckHeight;
			}
		}
		return array;
	}

	public override bool IsInSmoke()
	{
		foreach (Section section in m_sections)
		{
			if (!section.IsInSmoke())
			{
				return false;
			}
		}
		return true;
	}

	public override void SetOwner(int owner)
	{
		base.SetOwner(owner);
		foreach (Section section in m_sections)
		{
			if (section != null)
			{
				section.SetOwner(owner);
			}
		}
		SetupShipColor();
	}

	private void SetupShipColor()
	{
		if ((bool)m_shipMaterial)
		{
			Color primaryColor = Color.white;
			if (TurnMan.instance != null)
			{
				TurnMan.instance.GetPlayerColors(GetOwner(), out primaryColor);
			}
			m_shipMaterial.SetColor("_TeamColor0", primaryColor);
		}
	}

	public void SetHighlight(bool enabled)
	{
		if ((bool)m_shipMaterial)
		{
			m_shipMaterial.SetFloat("_Highlight", (!enabled) ? 0f : 0.3f);
		}
	}

	protected override bool CanMove(Vector3 from, Vector3 to)
	{
		Vector3 normalized = (to - from).normalized;
		int num = 0;
		num = ((!m_deepKeel) ? (1 << LayerMask.NameToLayer("Default")) : ((1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("shallow")) | (1 << LayerMask.NameToLayer("bottom_raytrace_only"))));
		float distance = Vector3.Distance(from, to);
		if (Physics.SphereCast(from, m_Width / 2f, normalized, out var _, distance, num))
		{
			return false;
		}
		return true;
	}

	public void UpdateStats()
	{
		float num = 0f;
		m_maxSpeed = m_baseSettings.m_speed;
		m_maxReverseSpeed = m_baseSettings.m_reverseSpeed;
		m_maxTurnSpeed = m_baseSettings.m_turnSpeed;
		m_acceleration = m_baseSettings.m_acceleration;
		m_reverseAcceleration = m_baseSettings.m_reverseAcceleration;
		m_breakAcceleration = m_baseSettings.m_breakAcceleration;
		num = m_baseSettings.m_sightRange;
		m_maxHealth = m_baseSettings.m_maxHealth;
		foreach (Section section in m_sections)
		{
			if (section != null)
			{
				m_acceleration += section.m_modifiers.m_acceleration;
				m_reverseAcceleration += section.m_modifiers.m_reverseAcceleration;
				m_breakAcceleration += section.m_modifiers.m_breakAcceleration;
				m_maxTurnSpeed += section.m_modifiers.m_turnSpeed;
				m_maxSpeed += section.m_modifiers.m_speed;
				m_maxReverseSpeed += section.m_modifiers.m_reverseSpeed;
				num += section.m_modifiers.m_sightRange;
				m_maxHealth += section.m_maxHealth;
			}
		}
		SetSightRange(num);
	}

	public void ResetStats()
	{
		m_health = m_maxHealth;
	}

	public override void SetSelected(bool selected, bool explicitSelected)
	{
		base.SetSelected(selected, explicitSelected);
	}

	private void OnCollisionStay(Collision collision)
	{
		Vector3 vector = Vector3.zero;
		Vector3 zero = Vector3.zero;
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			vector += contactPoint.normal;
			zero += contactPoint.point;
		}
		vector.y = 0f;
		vector.Normalize();
		zero /= (float)collision.contacts.Length;
		zero.y = m_realPos.y;
		float num = 99999f;
		float num2 = 0f;
		Vector3 rhs = Vector3.zero;
		Section component = collision.contacts[0].thisCollider.GetComponent<Section>();
		if (!component && collision.contacts[0].thisCollider.transform.parent != null)
		{
			component = collision.contacts[0].thisCollider.transform.parent.GetComponent<Section>();
		}
		Ship component2 = collision.gameObject.GetComponent<Ship>();
		if (component2 != null)
		{
			if (component != null)
			{
				Vector3 lhs = base.transform.position - collision.gameObject.transform.position;
				if (Vector3.Dot(lhs, vector) < 0f)
				{
					vector = -vector;
				}
				num2 = component2.GetMoment(zero, vector);
				num = component2.m_mass;
				rhs = component2.GetVelAtPoint(zero) - GetVelAtPoint(zero);
				if (m_collisionDamageTimer <= 0f)
				{
					m_collisionDamageTimer = 0.2f;
					Hit hit = new Hit(component2, 10, component.m_armorClass, zero, Vector3.zero);
					hit.m_collision = true;
					component.Damage(hit);
					if (IsVisible() && m_hitEffect != null)
					{
						UnityEngine.Object.Instantiate(m_hitEffect, zero, Quaternion.identity);
					}
				}
			}
		}
		else if (component != null)
		{
			rhs = -GetVelAtPoint(zero);
			if (m_collisionDamageTimer <= 0f)
			{
				m_collisionDamageTimer = 0.2f;
				Hit hit2 = new Hit(10, component.m_armorClass, zero, Vector3.zero);
				hit2.m_collision = true;
				component.Damage(hit2);
				if (IsVisible() && m_hitEffect != null)
				{
					UnityEngine.Object.Instantiate(m_hitEffect, zero, Quaternion.identity);
				}
			}
		}
		float moment = GetMoment(zero, vector);
		float num3 = Vector3.Dot(vector, rhs) / (1f / m_mass + 1f / num + (moment + num2));
		if (num3 < 0f)
		{
			num3 *= -1f;
		}
		num3 = Mathf.Clamp(num3, 50f, 200f);
		ApplyImpulse(zero, vector * num3, separate: true);
	}

	private Vector3 GetVelAtPoint(Vector3 worldPos)
	{
		Vector3 rhs = worldPos - m_realPos;
		Vector3 lhs = new Vector3(0f, m_rotVelocity * ((float)Math.PI / 180f), 0f);
		Vector3 vector = Vector3.Cross(lhs, rhs);
		return m_velocity + vector;
	}

	private float GetMoment(Vector3 worldPos, Vector3 dir)
	{
		Vector3 lhs = worldPos - m_realPos;
		return Vector3.Cross(lhs, dir).magnitude / m_mass;
	}

	public void ApplyImpulse(Vector3 worldPos, Vector3 worldDir, bool separate)
	{
		Quaternion quaternion = Quaternion.Inverse(m_realRot);
		Vector3 lhs = quaternion * (worldPos - m_realPos);
		Vector3 rhs = quaternion * worldDir;
		m_velocity += worldDir * (1f / m_mass);
		if (separate)
		{
			float magnitude = m_velocity.magnitude;
			if (magnitude < 0.2f)
			{
				m_velocity = m_velocity.normalized * 0.2f;
			}
			if (magnitude > 2f)
			{
				m_velocity = m_velocity.normalized * 2f;
			}
		}
		Vector3 vector = Vector3.Cross(lhs, rhs);
		float num = m_mass * m_length * m_length / 12f;
		float num2 = m_mass * m_length * m_length / 500f;
		float mass = m_mass;
		m_rotVelocity += vector.y / num * 57.29578f;
		m_rockVelocityX += vector.x / num2 * 57.29578f;
		m_rockVelocityZ += vector.z / mass * 57.29578f;
		m_rockVelocityX = Mathf.Clamp(m_rockVelocityX, 0f - m_maxRockVel, m_maxRockVel);
		m_rockVelocityZ = Mathf.Clamp(m_rockVelocityZ, 0f - m_maxRockVel, m_maxRockVel);
	}

	public int GetBatterySlots()
	{
		int num = 0;
		num += GetSectionFront().GetBatterySlots();
		num += GetSectionMid().GetBatterySlots();
		num += GetSectionRear().GetBatterySlots();
		return num + GetSectionTop().GetBatterySlots();
	}

	public override void SetVisible(bool visible)
	{
		if (IsVisible() == visible)
		{
			return;
		}
		base.SetVisible(visible);
		Trace[] componentsInChildren = base.gameObject.GetComponentsInChildren<Trace>(includeInactive: true);
		Trace[] array = componentsInChildren;
		foreach (Trace trace in array)
		{
			trace.SetVisible(visible);
		}
		Wake[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<Wake>(includeInactive: true);
		Wake[] array2 = componentsInChildren2;
		foreach (Wake wake in array2)
		{
			wake.SetVisible(visible);
		}
		for (int k = 0; k < base.transform.childCount; k++)
		{
			Transform child = base.transform.GetChild(k);
			if ((bool)child.renderer)
			{
				child.renderer.renderer.enabled = visible;
			}
		}
		foreach (Section section in m_sections)
		{
			section.SetVisible(visible);
		}
		if (IsKing() && GetObjectiveIcon() != null)
		{
			GetObjectiveIcon().renderer.enabled = true;
		}
		m_damageMan.SetVisible(visible);
		if (m_dead)
		{
			DisableAOPlane();
		}
	}

	public override float GetLength()
	{
		return m_length;
	}

	public override float GetWidth()
	{
		return m_Width;
	}

	public override Vector3 GetVelocity()
	{
		return m_velocity;
	}

	public override bool IsTakingWater()
	{
		return m_sinkTimer >= 0f;
	}

	public bool IsEngineDamaged()
	{
		return m_engineDamagedTimer > 0f;
	}

	public bool IsBridgeDamaged()
	{
		return m_bridgeDamagedTimer > 0f;
	}

	public bool IsOutOfControl()
	{
		return m_outOfControlTimer > 0f;
	}

	public float GetEngineRepairTime()
	{
		return m_engineDamagedTimer;
	}

	public float GetBridgeRepairTime()
	{
		return m_bridgeDamagedTimer;
	}

	public float GetControlRepairTime()
	{
		return m_outOfControlTimer;
	}

	public float GetGroundedTime()
	{
		return m_groundedTimer;
	}

	public float GetTimeToSink()
	{
		return m_sinkTimer;
	}

	public override bool IsSinking()
	{
		return m_sinking != SinkStyle.None;
	}

	public bool IsSupplied()
	{
		return m_suppliedTimer > 0f;
	}

	public bool IsAutoRepairing()
	{
		if (IsDead() || IsTakingWater())
		{
			return false;
		}
		return (float)m_health < (float)m_maxHealth * m_autoRepairTreshold;
	}

	private void UpdateAutoRepair(float dt)
	{
		if (IsAutoRepairing())
		{
			m_autoRepairTimer += dt;
			if (m_autoRepairTimer >= 1f)
			{
				m_autoRepairTimer = 0f;
				Heal(m_autoRepairAmount);
			}
		}
	}

	public void SetRequestedMaintenanceMode(bool enabled)
	{
		GameType gameType = TurnMan.instance.GetGameType();
		if (gameType != GameType.Campaign && gameType != GameType.Challenge && !m_king && m_maintenanceTimer < 0f)
		{
			m_requestedMaintenanceMode = enabled;
		}
	}

	public bool GetRequestedMaintenanceMode()
	{
		return m_requestedMaintenanceMode;
	}

	public override bool IsDoingMaintenance()
	{
		return m_maintenanceMode || m_requestedMaintenanceMode;
	}

	public bool GetCurrentMaintenanceMode()
	{
		return m_maintenanceMode;
	}

	public float GetMaintenanceTimer()
	{
		if (m_maintenanceTimer < 0f)
		{
			if (m_maintenanceMode != m_requestedMaintenanceMode)
			{
				return 0f;
			}
			return -1f;
		}
		float num = ((!m_requestedMaintenanceMode) ? 6f : 1f);
		return m_maintenanceTimer / num;
	}

	private void UpdateMaintenance(float dt)
	{
		if (m_maintenanceMode)
		{
			m_maintenanceHealTimer -= dt;
			if (m_maintenanceHealTimer <= 0f)
			{
				m_maintenanceHealTimer = 1f;
				int resources = 10000;
				Supply(ref resources);
				if (resources == 10000)
				{
					SetRequestedMaintenanceMode(enabled: false);
				}
			}
		}
		if (m_maintenanceMode == m_requestedMaintenanceMode)
		{
			return;
		}
		if (m_maintenanceTimer < 0f)
		{
			m_maintenanceTimer = 0f;
			if (m_requestedMaintenanceMode)
			{
				if (m_onMaintenanceActivation != null)
				{
					m_onMaintenanceActivation();
				}
				ClearOrders();
			}
		}
		else
		{
			m_maintenanceTimer += dt;
			float num = ((!m_requestedMaintenanceMode) ? 6f : 1f);
			if (m_maintenanceTimer >= num)
			{
				m_maintenanceTimer = -1f;
				InternalSetMaintenanceMode(m_requestedMaintenanceMode);
			}
		}
	}

	private void InternalSetMaintenanceMode(bool enabled)
	{
		m_maintenanceMode = enabled;
	}

	public void Heal(int health)
	{
		if (!m_dead)
		{
			m_health += health;
			if (m_health > m_maxHealth)
			{
				m_health = m_maxHealth;
			}
			if (m_sinkTimer >= 0f && m_health > m_sinkTreshold)
			{
				m_sinkTimer = -1f;
			}
			m_damageMan.HealDamageEffects(health * 2);
			m_damageMan.OnShipHealthChanged((float)m_health / (float)m_maxHealth);
		}
	}

	public override void Supply(ref int resources)
	{
		if (m_dead || resources <= 0)
		{
			return;
		}
		int num = resources;
		if (resources > 12 && m_health < m_maxHealth)
		{
			resources -= 12;
			Heal(10);
		}
		foreach (Section section in m_sections)
		{
			section.Supply(ref resources);
		}
		if (resources < num)
		{
			m_suppliedTimer = 2.1f;
		}
	}

	public Route GetRoute()
	{
		return m_route;
	}

	public void SetPath(GameObject path)
	{
		m_path = path;
	}

	public GameObject GetPath()
	{
		if (m_path != null)
		{
			return m_path;
		}
		if (m_pathNetID == 0)
		{
			return null;
		}
		m_path = NetObj.GetByID(m_pathNetID).gameObject;
		return m_path;
	}

	public override int GetHealth()
	{
		return m_health;
	}

	public override int GetMaxHealth()
	{
		return m_maxHealth;
	}

	public string GetClassName()
	{
		return m_displayClassName;
	}

	public Vector3 GetRealPos()
	{
		return m_realPos;
	}

	public Quaternion GetRealRot()
	{
		return m_realRot;
	}

	public bool IsWater(Vector3 position)
	{
		NavMeshHit hit;
		return NavMesh.SamplePosition(base.transform.position, out hit, 5f, 1);
	}

	protected bool CanMove2(Vector3 from, Vector3 to)
	{
		Vector3 normalized = (to - from).normalized;
		from += normalized * GetLength() * 0.75f;
		from.y += 1f;
		to.y += 1f;
		int layerMask = 256;
		RaycastHit hitInfo;
		bool flag = Physics.Linecast(from, to, out hitInfo, layerMask);
		Debug.DrawLine(from, to, Color.green, 0f, depthTest: false);
		if (flag)
		{
			PLog.Log("CanMove2 blocked by: " + hitInfo.collider.gameObject.ToString());
		}
		return !flag;
	}

	public bool IsPathBlocked()
	{
		Vector3 from = base.transform.position;
		foreach (Order order in m_orders)
		{
			if (order.m_type == Order.Type.MoveForward || order.m_type == Order.Type.MoveBackward)
			{
				Vector3 pos = order.GetPos();
				if (!CanMove2(from, pos))
				{
					return true;
				}
				from = pos;
			}
		}
		return false;
	}

	public bool IsBlocked()
	{
		Vector3 vector = base.transform.position + base.transform.forward * GetLength() * 0.6f;
		Vector3 end = vector + base.transform.forward * GetLength();
		int layerMask = 256;
		RaycastHit hitInfo;
		bool result = Physics.Linecast(vector, end, out hitInfo, layerMask);
		Debug.DrawLine(vector, end, Color.yellow, 0f, depthTest: false);
		return result;
	}

	public void SetOrdersTo(Vector3 position)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMesh.CalculatePath(base.transform.position, position, 1, navMeshPath);
		ClearMoveOrders();
		for (int i = 0; i < navMeshPath.corners.Length; i++)
		{
			Order order = new Order(this, Order.Type.MoveForward, navMeshPath.corners[i]);
			AddOrder(order);
		}
	}

	public void DebugDrawRoute()
	{
		Route route = GetRoute();
		if (route.NrOfWaypoints() != 0)
		{
			List<Vector3> positions = route.GetPositions();
			Debug.DrawLine(base.transform.position, positions[0], Color.yellow, 0f, depthTest: false);
			Vector3 vector = new Vector3(0f, 2f, 0f);
			for (int i = 0; i < positions.Count - 1; i++)
			{
				Debug.DrawLine(positions[i] + vector, positions[i + 1] + vector, Color.red, 0f, depthTest: false);
			}
		}
	}

	public virtual Dictionary<string, string> GetShipEditorInfo()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["Sight Range"] = GetSightRange().ToString();
		dictionary["Speed"] = GetMaxSpeed().ToString();
		dictionary["Turn Speedua"] = GetMaxReverseSpeed().ToString();
		return dictionary;
	}

	public ShipAi GetShipAi()
	{
		return m_Ai as ShipAi;
	}

	public ShipAISettings GetAiSettings()
	{
		return m_aiSettings;
	}

	public void SetAiSettings(ShipAISettings aiSetting)
	{
		m_aiSettings.Transfer(aiSetting);
	}

	public bool IsPositionForward(Vector3 position)
	{
		float distanceToPoint = new Plane(base.transform.forward, base.transform.position).GetDistanceToPoint(position);
		if (distanceToPoint >= 0f)
		{
			return true;
		}
		return false;
	}

	public bool IsPositionRight(Vector3 position)
	{
		float distanceToPoint = new Plane(base.transform.right, base.transform.position).GetDistanceToPoint(position);
		if (distanceToPoint >= 0f)
		{
			return true;
		}
		return false;
	}

	public void SetInMonsterMineField(GameObject monsterMine)
	{
		m_monsterMine = monsterMine;
		m_inMonsterMineTimer = 1f;
	}

	public float GetMaxSpeed()
	{
		return m_maxSpeed;
	}

	public float GetMaxReverseSpeed()
	{
		return m_maxReverseSpeed;
	}
}
