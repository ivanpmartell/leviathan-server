using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Battery : MonoBehaviour
{
	private const float m_tileSize = 1f;

	private Unit m_unit;

	private int m_owner = -1;

	private int m_orderNumber;

	private HPModule[] m_modules;

	public int m_width = 1;

	public int m_length = 1;

	public bool m_allowOffensive = true;

	public bool m_allowDefensive = true;

	private void Awake()
	{
		SetSize(m_width, m_length);
	}

	public void SetSize(int width, int length)
	{
		m_width = width;
		m_length = length;
		m_modules = new HPModule[m_width * m_length];
		Transform child = base.transform.GetChild(0);
		child.localScale = new Vector3(m_width, child.localScale.y, m_length);
	}

	public int GetWidth()
	{
		return m_width;
	}

	public int GetLength()
	{
		return m_length;
	}

	public int GetSlots()
	{
		return m_width * m_length;
	}

	public void Setup(Unit unit, int orderNumber)
	{
		m_unit = unit;
		m_orderNumber = orderNumber;
	}

	public void SetOwner(int owner)
	{
		m_owner = owner;
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			hPModule.SetOwner(owner);
		}
	}

	public Section.SectionType GetSectionType()
	{
		Section component = base.transform.parent.gameObject.GetComponent<Section>();
		return component.GetSectionType();
	}

	public Section GetSection()
	{
		return base.transform.parent.gameObject.GetComponent<Section>();
	}

	public void FillUsableBuffer(bool[] usable, int px, int py, int w, int h)
	{
		if (px < 0 || px + w > m_width || py < 0 || py + h > m_length)
		{
			return;
		}
		for (int i = py; i < py + h; i++)
		{
			for (int j = px; j < px + w; j++)
			{
				usable[i * m_width + j] = true;
			}
		}
	}

	public void GetFitTiles(out List<Vector3> points, int w, int h, bool fillall)
	{
		points = new List<Vector3>();
		bool[] array = new bool[m_length * m_width];
		for (int i = 0; i < m_length; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				if (CanPlaceAt(j, i, w, h, null))
				{
					array[i * m_width + j] = true;
					if (fillall)
					{
						FillUsableBuffer(array, j, i, w, h);
					}
				}
				if (array[i * m_width + j])
				{
					Vector3 position = GetTileTopLeft(j, i) + new Vector3(0.5f, 0f, 0.5f);
					position = base.transform.TransformPoint(position);
					points.Add(position);
				}
			}
		}
	}

	public void GetBestFitTiles(out List<Vector3> points, Vector3 pos, int w, int h)
	{
		points = new List<Vector3>();
	}

	public void GetUnusedTiles(out List<Vector3> points)
	{
		points = new List<Vector3>();
		for (int i = 0; i < m_length; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				if (m_modules[i * m_width + j] == null)
				{
					Vector3 position = GetTileTopLeft(j, i) + new Vector3(0.5f, 0f, 0.5f);
					position = base.transform.TransformPoint(position);
					points.Add(position);
				}
			}
		}
	}

	public Vector3 GetModulePosition(int x, int y, int w, int h)
	{
		return GetTileTopLeft(x, y) + new Vector3((float)w * 1f * 0.5f, 0f, (float)h * 1f * 0.5f);
	}

	public static Quaternion GetRotation(Direction dir)
	{
		Quaternion result = Quaternion.Euler(0f, 0f, 0f);
		switch (dir)
		{
		case Direction.Right:
			result = Quaternion.Euler(0f, 90f, 0f);
			break;
		case Direction.Backward:
			result = Quaternion.Euler(0f, 180f, 0f);
			break;
		case Direction.Left:
			result = Quaternion.Euler(0f, 270f, 0f);
			break;
		}
		return result;
	}

	public HPModule AddHPModule(string name, int x, int y, Direction dir)
	{
		GameObject gameObject = ObjectFactory.instance.Create(name);
		if (gameObject == null)
		{
			PLog.LogError("Failed to AddHPModule " + name);
		}
		HPModule component = gameObject.GetComponent<HPModule>();
		component.SetDir(dir);
		if (!AllowedModule(component))
		{
			PLog.LogError("Error AddHPModule. Module " + name + " not allowed in battery on ship " + m_unit.GetName());
			Object.Destroy(gameObject);
			return null;
		}
		if (!CanPlaceAt(x, y, component.GetWidth(), component.GetLength(), null))
		{
			string text = "<" + x + "," + y + ">";
			PLog.LogError("Error AddHPModule. Tried to place " + name + " outside battery at " + text + " on ship " + m_unit.GetName());
			Object.Destroy(gameObject);
			return null;
		}
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = GetLocalPlacePos(x, y, component);
		gameObject.transform.localRotation = Quaternion.identity;
		component.Setup(m_unit, this, x, y, dir, OnModuleDestroyed);
		component.SetOwner(m_owner);
		AddModuleToGrid(component);
		return component;
	}

	private Vector3 GetLocalPlacePos(int x, int y, HPModule module)
	{
		return GetTileTopLeft(x, y) + new Vector3((float)module.GetWidth() * 1f * 0.5f, 0f, (float)module.GetLength() * 1f * 0.5f);
	}

	public Vector3 GetWorldPlacePos(int x, int y, HPModule module)
	{
		Vector3 localPlacePos = GetLocalPlacePos(x, y, module);
		return base.transform.TransformPoint(localPlacePos);
	}

	public void GetAllHPModules(ref List<HPModule> modules)
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule item in array)
		{
			modules.Add(item);
		}
	}

	public bool AllowedModule(HPModule module)
	{
		return module.m_type switch
		{
			HPModule.HPModuleType.Defensive => m_allowDefensive, 
			HPModule.HPModuleType.Offensive => m_allowOffensive, 
			_ => false, 
		};
	}

	private void OnModuleDestroyed(HPModule module)
	{
		RemoveModuleFromGrid(module);
	}

	private void RemoveModuleFromGrid(HPModule module)
	{
		for (int i = 0; i < m_modules.Length; i++)
		{
			if (m_modules[i] == module)
			{
				m_modules[i] = null;
			}
		}
	}

	private void AddModuleToGrid(HPModule module)
	{
		Vector2i gridPos = module.GetGridPos();
		for (int i = 0; i < module.GetLength(); i++)
		{
			for (int j = 0; j < module.GetWidth(); j++)
			{
				m_modules[(gridPos.y + i) * m_width + (gridPos.x + j)] = module;
			}
		}
	}

	public bool CanPlaceAt(int px, int py, int w, int h, HPModule ignoreModule)
	{
		if (px < 0 || px + w > m_width || py < 0 || py + h > m_length)
		{
			return false;
		}
		for (int i = py; i < py + h; i++)
		{
			for (int j = px; j < px + w; j++)
			{
				HPModule hPModule = m_modules[i * m_width + j];
				if (hPModule != null && !(hPModule == ignoreModule))
				{
					return false;
				}
			}
		}
		return true;
	}

	public HPModule GetModuleAt(int x, int y)
	{
		return m_modules[y * m_width + x];
	}

	public void SaveState(BinaryWriter writer)
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		writer.Write((byte)componentsInChildren.Length);
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			writer.Write(hPModule.gameObject.name);
			writer.Write((byte)hPModule.GetGridPos().x);
			writer.Write((byte)hPModule.GetGridPos().y);
			writer.Write((byte)hPModule.GetDir());
			hPModule.SaveState(writer);
		}
	}

	public void LoadState(BinaryReader reader)
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			Object.DestroyImmediate(hPModule.gameObject);
		}
		int num = reader.ReadByte();
		for (int j = 0; j < num; j++)
		{
			string text = reader.ReadString();
			int x = reader.ReadByte();
			int y = reader.ReadByte();
			Direction dir = (Direction)reader.ReadByte();
			HPModule hPModule2 = AddHPModule(text, x, y, dir);
			hPModule2.LoadState(reader);
		}
	}

	public void SaveOrders(BinaryWriter writer)
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		writer.Write(componentsInChildren.Length);
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter stream = new BinaryWriter(memoryStream);
			hPModule.SaveOrders(stream);
			byte[] array2 = memoryStream.ToArray();
			writer.Write(hPModule.GetNetID());
			writer.Write(array2.Length);
			writer.Write(array2);
		}
	}

	public void LoadOrders(BinaryReader reader)
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = reader.ReadInt32();
			int count = reader.ReadInt32();
			byte[] buffer = reader.ReadBytes(count);
			HPModule[] array = componentsInChildren;
			foreach (HPModule hPModule in array)
			{
				if (hPModule.GetNetID() == num2)
				{
					MemoryStream input = new MemoryStream(buffer);
					BinaryReader stream = new BinaryReader(input);
					hPModule.LoadOrders(stream);
					break;
				}
			}
		}
	}

	public void ClearOrders()
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			hPModule.ClearOrders();
		}
	}

	public void ClearGunOrdersAndTargets()
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			Gun gun = hPModule as Gun;
			if (gun != null)
			{
				gun.SetTarget(null);
				gun.ClearOrders();
			}
		}
	}

	public int GetOrderNumber()
	{
		return m_orderNumber;
	}

	public void SetVisible(bool visible)
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			hPModule.SetVisible(visible);
		}
	}

	public void DestroyAll()
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			hPModule.TimedDestruction(PRand.Range(0.4f, 3f));
		}
	}

	public void RemoveAll()
	{
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			hPModule.Remove();
		}
	}

	public int GetTotalValue()
	{
		int num = 0;
		HPModule[] componentsInChildren = base.transform.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			num += hPModule.GetTotalValue();
		}
		return num;
	}

	private Vector3 GetTileTopLeft(int x, int y)
	{
		return new Vector3(((float)(-m_width) / 2f + (float)x) * 1f, 0f, (float)(-m_length) / 2f + (float)y) * 1f;
	}

	public bool WorldToTile(Vector3 worldPos, out int x, out int y)
	{
		Vector3 vector = base.transform.InverseTransformPoint(worldPos);
		vector.x += (float)m_width / 2f * 1f;
		vector.z += (float)m_length / 2f * 1f;
		x = (int)(vector.x / 1f);
		y = (int)(vector.z / 1f);
		if (x < 0 || y < 0 || x >= m_width || y >= m_length)
		{
			return false;
		}
		return true;
	}
}
