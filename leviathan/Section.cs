#define DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Section : MonoBehaviour
{
	public enum SectionType
	{
		Front,
		Mid,
		Rear,
		Top
	}

	[Serializable]
	public class ModifierSettings
	{
		public float m_speed;

		public float m_reverseSpeed;

		public float m_acceleration;

		public float m_reverseAcceleration;

		public float m_turnSpeed;

		public float m_sightRange;

		public float m_breakAcceleration;
	}

	[Serializable]
	public class SectionRating
	{
		public int m_health = 3;

		public int m_armor = 3;

		public int m_speed = 3;

		public int m_sight = 3;
	}

	public string m_series;

	public bool m_defaultSection;

	public SectionType m_type;

	public int m_maxHealth = 100;

	public int m_armorClass = 10;

	public ModifierSettings m_modifiers;

	public Texture2D m_GUITexture;

	public GameObject m_explosionPrefab;

	public GameObject m_explosionLowPrefab;

	public SectionSettings m_settings;

	private int m_owner = -1;

	private Ship m_unit;

	private float m_inSmokeTimer = -1f;

	private float m_explodeTimer = -1f;

	private Vector3 m_explosionPos = new Vector3(0f, 0f, 0f);

	public SectionRating m_rating = new SectionRating();

	private void Awake()
	{
		m_settings = ComponentDB.instance.GetSection(base.name.Substring(0, base.name.Length - 7));
		DebugUtils.Assert(m_settings != null);
	}

	public Vector3 GetCenter()
	{
		if (base.collider != null)
		{
			return base.collider.bounds.center;
		}
		LODGroup component = GetComponent<LODGroup>();
		if (component != null)
		{
			return base.transform.TransformPoint(component.localReferencePoint);
		}
		if (base.renderer != null)
		{
			return base.renderer.bounds.center;
		}
		PLog.LogError("Failed to aquire any valid center point of section " + base.name);
		return base.transform.position;
	}

	public void FixedUpdate()
	{
		if (!NetObj.IsSimulating())
		{
			return;
		}
		if (m_inSmokeTimer >= 0f)
		{
			m_inSmokeTimer += Time.fixedDeltaTime;
		}
		if (!(m_explodeTimer >= 0f))
		{
			return;
		}
		m_explodeTimer -= Time.fixedDeltaTime;
		if (!(m_explodeTimer <= 0f))
		{
			return;
		}
		m_explodeTimer = -1f;
		if (m_unit.IsVisible())
		{
			GameObject explosionPrefab = m_explosionPrefab;
			if (explosionPrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(explosionPrefab, base.transform.localToWorldMatrix.MultiplyPoint3x4(m_explosionPos), Quaternion.identity) as GameObject;
				gameObject.transform.parent = base.transform;
			}
		}
	}

	public void Setup(Ship unit)
	{
		m_unit = unit;
		int num = 0;
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.Setup(m_unit, num);
			num++;
		}
	}

	public int GetBatterySlots()
	{
		int num = 0;
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			num += battery.GetSlots();
		}
		return num;
	}

	public void GetAllHPModules(ref List<HPModule> modules)
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.GetAllHPModules(ref modules);
		}
	}

	public Material GetMaterial()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			Renderer renderer = child.renderer;
			if (renderer != null)
			{
				return UnityEngine.Object.Instantiate(renderer.sharedMaterial) as Material;
			}
			Animation animation = child.animation;
			if (!animation)
			{
				continue;
			}
			for (int j = 0; j < child.transform.childCount; j++)
			{
				Transform child2 = child.transform.GetChild(j);
				Renderer renderer2 = child2.renderer;
				if (renderer2 != null)
				{
					return UnityEngine.Object.Instantiate(renderer2.sharedMaterial) as Material;
				}
			}
		}
		return null;
	}

	public void SetMaterial(Material material)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			Renderer renderer = child.renderer;
			if (renderer != null)
			{
				Material[] array = new Material[renderer.sharedMaterials.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = material;
				}
				renderer.sharedMaterials = array;
			}
			Animation animation = child.animation;
			if (!animation)
			{
				continue;
			}
			for (int k = 0; k < child.transform.childCount; k++)
			{
				Transform child2 = child.transform.GetChild(k);
				Renderer renderer2 = child2.renderer;
				if (renderer2 != null)
				{
					Material[] array2 = new Material[renderer2.sharedMaterials.Length];
					for (int l = 0; l < renderer2.materials.Length; l++)
					{
						array2[l] = material;
					}
					renderer2.sharedMaterials = array2;
				}
			}
		}
	}

	public void SetOwner(int owner)
	{
		m_owner = owner;
		Battery[] componentsInChildren = GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.SetOwner(owner);
		}
	}

	public void Supply(ref int resources)
	{
		HPModule[] componentsInChildren = GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			hPModule.Supply(ref resources);
		}
	}

	public SectionType GetSectionType()
	{
		return m_type;
	}

	public Battery GetBattery(int id)
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		if (id < 0 || id >= componentsInChildren.Length)
		{
			return null;
		}
		return componentsInChildren[id];
	}

	public void SaveState(BinaryWriter writer)
	{
		writer.Write(m_inSmokeTimer);
		writer.Write(m_explodeTimer);
		writer.Write(m_explosionPos.x);
		writer.Write(m_explosionPos.y);
		writer.Write(m_explosionPos.z);
		writer.Write(IsColliderEnabled());
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		writer.Write((byte)componentsInChildren.Length);
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.SaveState(writer);
		}
	}

	public void LoadState(BinaryReader reader)
	{
		m_inSmokeTimer = reader.ReadSingle();
		m_explodeTimer = reader.ReadSingle();
		m_explosionPos.x = reader.ReadSingle();
		m_explosionPos.y = reader.ReadSingle();
		m_explosionPos.z = reader.ReadSingle();
		bool colliderEnabled = reader.ReadBoolean();
		SetColliderEnabled(colliderEnabled);
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		int num = reader.ReadByte();
		DebugUtils.Assert(num == componentsInChildren.Length, "number of batteries missmatch");
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.LoadState(reader);
		}
	}

	public void SaveOrders(BinaryWriter writer)
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		writer.Write(componentsInChildren.Length);
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.SaveOrders(writer);
		}
	}

	public void LoadOrders(BinaryReader reader)
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		int num = reader.ReadInt32();
		DebugUtils.Assert(componentsInChildren.Length == num);
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.LoadOrders(reader);
		}
	}

	public void ClearOrders()
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.ClearOrders();
		}
	}

	public Unit GetUnit()
	{
		return m_unit;
	}

	public int GetOwner()
	{
		return m_owner;
	}

	public bool Damage(Hit hit)
	{
		return m_unit.Damage(hit, this);
	}

	public void SetVisible(bool visible)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.animation != null && child.childCount > 0)
			{
				for (int j = 0; j < child.childCount; j++)
				{
					if ((bool)child.GetChild(j).renderer)
					{
						child.GetChild(j).renderer.enabled = visible;
					}
				}
			}
			else if (child.renderer != null)
			{
				child.renderer.enabled = visible;
			}
		}
	}

	public void Explode()
	{
		m_explosionPos = base.transform.worldToLocalMatrix.MultiplyPoint3x4(GetCenter());
		m_explodeTimer = UnityEngine.Random.Range(1.5f, 4f);
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.DestroyAll();
		}
	}

	public void OnKilled()
	{
		SetColliderEnabled(enabled: false);
	}

	private bool IsColliderEnabled()
	{
		if (base.collider != null)
		{
			return base.collider.enabled;
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Collider collider = base.transform.GetChild(i).collider;
			if (collider != null)
			{
				return collider.enabled;
			}
		}
		return false;
	}

	private void SetColliderEnabled(bool enabled)
	{
		if (base.collider != null)
		{
			base.collider.enabled = enabled;
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Collider collider = base.transform.GetChild(i).collider;
			if (collider != null)
			{
				collider.enabled = enabled;
			}
		}
	}

	public int GetTotalValue()
	{
		int num = m_settings.m_value;
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			num += battery.GetTotalValue();
		}
		return num;
	}

	public int GetValue()
	{
		return m_settings.m_value;
	}

	public void OnSmokeEnter()
	{
		m_inSmokeTimer = 0f;
	}

	public bool IsInSmoke()
	{
		return m_inSmokeTimer >= 0f && m_inSmokeTimer < 0.5f;
	}

	public void OnSetSimulating(bool enabled)
	{
	}

	public virtual Dictionary<string, string> GetShipEditorInfo()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["Health"] = m_maxHealth.ToString();
		dictionary["AC"] = m_armorClass.ToString();
		dictionary["Forward Speed"] = m_modifiers.m_speed.ToString();
		dictionary["Reverse Speed"] = m_modifiers.m_reverseSpeed.ToString();
		dictionary["Sight Range"] = m_modifiers.m_sightRange.ToString();
		return dictionary;
	}

	public string GetName()
	{
		return Localize.instance.Translate("$" + base.name + "_name");
	}

	public void ClearGunOrdersAndTargets()
	{
		Battery[] componentsInChildren = base.transform.GetComponentsInChildren<Battery>();
		Battery[] array = componentsInChildren;
		foreach (Battery battery in array)
		{
			battery.ClearGunOrdersAndTargets();
		}
	}
}
