using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

internal class Utils
{
	public enum ValidationStatus
	{
		Ok,
		ToShort,
		InvalidCharacter
	}

	private static bool m_androidBack;

	public static long GetTimeMS()
	{
		return DateTime.Now.Ticks / 10000;
	}

	public static bool IsEmailAddress(string email)
	{
		if (email.Contains("..") || email.Contains("...") || email.Contains("...."))
		{
			return false;
		}
		Regex regex = new Regex("^([\\w\\.\\-]+)@([\\w\\-]+)((\\.(\\w){2,3})+)$");
		return regex.IsMatch(email);
	}

	public static List<string> GetDistinctList(List<string> data)
	{
		HashSet<string> collection = new HashSet<string>(data);
		return new List<string>(collection);
	}

	public static List<int> GetDistinctList(List<int> data)
	{
		HashSet<int> collection = new HashSet<int>(data);
		return new List<int>(collection);
	}

	public static ValidationStatus IsValidUsername(string name)
	{
		if (name.Length < 3 || name.Length > 12)
		{
			return ValidationStatus.ToShort;
		}
		List<char> list = new List<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_".ToCharArray());
		foreach (char item in name)
		{
			if (!list.Contains(item))
			{
				return ValidationStatus.InvalidCharacter;
			}
		}
		return ValidationStatus.Ok;
	}

	public static ValidationStatus IsValidPassword(string name)
	{
		if (name.Length < 6)
		{
			return ValidationStatus.ToShort;
		}
		List<char> list = new List<char>(" abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_#%&$()/?!@+,.=".ToCharArray());
		foreach (char item in name)
		{
			if (!list.Contains(item))
			{
				return ValidationStatus.InvalidCharacter;
			}
		}
		return ValidationStatus.Ok;
	}

	public static string FormatTimeLeftString(double time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		if (timeSpan.Days > 0)
		{
			if (timeSpan.Days == 1)
			{
				return timeSpan.Days + " $commit_timer_day ";
			}
			return timeSpan.Days + " $commit_timer_days ";
		}
		if (timeSpan.Hours > 0)
		{
			if (timeSpan.Hours == 1)
			{
				return timeSpan.Hours + " $commit_timer_hour ";
			}
			return timeSpan.Hours + " $commit_timer_hours ";
		}
		if (timeSpan.Minutes > 0)
		{
			if (timeSpan.Minutes == 1)
			{
				return timeSpan.Minutes + " $commit_timer_minute ";
			}
			return timeSpan.Minutes + " $commit_timer_minutes ";
		}
		if (timeSpan.Seconds > 0)
		{
			if (timeSpan.Seconds == 1)
			{
				return timeSpan.Seconds + " $commit_timer_second ";
			}
			return timeSpan.Seconds + " $commit_timer_seconds ";
		}
		return string.Empty;
	}

	public static void WriteVector3(BinaryWriter writer, Vector3 vector)
	{
		writer.Write(vector.x);
		writer.Write(vector.y);
		writer.Write(vector.z);
	}

	public static Vector3 ReadVector3(BinaryReader reader)
	{
		Vector3 result = default(Vector3);
		result.x = reader.ReadSingle();
		result.y = reader.ReadSingle();
		result.z = reader.ReadSingle();
		return result;
	}

	public static void WriteVector3Nullable(BinaryWriter writer, Vector3? vector)
	{
		if (!vector.HasValue)
		{
			writer.Write(value: false);
			return;
		}
		writer.Write(value: true);
		WriteVector3(writer, vector.Value);
	}

	public static void ReadVector3Nullable(BinaryReader reader, out Vector3? vector)
	{
		vector = null;
		if (reader.ReadBoolean())
		{
			vector = ReadVector3(reader);
		}
	}

	public static void WriteVector3Nullable(BinaryWriter writer, Vector3 vector)
	{
		writer.Write(value: true);
		WriteVector3(writer, vector);
	}

	private static string GetFileName(string name)
	{
		return name;
	}

	public static void DumpMemoryUsage()
	{
		PLog.Log("Dumping memory usage to " + Directory.GetCurrentDirectory() + "memory_*.csv");
		string text = string.Empty;
		int num = 0;
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			int num2 = Profiler.GetRuntimeMemorySize(@object) / 1024;
			if (num2 > 0)
			{
				string text2 = text;
				text = text2 + @object.GetType().ToString() + ";" + @object.name + ";" + num2 + ";\n";
				num += num2;
			}
		}
		text = text + "TOTAL MEMORY;" + num + "\n";
		File.WriteAllText(GetFileName("memory_all.csv"), text);
		num = 0;
		text = string.Empty;
		UnityEngine.Object[] array3 = Resources.FindObjectsOfTypeAll(typeof(Texture));
		UnityEngine.Object[] array4 = array3;
		for (int j = 0; j < array4.Length; j++)
		{
			Texture texture = (Texture)array4[j];
			string text3 = texture.width + "x" + texture.height;
			string text2 = text;
			text = text2 + texture.name + ";" + Profiler.GetRuntimeMemorySize(texture) + ";" + text3 + "\n";
			num += Profiler.GetRuntimeMemorySize(texture);
		}
		text = text + "TOTAL MEMORY;" + num + "\n";
		File.WriteAllText(GetFileName("memory_texture.csv"), text);
		num = 0;
		text = string.Empty;
		UnityEngine.Object[] array5 = Resources.FindObjectsOfTypeAll(typeof(Mesh));
		UnityEngine.Object[] array6 = array5;
		for (int k = 0; k < array6.Length; k++)
		{
			Mesh mesh = (Mesh)array6[k];
			int num3 = mesh.triangles.Length / 3;
			string text2 = text;
			text = text2 + mesh.name + ";" + Profiler.GetRuntimeMemorySize(mesh) + ";" + num3.ToString() + "\n";
			num += Profiler.GetRuntimeMemorySize(mesh);
		}
		text = text + "TOTAL MEMORY;" + num + "\n";
		File.WriteAllText(GetFileName("memory_mesh.csv"), text);
		num = 0;
		text = string.Empty;
		UnityEngine.Object[] array7 = Resources.FindObjectsOfTypeAll(typeof(AudioClip));
		UnityEngine.Object[] array8 = array7;
		for (int l = 0; l < array8.Length; l++)
		{
			AudioClip audioClip = (AudioClip)array8[l];
			string text4 = "L: " + audioClip.length + " Freq: " + audioClip.frequency;
			string text2 = text;
			text = text2 + audioClip.name + ";" + Profiler.GetRuntimeMemorySize(audioClip) + ";" + text4 + "\n";
			num += Profiler.GetRuntimeMemorySize(audioClip);
		}
		text = text + "TOTAL MEMORY;" + num + "\n";
		File.WriteAllText(GetFileName("memory_audio.csv"), text);
	}

	public static XmlDocument[] LoadXmlInDirectory(string dir)
	{
		UnityEngine.Object[] array = Resources.LoadAll(dir, typeof(TextAsset));
		XmlDocument[] array2 = new XmlDocument[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			TextAsset xmlText = array[i] as TextAsset;
			array2[i] = LoadXml(xmlText);
		}
		return array2;
	}

	public static XmlDocument LoadXml(string file)
	{
		TextAsset textAsset = Resources.Load(file) as TextAsset;
		if (textAsset == null)
		{
			return null;
		}
		return LoadXml(textAsset);
	}

	private static XmlDocument LoadXml(TextAsset xmlText)
	{
		//Discarded unreachable code: IL_004f
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = true;
		XmlReader xmlReader = XmlReader.Create(new StringReader(xmlText.text), xmlReaderSettings);
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(xmlReader);
			return xmlDocument;
		}
		catch (XmlException ex)
		{
			PLog.LogError("Parse error " + ex.ToString());
			return null;
		}
	}

	public static PlatformType GetPlatform()
	{
		return Application.platform switch
		{
			RuntimePlatform.Android => PlatformType.Android, 
			RuntimePlatform.IPhonePlayer => PlatformType.Ios, 
			RuntimePlatform.WindowsPlayer => PlatformType.WindowsPC, 
			RuntimePlatform.WindowsEditor => PlatformType.WindowsPC, 
			RuntimePlatform.WindowsWebPlayer => PlatformType.WindowsPC, 
			RuntimePlatform.OSXPlayer => PlatformType.Osx, 
			RuntimePlatform.OSXEditor => PlatformType.Osx, 
			RuntimePlatform.OSXWebPlayer => PlatformType.Osx, 
			_ => PlatformType.Other, 
		};
	}

	public static void UpdateAndroidBack()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			m_androidBack = false;
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				m_androidBack = true;
			}
		}
	}

	public static bool IsAndroidBack()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		if (BtnClickOnEscape.IsAnyButtonsActive())
		{
			return false;
		}
		return m_androidBack;
	}

	public static bool AndroidBack()
	{
		if (m_androidBack)
		{
			m_androidBack = false;
			return true;
		}
		return false;
	}

	public static int GetMinPow2(int val)
	{
		if (val <= 1)
		{
			return 1;
		}
		if (val <= 2)
		{
			return 2;
		}
		if (val <= 4)
		{
			return 4;
		}
		if (val <= 8)
		{
			return 8;
		}
		if (val <= 16)
		{
			return 16;
		}
		if (val <= 32)
		{
			return 32;
		}
		if (val <= 64)
		{
			return 64;
		}
		if (val <= 128)
		{
			return 128;
		}
		if (val <= 256)
		{
			return 256;
		}
		if (val <= 512)
		{
			return 512;
		}
		if (val <= 1024)
		{
			return 1024;
		}
		if (val <= 2048)
		{
			return 2048;
		}
		if (val <= 4096)
		{
			return 4096;
		}
		return 1;
	}

	public static void NormalizeQuaternion(ref Quaternion q)
	{
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			num += q[i] * q[i];
		}
		float num2 = 1f / Mathf.Sqrt(num);
		for (int j = 0; j < 4; j++)
		{
			int index;
			int index2 = (index = j);
			float num3 = q[index];
			q[index2] = num3 * num2;
		}
	}

	public static Vector3 Project(Vector3 v, Vector3 onTo)
	{
		float num = Vector3.Dot(onTo, v);
		return onTo * num;
	}

	public static float DistanceXZ(Vector3 v0, Vector3 v1)
	{
		return new Vector2(v1.x - v0.x, v1.z - v0.z).magnitude;
	}

	public static Vector3 Bezier2(Vector3 Start, Vector3 Control, Vector3 End, float delta)
	{
		return (1f - delta) * (1f - delta) * Start + 2f * delta * (1f - delta) * Control + delta * delta * End;
	}

	public static float FixDegAngle(float p_Angle)
	{
		while (p_Angle >= 360f)
		{
			p_Angle -= 360f;
		}
		while (p_Angle < 0f)
		{
			p_Angle += 360f;
		}
		return p_Angle;
	}

	public static float DegDistance(float p_a, float p_b)
	{
		if (p_a == p_b)
		{
			return 0f;
		}
		p_a = FixDegAngle(p_a);
		p_b = FixDegAngle(p_b);
		float f = p_b - p_a;
		float num = Mathf.Abs(f);
		if (num > 180f)
		{
			num = Mathf.Abs(num - 360f);
		}
		return num;
	}

	public static float DegDirection(float p_a, float p_b)
	{
		if (p_a == p_b)
		{
			return 0f;
		}
		p_a = FixDegAngle(p_a);
		p_b = FixDegAngle(p_b);
		float num = p_a - p_b;
		float num2 = ((!(num > 0f)) ? (-1f) : 1f);
		if (Mathf.Abs(num) > 180f)
		{
			num2 *= -1f;
		}
		return num2;
	}

	public static Vector2 ScreenToGUIPos(Vector2 pos)
	{
		return new Vector2(pos.x, (float)Screen.height - pos.y);
	}

	public static void DisableLayer(Transform parent, int layerID)
	{
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.gameObject.layer == layerID)
			{
				transform.gameObject.active = false;
			}
		}
	}

	public static Transform FindTransform(Transform parent, string name)
	{
		if (parent.name == name)
		{
			return parent;
		}
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	public static Bounds FindHeirarcyBounds(Transform parent)
	{
		Bounds result = default(Bounds);
		if (parent.renderer != null)
		{
			result.Encapsulate(parent.renderer.bounds.min);
			result.Encapsulate(parent.renderer.bounds.max);
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Bounds bounds = FindHeirarcyBounds(parent.GetChild(i));
			result.Encapsulate(bounds.min);
			result.Encapsulate(bounds.max);
		}
		return result;
	}

	public static void LoadTrail(ref TrailRenderer trail, BinaryReader reader)
	{
		if (reader.ReadBoolean())
		{
			trail = new TrailRenderer();
		}
	}

	public static void SaveTrail(TrailRenderer trail, BinaryWriter writer)
	{
		if (trail != null)
		{
			writer.Write(value: false);
		}
		else
		{
			writer.Write(value: true);
		}
	}

	public static void SaveParticles(ParticleSystem ps, BinaryWriter stream)
	{
		PLog.Log("partzz " + ps.particleCount + " on " + ps.gameObject.name);
		ParticleSystem.Particle[] array = new ParticleSystem.Particle[ps.particleCount];
		ps.GetParticles(array);
		stream.Write(array.Length);
		ParticleSystem.Particle[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ParticleSystem.Particle particle = array2[i];
			stream.Write(particle.position.x);
			stream.Write(particle.position.y);
			stream.Write(particle.position.z);
			stream.Write(particle.velocity.x);
			stream.Write(particle.velocity.y);
			stream.Write(particle.velocity.z);
			stream.Write(particle.lifetime);
			stream.Write(particle.startLifetime);
			stream.Write(particle.size);
			stream.Write(particle.rotation);
			stream.Write(particle.angularVelocity);
			stream.Write(particle.color.r);
			stream.Write(particle.color.g);
			stream.Write(particle.color.b);
			stream.Write(particle.color.a);
			stream.Write(particle.randomValue);
		}
		stream.Write(ps.startDelay);
		stream.Write(ps.time);
		stream.Write(ps.playbackSpeed);
		stream.Write(ps.emissionRate);
		stream.Write(ps.startSpeed);
		stream.Write(ps.startSize);
		stream.Write(ps.startColor.r);
		stream.Write(ps.startColor.g);
		stream.Write(ps.startColor.b);
		stream.Write(ps.startColor.a);
		stream.Write(ps.startLifetime);
	}

	public static void LoadParticles(ParticleSystem ps, BinaryReader stream)
	{
		int num = stream.ReadInt32();
		ParticleSystem.Particle[] array = new ParticleSystem.Particle[num];
		Vector3 position = default(Vector3);
		Vector3 velocity = default(Vector3);
		Color32 color = default(Color32);
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.Particle particle = default(ParticleSystem.Particle);
			position.x = stream.ReadSingle();
			position.y = stream.ReadSingle();
			position.z = stream.ReadSingle();
			particle.position = position;
			velocity.x = stream.ReadSingle();
			velocity.y = stream.ReadSingle();
			velocity.z = stream.ReadSingle();
			array[i].velocity = velocity;
			particle.lifetime = stream.ReadSingle();
			particle.startLifetime = stream.ReadSingle();
			particle.lifetime = stream.ReadSingle();
			particle.rotation = stream.ReadSingle();
			particle.angularVelocity = stream.ReadSingle();
			color.r = stream.ReadByte();
			color.g = stream.ReadByte();
			color.b = stream.ReadByte();
			color.a = stream.ReadByte();
			particle.color = color;
			particle.randomValue = stream.ReadSingle();
			array[i] = particle;
		}
		ps.SetParticles(array, array.Length);
		ps.startDelay = stream.ReadSingle();
		ps.time = stream.ReadSingle();
		ps.playbackSpeed = stream.ReadSingle();
		ps.emissionRate = stream.ReadSingle();
		ps.startSpeed = stream.ReadSingle();
		ps.startSize = stream.ReadSingle();
		Color startColor = default(Color);
		startColor.r = stream.ReadSingle();
		startColor.g = stream.ReadSingle();
		startColor.b = stream.ReadSingle();
		startColor.a = stream.ReadSingle();
		ps.startColor = startColor;
		ps.startLifetime = stream.ReadSingle();
		PLog.Log("loaded particles " + ps.particleCount);
	}

	public static void GetDimensionsOfGameObject(GameObject go, out float width, out float height, out float depth)
	{
		width = (height = (depth = 0f));
		float num2;
		float num;
		float num3 = (num2 = (num = float.MaxValue));
		float num5;
		float num4;
		float num6 = (num5 = (num4 = float.MinValue));
		float x = go.transform.position.x;
		float y = go.transform.position.y;
		float z = go.transform.position.z;
		for (int i = 0; i < 2; i++)
		{
			Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				float num7 = x + renderer.transform.localPosition.x + renderer.bounds.min.x;
				float num8 = x + renderer.transform.localPosition.x + renderer.bounds.max.x;
				float num9 = y + renderer.transform.localPosition.y + renderer.bounds.min.y;
				float num10 = y + renderer.transform.localPosition.y + renderer.bounds.max.y;
				float num11 = z + renderer.transform.localPosition.z + renderer.bounds.min.z;
				float num12 = z + renderer.transform.localPosition.z + renderer.bounds.max.z;
				if (num7 < num3)
				{
					num3 = num7;
				}
				if (num8 > num6)
				{
					num6 = num8;
				}
				if (num9 < num2)
				{
					num2 = num9;
				}
				if (num10 > num5)
				{
					num5 = num10;
				}
				if (num11 < num)
				{
					num = num11;
				}
				if (num12 > num4)
				{
					num4 = num12;
				}
			}
		}
		width = num6 - num3;
		height = num5 - num2;
		depth = num4 - num;
	}

	public static bool TrySetTextureFromResources(GameObject onObject, string resourceUrl, float fallbackWidth, float fallbackHeight, bool scaleWithChildren, out string debugOutput)
	{
		debugOutput = string.Empty;
		SimpleSprite component = onObject.GetComponent<SimpleSprite>();
		if (component == null)
		{
			debugOutput = string.Format("GameObject {0} had no SimpleSprite-script on it.");
			return false;
		}
		Texture2D texture2D = Resources.Load(resourceUrl) as Texture2D;
		if (texture2D == null)
		{
			debugOutput = $"\"{resourceUrl}\" could not be found in Resources or it is no a Texture2D.";
			return false;
		}
		float x = texture2D.width;
		float y = texture2D.height;
		component.SetTexture(texture2D);
		bool flag = true;
		float num = fallbackWidth;
		float h = fallbackHeight;
		if (onObject.renderer != null)
		{
			num = onObject.renderer.bounds.size.x;
			h = onObject.renderer.bounds.size.y;
			flag = false;
		}
		if (scaleWithChildren)
		{
			bool flag2 = false;
			Renderer[] componentsInChildren = onObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (renderer != null)
				{
					num = Mathf.Max(num, renderer.bounds.size.x);
					h = Mathf.Max(num, renderer.bounds.size.y);
					flag2 = true;
				}
				flag = flag || flag2;
			}
		}
		if (flag)
		{
			if (debugOutput.Length > 0)
			{
				debugOutput += "\n";
			}
			debugOutput += "No renderer found in the GameObject";
			if (scaleWithChildren)
			{
				debugOutput += " or any of its children";
			}
			debugOutput += ".";
		}
		component.Setup(num, h, new Vector2(0f, y), new Vector2(x, y));
		component.autoResize = true;
		component.UpdateUVs();
		return true;
	}
}
