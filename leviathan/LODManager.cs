#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class LODManager : MonoBehaviour
{
	private GameCamera cameraPtr;

	private int currentIndex;

	private OnLODLevelChanged OnLODLevelChangedDelegate;

	private bool m_isEnabled = true;

	[SerializeField]
	public List<LODInstruction> m_instructions;

	public bool IsEnabled => m_isEnabled;

	public bool IsInLockedToHighdetail { get; private set; }

	public GameCamera CameraPtr
	{
		private get
		{
			return cameraPtr;
		}
		set
		{
			if (value == null && cameraPtr != null)
			{
				IsInLockedToHighdetail = true;
				return;
			}
			IsInLockedToHighdetail = false;
			if (value != cameraPtr)
			{
				cameraPtr = value;
			}
		}
	}

	private void Start()
	{
		InitializeCamera();
	}

	private void InitializeCamera()
	{
		if (cameraPtr == null)
		{
			cameraPtr = UnityEngine.Object.FindObjectOfType(typeof(GameCamera)) as GameCamera;
			if (cameraPtr == null)
			{
				IsInLockedToHighdetail = true;
				string message = $"LODManager for {base.gameObject.name} failed to find the GameCamera on Start(), will only use High-detail untill it is properly set !";
				Debug.LogWarning(message);
			}
			else
			{
				IsInLockedToHighdetail = false;
			}
		}
		else
		{
			IsInLockedToHighdetail = false;
		}
	}

	public void OnCameraMove(GameCamera camera)
	{
		if (!IsInLockedToHighdetail)
		{
			Think(forceUpdate: false);
		}
	}

	public GameObject ForceUpdateAndGetCurrent()
	{
		if (cameraPtr == null)
		{
			InitializeCamera();
		}
		if (!IsInLockedToHighdetail)
		{
			DisableAll();
			Think(forceUpdate: true);
		}
		return GetCurrentLODObject();
	}

	public GameObject UpdateFrom(int index)
	{
		if (!IsInLockedToHighdetail)
		{
			DisableAll();
			if (m_instructions != null && index > 0 && index < m_instructions.Count - 1)
			{
				currentIndex = index;
			}
			EnableCurrent();
		}
		return GetCurrentLODObject();
	}

	private void Think(bool forceUpdate)
	{
		if (m_instructions == null || m_instructions.Count <= 1)
		{
			currentIndex = 0;
		}
		else
		{
			if (CameraPtr == null)
			{
				return;
			}
			LODInstruction currentLODLevel = GetCurrentLODLevel();
			if (currentLODLevel == null || currentLODLevel.m_target == null)
			{
				return;
			}
			Vector3 vector = CameraPtr.transform.position - base.transform.position;
			vector.y *= 0.8f;
			float num = Mathf.Abs(vector.magnitude);
			if (num > currentLODLevel.m_maxDist)
			{
				if (m_isEnabled || forceUpdate)
				{
					EnableNext();
					return;
				}
				currentIndex++;
				if (currentIndex > m_instructions.Count - 1)
				{
					currentIndex = m_instructions.Count - 1;
				}
			}
			else if (num < currentLODLevel.m_minDist)
			{
				if (m_isEnabled || forceUpdate)
				{
					EnablePrev();
					return;
				}
				currentIndex--;
				if (currentIndex < 0)
				{
					currentIndex = 0;
				}
			}
			else
			{
				DisableAll();
				FindAndEnableCurrent(num);
			}
		}
	}

	public int GetCurrentLODIndex()
	{
		return currentIndex;
	}

	public LODInstruction GetCurrentLODLevel()
	{
		if (m_instructions == null || m_instructions.Count == 0)
		{
			return null;
		}
		if (IsInLockedToHighdetail)
		{
			currentIndex = 0;
		}
		return m_instructions[currentIndex];
	}

	public GameObject GetCurrentLODObject()
	{
		return GetCurrentLODLevel()?.m_target;
	}

	private void EnableNext()
	{
		int num = currentIndex + 1;
		if (num > m_instructions.Count - 1)
		{
			num = m_instructions.Count - 1;
		}
		if (num != currentIndex)
		{
			DisableCurrent();
			currentIndex = num;
			EnableCurrent();
		}
	}

	public void EnableCurrent()
	{
		if (!m_isEnabled)
		{
			return;
		}
		LODInstruction currentLODLevel = GetCurrentLODLevel();
		if (currentLODLevel.m_useAllRenderers)
		{
			if (currentLODLevel.m_target.renderer != null)
			{
				currentLODLevel.m_target.renderer.enabled = true;
			}
			Renderer[] componentsInChildren = currentLODLevel.m_target.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (!renderer.enabled)
				{
					renderer.enabled = true;
					if (currentIndex == 0)
					{
						renderer.castShadows = true;
						renderer.receiveShadows = true;
					}
				}
			}
		}
		else if (!currentLODLevel.m_target.renderer.enabled)
		{
			currentLODLevel.m_target.renderer.enabled = true;
		}
		if (OnLODLevelChangedDelegate != null)
		{
			OnLODLevelChangedDelegate(m_instructions[currentIndex].m_target);
		}
	}

	public void FindAndEnableCurrent(float distToCamera)
	{
		int num = 0;
		foreach (LODInstruction instruction in m_instructions)
		{
			if (instruction.m_minDist <= distToCamera && instruction.m_maxDist >= distToCamera)
			{
				break;
			}
			num++;
		}
		currentIndex = num;
		EnableCurrent();
	}

	private void EnablePrev()
	{
		int num = currentIndex - 1;
		if (num < 0)
		{
			num = 0;
		}
		if (num != currentIndex)
		{
			DisableCurrent();
			currentIndex = num;
			EnableCurrent();
		}
	}

	private void DisableCurrent()
	{
		LODInstruction currentLODLevel = GetCurrentLODLevel();
		if (currentLODLevel.m_useAllRenderers)
		{
			Renderer[] componentsInChildren = currentLODLevel.m_target.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (renderer.enabled)
				{
					renderer.enabled = false;
				}
			}
		}
		else if (currentLODLevel.m_target.renderer.enabled)
		{
			currentLODLevel.m_target.renderer.enabled = false;
		}
	}

	public bool Add(LODInstruction instr)
	{
		if (m_instructions == null)
		{
			m_instructions = new List<LODInstruction>();
		}
		if (!m_instructions.Contains(instr))
		{
			if (instr.m_useAllRenderers)
			{
				Renderer[] componentsInChildren = instr.m_target.GetComponentsInChildren<Renderer>();
				DebugUtils.Assert(componentsInChildren.Length > 0, "Cant add LOD target, it has no renderers !");
				if (componentsInChildren.Length == 0)
				{
					return false;
				}
				Renderer[] array = componentsInChildren;
				foreach (Renderer renderer in array)
				{
					renderer.enabled = false;
				}
			}
			else
			{
				DebugUtils.Assert(instr.m_target.renderer != null, "LODInstruction is set to NOT serach children for renderers, yet it has no renderer of it's own !");
				if (instr.m_target.renderer == null)
				{
					return false;
				}
				instr.m_target.renderer.enabled = false;
			}
			m_instructions.Add(instr);
			SortList();
			return true;
		}
		DebugUtils.Assert(condition: false, "Trying to add object to LODManager that already exists !");
		return false;
	}

	public void SortList()
	{
		if (m_instructions != null && m_instructions.Count > 1)
		{
			m_instructions.Sort((LODInstruction instr1, LODInstruction instr2) => instr1.CompareTo(instr2));
		}
	}

	public void AddOnLODChangedInterest(OnLODLevelChanged interest)
	{
		OnLODLevelChangedDelegate = (OnLODLevelChanged)Delegate.Remove(OnLODLevelChangedDelegate, interest);
		OnLODLevelChangedDelegate = (OnLODLevelChanged)Delegate.Combine(OnLODLevelChangedDelegate, interest);
	}

	public void RemoveLODChangedInterest(OnLODLevelChanged interest)
	{
		OnLODLevelChangedDelegate = (OnLODLevelChanged)Delegate.Remove(OnLODLevelChangedDelegate, interest);
	}

	private void Disable(LODInstruction instr)
	{
		if (instr.m_useAllRenderers)
		{
			Renderer[] componentsInChildren = instr.m_target.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.enabled = false;
			}
		}
		else
		{
			instr.m_target.renderer.enabled = false;
		}
	}

	private void DisableAll()
	{
		if (m_instructions != null && m_instructions.Count != 0)
		{
			m_instructions.ForEach(delegate(LODInstruction instr)
			{
				Disable(instr);
			});
		}
	}

	public void SetEnable(bool enabled)
	{
		if (m_isEnabled != enabled)
		{
			m_isEnabled = enabled;
			if (m_isEnabled)
			{
				EnableCurrent();
			}
			else
			{
				DisableAll();
			}
		}
	}
}
