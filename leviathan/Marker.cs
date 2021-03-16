using System;
using System.IO;
using UnityEngine;

public class Marker : NetObj
{
	private bool m_show;

	public override void Awake()
	{
		base.Awake();
		base.renderer.enabled = false;
	}

	public void Update()
	{
		Camera camera = Camera.main;
		if (camera != null)
		{
			float num = Vector3.Distance(camera.transform.position, base.transform.position);
			float num2 = Mathf.Tan((float)Math.PI / 180f * camera.fieldOfView * 0.5f) * 0.04f * num;
			base.transform.localScale = new Vector3(num2, num2, num2);
			Transform transform = base.transform.FindChild("particle");
			if (transform != null)
			{
				transform.GetComponent<ParticleSystem>().startSize = Mathf.Tan((float)Math.PI / 180f * camera.fieldOfView * 0.5f) * num;
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
		writer.Write(base.transform.localScale.x);
		writer.Write(base.transform.localScale.y);
		writer.Write(base.transform.localScale.z);
		writer.Write(m_show);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		Vector3 position = default(Vector3);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		base.transform.position = position;
		Quaternion rotation = default(Quaternion);
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.rotation = rotation;
		Vector3 localScale = default(Vector3);
		localScale.x = reader.ReadSingle();
		localScale.y = reader.ReadSingle();
		localScale.z = reader.ReadSingle();
		base.transform.localScale = localScale;
		m_show = reader.ReadBoolean();
		SetVisibleState(m_show);
	}

	public void SetVisibleState(bool visible)
	{
		m_show = visible;
		base.renderer.enabled = visible;
		Transform transform = base.transform.FindChild("particle");
		if (!(transform == null))
		{
			if (visible)
			{
				transform.GetComponent<ParticleSystem>().Play();
			}
			else
			{
				transform.GetComponent<ParticleSystem>().Stop();
			}
		}
	}

	public override void SetVisible(bool visible)
	{
	}
}
