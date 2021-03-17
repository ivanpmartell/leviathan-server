namespace UnityEngine
{
	internal class SendMouseEvents
	{
		private struct HitInfo
		{
			public GameObject target;

			public Camera camera;

			public void SendMessage(string name)
			{
				target.SendMessage(name, null, SendMessageOptions.DontRequireReceiver);
			}

			public static bool Compare(HitInfo lhs, HitInfo rhs)
			{
				return lhs.target == rhs.target && lhs.camera == rhs.camera;
			}

			public static implicit operator bool(HitInfo exists)
			{
				return exists.target != null && exists.camera != null;
			}
		}

		private static HitInfo[] m_LastHit = new HitInfo[2]
		{
			default(HitInfo),
			default(HitInfo)
		};

		private static HitInfo[] m_MouseDownHit = new HitInfo[2]
		{
			default(HitInfo),
			default(HitInfo)
		};

		[NotRenamed]
		private static void DoSendMouseEvents()
		{
			HitInfo[] array = new HitInfo[2]
			{
				default(HitInfo),
				default(HitInfo)
			};
			Vector3 mousePosition = Input.mousePosition;
			Camera[] allCameras = Camera.allCameras;
			if (!GUIUtility.mouseUsed)
			{
				Camera[] array2 = allCameras;
				foreach (Camera camera in array2)
				{
					if (!camera.pixelRect.Contains(mousePosition))
					{
						continue;
					}
					GUILayer gUILayer = (GUILayer)camera.GetComponent(typeof(GUILayer));
					if ((bool)gUILayer)
					{
						GUIElement gUIElement = gUILayer.HitTest(mousePosition);
						if ((bool)gUIElement)
						{
							array[0].target = gUIElement.gameObject;
							array[0].camera = camera;
						}
					}
					if (camera.farClipPlane > 0f && Physics.Raycast(camera.ScreenPointToRay(mousePosition), out var hitInfo, camera.farClipPlane, camera.cullingMask & -5))
					{
						if ((bool)hitInfo.rigidbody)
						{
							array[1].target = hitInfo.rigidbody.gameObject;
							array[1].camera = camera;
						}
						else
						{
							array[1].target = hitInfo.collider.gameObject;
							array[1].camera = camera;
						}
					}
					else if (camera.clearFlags == CameraClearFlags.Skybox || camera.clearFlags == CameraClearFlags.Color)
					{
						array[1].target = null;
						array[1].camera = null;
					}
				}
			}
			for (int j = 0; j < 2; j++)
			{
				bool mouseButtonDown = Input.GetMouseButtonDown(0);
				bool mouseButton = Input.GetMouseButton(0);
				if (mouseButtonDown)
				{
					if ((bool)array[j])
					{
						ref HitInfo reference = ref m_MouseDownHit[j];
						reference = array[j];
						m_MouseDownHit[j].SendMessage("OnMouseDown");
					}
				}
				else if (!mouseButton)
				{
					if ((bool)m_MouseDownHit[j])
					{
						if (HitInfo.Compare(array[j], m_MouseDownHit[j]))
						{
							m_MouseDownHit[j].SendMessage("OnMouseUpAsButton");
						}
						m_MouseDownHit[j].SendMessage("OnMouseUp");
						m_MouseDownHit[j] = default(HitInfo);
					}
				}
				else if ((bool)m_MouseDownHit[j])
				{
					m_MouseDownHit[j].SendMessage("OnMouseDrag");
				}
				if (HitInfo.Compare(array[j], m_LastHit[j]))
				{
					if ((bool)array[j])
					{
						array[j].SendMessage("OnMouseOver");
					}
				}
				else
				{
					if ((bool)m_LastHit[j])
					{
						m_LastHit[j].SendMessage("OnMouseExit");
					}
					if ((bool)array[j])
					{
						array[j].SendMessage("OnMouseEnter");
						array[j].SendMessage("OnMouseOver");
					}
				}
				ref HitInfo reference2 = ref m_LastHit[j];
				reference2 = array[j];
			}
		}
	}
}
