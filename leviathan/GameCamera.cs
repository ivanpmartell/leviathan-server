#define DEBUG
using System;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	public enum Mode
	{
		Active,
		Passive,
		Disabled
	}

	private enum TargetType
	{
		None,
		Land,
		Unit,
		HPModule
	}

	private const float m_grabDelay = 0.1f;

	private const float m_pinchTreshold = 25f;

	private const float m_moveTreshold = 20f;

	public Texture m_moveIcon;

	public Texture m_reverseIcon;

	public Texture m_facingIcon;

	public Texture m_resetFacingIcon;

	public Texture m_targetIcon;

	public Texture m_removeIcon;

	public Texture m_addIcon;

	public Texture m_deployIcon;

	public Texture m_returnFireIcon;

	public Texture m_fireAtWillIcon;

	public Texture m_holdFireIcon;

	public Texture m_supplyIcon;

	public Texture m_dontSupplyIcon;

	public Texture m_deployRadarIcon;

	public Texture m_deployCloakIcon;

	public Texture m_deployShieldLeftIcon;

	public Texture m_deployShieldRightIcon;

	public Texture m_deployShieldForwardIcon;

	public Texture m_deployShieldBackwardIcon;

	public GameObject m_effectPrefab;

	public float m_shakeScale = 0.25f;

	public float m_shakeMaxIntensity = 3f;

	public float m_keyboardScrollSpeed = 700f;

	public float m_keyboardZoomSpeed = 100f;

	private int m_localPlayerID = -1;

	private Mode m_mode = Mode.Disabled;

	private GameObject m_guiCamera;

	private GameObject m_selectedOrder;

	private bool m_settingMoveOrderFacing;

	private int m_selectedNetID = -1;

	private bool m_selectedNetObjExplicit;

	private GameObject m_hover;

	private TargetType m_hoverType;

	private Vector3 m_targetPos;

	private float m_levelSize = 500f;

	public float m_minZoom = 600f;

	public float m_maxZoom = 90f;

	public float m_clipPlaneDistance = 200f;

	private float m_mouseMoveSpeed = 0.001f;

	private float m_zBorderOffset = -0.3f;

	private bool m_pinchZoom;

	private float m_pinchStartDistance = -1f;

	private float m_totalScrollZoomDelta;

	private bool m_flowerMenuFireDrag;

	private bool m_draging;

	private Vector3 m_dragStart;

	private Vector3 m_dragStartMousePos;

	private Vector3 m_dragLastPos;

	private GameObject m_dragObject;

	private ContextMenu m_contextMenu;

	private FlowerMenu m_flowerMenu;

	private StatusWnd_HPModule m_statusWnd_HPModule;

	private StatusWnd_Ship m_statusWnd_Ship;

	private bool m_firstSetup = true;

	private bool m_hasFocusPos;

	private Vector2 m_focusPos = Vector3.zero;

	private float m_focusHeight;

	private float m_shakeTimer = -1f;

	private Quaternion m_shakeRot;

	private float m_shakeIntensity;

	private float m_shakeLength;

	private Vector3 m_shakeDirection;

	private int m_markersMask;

	private int m_pickMask;

	private int m_terrainMask;

	private bool m_allowSelection = true;

	private bool m_allowFlowerMenu = true;

	private void Start()
	{
		AudioListener component = base.gameObject.transform.GetChild(0).GetComponent<AudioListener>();
		component.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
		m_terrainMask = (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Default"));
		m_markersMask = 1 << LayerMask.NameToLayer("markers");
		m_pickMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("units")) | (1 << LayerMask.NameToLayer("hpmodules")) | (1 << LayerMask.NameToLayer("Water"));
		int num = ~(1 << LayerMask.NameToLayer("low_vis"));
		base.camera.cullingMask = base.camera.cullingMask & num;
		base.camera.depthTextureMode = DepthTextureMode.Depth;
		if (m_effectPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_effectPrefab, base.gameObject.transform.position, Quaternion.identity) as GameObject;
			gameObject.transform.parent = base.transform;
		}
		UpdateClipPlanes();
	}

	private void OnDestroy()
	{
		CloseStatusWindow_Ship();
		CloseStatusWindow_HPModule();
		CloseFlowerMenu();
	}

	public void SetMode(Mode mode)
	{
		m_mode = mode;
		CloseFlowerMenu();
		if (mode == Mode.Passive || mode == Mode.Disabled)
		{
			m_contextMenu = null;
		}
		if (mode == Mode.Active || mode == Mode.Passive)
		{
			RestoreNetObjSelection();
		}
	}

	public Mode GetMode()
	{
		return m_mode;
	}

	public void Setup(int localPlayerID, float levelSize, GameObject guiCamera)
	{
		m_guiCamera = guiCamera;
		m_localPlayerID = localPlayerID;
		m_levelSize = levelSize;
		float num = Mathf.Tan((float)Math.PI / 180f * base.camera.fieldOfView * 0.5f);
		if (m_firstSetup)
		{
			Zoom(-1000f, base.transform.forward, 1f);
			m_firstSetup = false;
		}
		if (m_localPlayerID >= 0)
		{
			RestoreNetObjSelection();
			SetHovered(null);
		}
	}

	private void OnGUI()
	{
		if (m_mode == Mode.Disabled)
		{
			return;
		}
		if (m_hover != null && Debug.isDebugBuild)
		{
			int num = 0;
			if (CheatMan.instance.DebugAi())
			{
				num = 300;
			}
			Unit component = m_hover.GetComponent<Unit>();
			if (component != null)
			{
				GUI.TextField(new Rect(Input.mousePosition.x + 10f, (float)Screen.height - Input.mousePosition.y - 25f, 160 + num, num + 80), component.GetTooltip());
			}
			HPModule component2 = m_hover.GetComponent<HPModule>();
			if (component2 != null)
			{
				GUI.TextField(new Rect(Input.mousePosition.x + 10f, (float)Screen.height - Input.mousePosition.y - 25f, 200 + num, num + 70), component2.GetTooltip());
			}
		}
		if (m_contextMenu != null)
		{
			m_contextMenu.DrawGui(base.camera);
		}
	}

	private void CursorRayCast(Ray ray, out GameObject markerObject, out GameObject hitObject, out Vector3 hitPoint)
	{
		hitObject = null;
		markerObject = null;
		hitPoint = Vector3.zero;
		if (Physics.Raycast(ray, out var hitInfo, 10000f, m_markersMask))
		{
			markerObject = hitInfo.collider.gameObject;
			if (markerObject.transform.parent != null && markerObject.transform.parent.GetComponent<OrderMarker>() != null)
			{
				markerObject = markerObject.transform.parent.gameObject;
			}
			hitPoint = hitInfo.point;
		}
		float num = 9999999f;
		RaycastHit[] array = Physics.RaycastAll(ray, 10000f, m_pickMask);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			GameObject gameObject = raycastHit.collider.gameObject;
			if (gameObject.GetComponent<HPModule>() == null)
			{
				Section component = gameObject.GetComponent<Section>();
				if (component != null)
				{
					gameObject = component.GetUnit().gameObject;
				}
				if (component == null && gameObject.GetComponent<Unit>() == null && gameObject.collider != null && (bool)gameObject.collider.attachedRigidbody)
				{
					gameObject = gameObject.collider.attachedRigidbody.gameObject;
				}
			}
			NetObj component2 = gameObject.GetComponent<NetObj>();
			if (!(component2 != null) || component2.IsVisible())
			{
				HPModule component3 = gameObject.GetComponent<HPModule>();
				if (component3 != null)
				{
					gameObject = component3.GetUnit().gameObject;
				}
				if (raycastHit.distance < num)
				{
					hitObject = gameObject;
					hitPoint = raycastHit.point;
					num = raycastHit.distance;
				}
			}
		}
	}

	public void Update()
	{
		if (m_contextMenu != null)
		{
			m_contextMenu.Update(Time.deltaTime);
		}
		if (m_flowerMenu != null)
		{
			m_flowerMenu.Update(Time.deltaTime);
		}
		UpdateShake(Time.deltaTime);
		if (m_hasFocusPos)
		{
			UpdateFocus(Time.deltaTime);
		}
		else
		{
			if (m_mode == Mode.Disabled)
			{
				return;
			}
			Ray ray = base.camera.ScreenPointToRay(Input.mousePosition);
			CursorRayCast(ray, out var markerObject, out var hitObject, out var hitPoint);
			if (hitObject == null)
			{
				SetHovered(null);
				return;
			}
			m_targetPos = hitPoint;
			SetHovered(hitObject);
			hitObject = ((!(markerObject != null)) ? hitObject : markerObject);
			UIManager component = m_guiCamera.GetComponent<UIManager>();
			bool flag = component.DidAnyPointerHitUI() || (m_contextMenu != null && m_contextMenu.IsMouseOver());
			bool flag2 = m_flowerMenu != null && m_flowerMenu.IsMouseOver();
			if (!flag)
			{
				if (Input.touchCount <= 1)
				{
					if (!m_draging)
					{
						if (Input.GetMouseButtonDown(0))
						{
							PrepareDraging(hitPoint, Input.mousePosition, hitObject);
						}
						if (Input.GetMouseButton(0))
						{
							float num = Vector3.Distance(m_dragStartMousePos, Input.mousePosition);
							if (num > 10f)
							{
								StartDraging(Input.mousePosition);
							}
						}
						if (Input.GetMouseButtonUp(0))
						{
							OnMouseReleased(hitPoint, hitObject);
						}
					}
					else
					{
						if (Input.GetMouseButton(0))
						{
							Vector3 mouseDelta = Input.mousePosition - m_dragLastPos;
							m_dragLastPos = Input.mousePosition;
							if (m_dragObject != null)
							{
								OnDragUpdate(m_dragStart, hitPoint, mouseDelta, m_dragObject);
							}
						}
						if (Input.GetMouseButtonUp(0) && m_draging)
						{
							m_draging = false;
							if (m_dragObject != null)
							{
								OnDragStoped(hitPoint, m_dragObject);
							}
						}
					}
				}
				else
				{
					m_draging = false;
				}
				UpdatePinchZoom(ray);
			}
			if (!flag || flag2)
			{
				m_totalScrollZoomDelta += Input.GetAxis("Mouse ScrollWheel");
				if (Mathf.Abs(m_totalScrollZoomDelta) > 0.01f)
				{
					float num2 = m_totalScrollZoomDelta * 0.2f;
					m_totalScrollZoomDelta -= num2;
					Zoom(num2 * 100f, ray.direction, Time.deltaTime);
				}
			}
			if (component.FocusObject == null)
			{
				UpdateWASDControls(Time.deltaTime);
			}
			NetObj selectedNetObj = GetSelectedNetObj();
			if (!(selectedNetObj != null))
			{
				return;
			}
			if (selectedNetObj.IsSeenByPlayer(m_localPlayerID))
			{
				if (m_statusWnd_Ship != null)
				{
					m_statusWnd_Ship.Update();
				}
				if (m_statusWnd_HPModule != null)
				{
					m_statusWnd_HPModule.Update();
				}
			}
			else
			{
				SetSelectedNetObj(null, explicitSelected: false);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateClipPlanes();
		if (m_flowerMenu != null)
		{
			m_flowerMenu.LateUpdate(Time.deltaTime);
		}
	}

	private void UpdateWASDControls(float dt)
	{
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			zero.y -= m_keyboardScrollSpeed * dt;
		}
		if (Input.GetKey(KeyCode.A))
		{
			zero.x += m_keyboardScrollSpeed * dt;
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero.y += m_keyboardScrollSpeed * dt;
		}
		if (Input.GetKey(KeyCode.D))
		{
			zero.x -= m_keyboardScrollSpeed * dt;
		}
		if (zero != Vector3.zero)
		{
			MoveCamera(zero);
		}
		float num = 0f;
		if (Input.GetKey(KeyCode.Q))
		{
			num -= m_keyboardZoomSpeed * dt;
		}
		if (Input.GetKey(KeyCode.E))
		{
			num += m_keyboardZoomSpeed * dt;
		}
		if (num != 0f)
		{
			Zoom(num, base.transform.forward, dt);
		}
	}

	private void UpdatePinchZoom(Ray ray)
	{
		if (Input.touchCount == 2 && !m_draging)
		{
			float pinchDistance = GetPinchDistance();
			if (!m_pinchZoom)
			{
				m_pinchZoom = true;
				m_pinchStartDistance = pinchDistance;
			}
			else
			{
				float zoomDelta = (0f - (m_pinchStartDistance - pinchDistance)) / 5f;
				m_pinchStartDistance = pinchDistance;
				Zoom(zoomDelta, ray.direction, Time.deltaTime);
			}
		}
		if (m_pinchZoom && Input.touchCount < 2)
		{
			m_pinchZoom = false;
		}
	}

	private void UpdateFocus(float dt)
	{
		Vector3 forward = base.transform.forward;
		Vector3 vector = new Vector3(m_focusPos.x, 0f, m_focusPos.y) - forward * m_focusHeight;
		Vector3 position = base.transform.position;
		Vector3 vector2 = vector - position;
		position += vector2 * dt * 4f;
		base.transform.position = position;
		if (vector2.magnitude < 10f)
		{
			m_hasFocusPos = false;
		}
	}

	private void UpdateShake(float dt)
	{
		if (!(m_shakeTimer < 0f))
		{
			m_shakeTimer += dt;
			if (m_shakeTimer >= m_shakeLength)
			{
				base.transform.rotation = m_shakeRot;
				m_shakeTimer = -1f;
				return;
			}
			float num = (1f - m_shakeTimer / m_shakeLength) * m_shakeIntensity * m_shakeScale;
			float num2 = 50f;
			float x = (Mathf.Sin(m_shakeTimer * num2) * num + Mathf.Cos(m_shakeTimer * num2 * 2f) * num * 0.2f) * m_shakeDirection.x;
			float y = (Mathf.Cos(m_shakeTimer * num2) * num + Mathf.Sin(m_shakeTimer * num2 * 2f) * num * 0.2f) * m_shakeDirection.y;
			base.transform.localRotation = m_shakeRot;
			base.transform.Rotate(new Vector3(x, y, 0f));
		}
	}

	public void AddShake(Vector3 origin, float intensity)
	{
		float num = Vector3.Distance(base.transform.position, origin);
		float intensity2 = intensity / (num / 100f);
		AddShake(intensity2);
	}

	private void AddShake(float intensity)
	{
		if (m_shakeTimer >= 0f)
		{
			float num = 1f - m_shakeTimer / m_shakeLength;
			float intensity2 = m_shakeIntensity * num + intensity;
			Shake(intensity2);
		}
		else
		{
			Shake(intensity);
		}
	}

	private void Shake(float intensity)
	{
		intensity = Mathf.Min(m_shakeMaxIntensity, intensity);
		if (m_shakeTimer >= 0f)
		{
			base.transform.localRotation = m_shakeRot;
		}
		else
		{
			m_shakeDirection = new Vector3(Mathf.Sign(UnityEngine.Random.value - 0.5f), Mathf.Sign(UnityEngine.Random.value - 0.5f), Mathf.Sign(UnityEngine.Random.value - 0.5f));
			m_shakeRot = base.transform.localRotation;
		}
		m_shakeTimer = 0f;
		m_shakeIntensity = intensity;
		m_shakeLength = intensity;
	}

	public void SetFocus(Vector3 pos, float height)
	{
		SetSelected(null);
		m_hasFocusPos = true;
		m_focusPos = new Vector2(pos.x, pos.z);
		m_focusHeight = height;
	}

	private void PrepareDraging(Vector3 hitPos, Vector3 mousePos, GameObject go)
	{
		m_dragStart = hitPos;
		m_dragStartMousePos = mousePos;
		m_dragObject = go;
	}

	private void StartDraging(Vector3 mousePos)
	{
		m_draging = true;
		m_flowerMenuFireDrag = false;
		m_dragLastPos = mousePos;
		if (m_dragObject != null)
		{
			OnDragStarted(m_dragStart, m_dragStartMousePos, m_dragObject);
		}
	}

	private void OnMouseReleased(Vector3 pos, GameObject go)
	{
		if (m_allowSelection)
		{
			SetSelected(go);
			OrderMarker component = go.GetComponent<OrderMarker>();
			if ((bool)component)
			{
				OpenOrderContextMenu(component);
			}
		}
	}

	private void OnDragStarted(Vector3 pos, Vector3 mousePos, GameObject go)
	{
		if (((1 << go.layer) & m_terrainMask) == 0)
		{
			SetSelected(go);
		}
	}

	private void OnDragUpdate(Vector3 startPos, Vector3 pos, Vector3 mouseDelta, GameObject go)
	{
		if (((1 << go.layer) & m_terrainMask) != 0)
		{
			MoveCamera(mouseDelta);
		}
		if (m_allowSelection && m_mode == Mode.Active)
		{
			OrderMarker component = go.GetComponent<OrderMarker>();
			if (component != null)
			{
				MoveOrder(component, pos);
			}
		}
	}

	private void MoveCamera(Vector3 moveDelta)
	{
		Vector3 vector = -moveDelta * m_mouseMoveSpeed * base.transform.position.y;
		Vector3 position = base.transform.position;
		position += new Vector3(vector.x, 0f, vector.y);
		position.x = Mathf.Clamp(position.x, (0f - m_levelSize) / 2f, m_levelSize / 2f);
		float num = m_zBorderOffset * position.y;
		position.z = Mathf.Clamp(position.z, (0f - m_levelSize) / 2f + num, m_levelSize / 2f + num);
		base.transform.position = position;
	}

	private void OnDragStoped(Vector3 pos, GameObject go)
	{
		if (!m_allowSelection)
		{
			return;
		}
		OrderMarker component = go.GetComponent<OrderMarker>();
		if (!component)
		{
			return;
		}
		if (m_flowerMenuFireDrag)
		{
			Gun gun = component.GetOrder().GetOwner() as Gun;
			if (gun != null)
			{
				Unit unit = gun.GetUnit();
				SetSelected(unit.gameObject);
			}
		}
		else
		{
			OpenOrderContextMenu(component);
		}
	}

	public FlowerMenu GetFlowerMenu()
	{
		return m_flowerMenu;
	}

	private TargetType GetTargetType(GameObject obj)
	{
		if (obj == null)
		{
			return TargetType.None;
		}
		Unit component = obj.GetComponent<Unit>();
		if (component != null)
		{
			return TargetType.Unit;
		}
		HPModule component2 = obj.GetComponent<HPModule>();
		if (component2 != null)
		{
			return TargetType.HPModule;
		}
		return TargetType.Land;
	}

	public void SetSelected(GameObject selected)
	{
		if (!m_allowSelection)
		{
			return;
		}
		if (selected == null)
		{
			SetSelectedOrder(null);
			SetSelectedNetObj(null, explicitSelected: false);
		}
		else if (selected.GetComponent<OrderMarker>() != null)
		{
			OrderMarker component = selected.GetComponent<OrderMarker>();
			MonoBehaviour monoBehaviour = component.GetOrder().GetOwner() as MonoBehaviour;
			if (monoBehaviour != null)
			{
				SetSelectedNetObj(monoBehaviour.gameObject, explicitSelected: false);
			}
			else
			{
				SetSelectedNetObj(null, explicitSelected: false);
			}
			SetSelectedOrder(selected);
		}
		else
		{
			SetSelectedOrder(null);
			SetSelectedNetObj(selected, explicitSelected: true);
		}
	}

	private bool SetSelectedOrder(GameObject selected)
	{
		if (m_selectedOrder != null)
		{
			OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
			if (component != null && m_selectedOrder != selected)
			{
				component.SetSelected(selected: false);
			}
		}
		CloseFlowerMenu();
		m_contextMenu = null;
		m_selectedOrder = null;
		m_settingMoveOrderFacing = false;
		if (selected != null)
		{
			OrderMarker component2 = selected.GetComponent<OrderMarker>();
			if (component2 != null)
			{
				component2.SetSelected(selected: true);
				m_selectedOrder = selected;
				return true;
			}
		}
		return false;
	}

	private void RestoreNetObjSelection()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		if (selectedNetObj != null)
		{
			SetSelectedNetObj(selectedNetObj.gameObject, m_selectedNetObjExplicit);
		}
		else
		{
			SetSelectedNetObj(null, m_selectedNetObjExplicit);
		}
	}

	private bool SetSelectedNetObj(GameObject selected, bool explicitSelected)
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		if (selectedNetObj != null)
		{
			Unit component = selectedNetObj.GetComponent<Unit>();
			if (component != null)
			{
				component.SetSelected(selected: false, explicitSelected);
			}
			HPModule component2 = selectedNetObj.GetComponent<HPModule>();
			if (component2 != null)
			{
				component2.SetSelected(selected: false, explicitSelected);
			}
		}
		m_contextMenu = null;
		m_selectedNetID = -1;
		CloseStatusWindow_HPModule();
		CloseStatusWindow_Ship();
		CloseFlowerMenu();
		if ((bool)selected)
		{
			m_selectedNetObjExplicit = explicitSelected;
			HPModule component3 = selected.GetComponent<HPModule>();
			if (component3 != null && component3.IsSeenByPlayer(m_localPlayerID))
			{
				m_selectedNetID = component3.GetNetID();
				component3.SetSelected(selected: true, explicitSelected);
				if (explicitSelected)
				{
					OpenModuleContextMenu(component3, updateStatusWindow: true);
				}
				return true;
			}
			Unit component4 = selected.GetComponent<Unit>();
			if (!component4 && (bool)selected.collider.attachedRigidbody)
			{
				component4 = selected.collider.attachedRigidbody.GetComponent<Unit>();
			}
			if (component4 != null && !component4.IsDead() && component4.IsSeenByPlayer(m_localPlayerID))
			{
				m_selectedNetID = component4.GetNetID();
				component4.SetSelected(selected: true, explicitSelected);
				if (explicitSelected)
				{
					OpenUnitContextMenu(component4);
				}
				return true;
			}
		}
		return false;
	}

	private void SetHovered(GameObject hover)
	{
		if (hover != null)
		{
			NetObj component = hover.GetComponent<NetObj>();
			if ((!(component != null) || component.IsVisible()) && !(m_hover == hover))
			{
				if ((bool)m_hover)
				{
					SetHighlight(m_hover, enabled: false);
					m_hover = null;
				}
				m_hover = hover;
				m_hoverType = GetTargetType(m_hover);
				SetHighlight(m_hover, enabled: true);
			}
		}
		else if ((bool)m_hover)
		{
			SetHighlight(m_hover, enabled: false);
			m_hover = null;
		}
	}

	private void SetupFlowerMenu(Ship ship)
	{
		if (m_flowerMenu != null && m_flowerMenu.GetShip() != ship)
		{
			CloseFlowerMenu();
		}
		if (m_flowerMenu == null)
		{
			bool canOrder = ship.GetOwner() == m_localPlayerID && m_mode == Mode.Active;
			bool localOwner = ship.GetOwner() == m_localPlayerID;
			m_flowerMenu = new FlowerMenu(base.camera, m_guiCamera, ship, canOrder, localOwner);
			m_flowerMenu.m_onModuleSelected = OnFlowerMenuSelect;
			m_flowerMenu.m_onModuleDragged = OnFlowerMenuDragged;
			m_flowerMenu.m_onMoveForward = OnFlowerMenuMoveForward;
			m_flowerMenu.m_onMoveReverse = OnFlowerMenuMoveReverse;
			m_flowerMenu.m_onMoveRotate = OnFlowerMenuMoveRotate;
			m_flowerMenu.m_onToggleSupply = OnFlowerMenuSupplyToggle;
		}
	}

	private void OnFlowerMenuSelect(HPModule module)
	{
		CloseFlowerMenu();
		SetSelected(module.gameObject);
	}

	private void OnFlowerMenuDragged(HPModule module)
	{
		if (m_mode == Mode.Active && module.GetOwner() == m_localPlayerID)
		{
			CloseFlowerMenu();
			SetSelected(module.gameObject);
			if (module is Gun)
			{
				OnSetGunTarget();
				m_flowerMenuFireDrag = true;
			}
		}
	}

	private void OnFlowerMenuMoveForward(Ship ship)
	{
		if (m_allowFlowerMenu && m_mode == Mode.Active && ship.GetOwner() == m_localPlayerID)
		{
			CloseFlowerMenu();
			OnMoveForward(ship);
		}
	}

	private void OnFlowerMenuMoveReverse(Ship ship)
	{
		if (m_allowFlowerMenu && m_mode == Mode.Active && ship.GetOwner() == m_localPlayerID)
		{
			CloseFlowerMenu();
			OnMoveBackward(ship);
		}
	}

	private void OnFlowerMenuMoveRotate(Ship ship)
	{
		if (m_allowFlowerMenu && m_mode == Mode.Active && ship.GetOwner() == m_localPlayerID)
		{
			CloseFlowerMenu();
			OnMoveRotate(ship);
		}
	}

	private void OnFlowerMenuSupplyToggle(Ship ship)
	{
		if (m_mode == Mode.Active && ship.GetOwner() == m_localPlayerID)
		{
			SupportShip supportShip = ship as SupportShip;
			if (!(supportShip == null))
			{
				supportShip.SetSupplyEnabled(!supportShip.IsSupplyEnabled());
			}
		}
	}

	private void CloseFlowerMenu()
	{
		if (m_flowerMenu != null)
		{
			m_flowerMenu.Close();
			m_flowerMenu = null;
		}
	}

	private void OpenUnitContextMenu(Unit unit)
	{
		bool flag = unit.GetOwner() == m_localPlayerID;
		Ship ship = unit as Ship;
		if (ship != null)
		{
			SetupFlowerMenu(ship);
		}
	}

	private void OpenModuleContextMenu(HPModule module, bool updateStatusWindow)
	{
		bool flag = module.GetOwner() == m_localPlayerID && m_mode == Mode.Active && !module.GetUnit().IsDoingMaintenance();
		float num = Vector3.Distance(base.transform.position, module.transform.position) * 0.006f;
		if (!flag)
		{
			return;
		}
		Radar radar = module as Radar;
		if (radar != null)
		{
			m_contextMenu = new ContextMenu(m_guiCamera.camera);
			if (!radar.GetDeploy())
			{
				m_contextMenu.AddClickButton(m_deployRadarIcon, Localize.instance.Translate("$radar_deploy"), module.transform.position, small: false, OnModuleDeploy);
			}
			else
			{
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$button_cancel"), module.transform.position, small: false, OnModuleAbortDeploy);
			}
		}
		Cloak cloak = module as Cloak;
		if (cloak != null)
		{
			m_contextMenu = new ContextMenu(m_guiCamera.camera);
			if (!cloak.GetDeploy())
			{
				m_contextMenu.AddClickButton(m_deployCloakIcon, Localize.instance.Translate("$cloak_deploy"), module.transform.position, small: false, OnModuleDeploy);
			}
			else
			{
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$button_cancel"), module.transform.position, small: false, OnModuleAbortDeploy);
			}
		}
		Shield shield = module as Shield;
		if (shield != null)
		{
			m_contextMenu = new ContextMenu(m_guiCamera.camera);
			Shield.DeployType deployShield = shield.GetDeployShield();
			Vector3 position = shield.transform.position;
			Vector3 vector = shield.GetUnit().transform.forward * num * 5f;
			Vector3 vector2 = shield.GetUnit().transform.right * num * 5f;
			if (deployShield == Shield.DeployType.Forward)
			{
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$button_cancel"), position + vector, small: false, OnShieldAbortDeploy);
			}
			else
			{
				m_contextMenu.AddClickButton(m_deployShieldForwardIcon, Localize.instance.Translate("$shield_dep_front"), position + vector, small: false, OnShieldDeployForward);
			}
			if (deployShield == Shield.DeployType.Backward)
			{
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$button_cancel"), position - vector, small: false, OnShieldAbortDeploy);
			}
			else
			{
				m_contextMenu.AddClickButton(m_deployShieldBackwardIcon, Localize.instance.Translate("$shield_dep_back"), position - vector, small: false, OnShieldDeployBackward);
			}
			if (deployShield == Shield.DeployType.Left)
			{
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$button_cancel"), position - vector2, small: false, OnShieldAbortDeploy);
			}
			else
			{
				m_contextMenu.AddClickButton(m_deployShieldLeftIcon, Localize.instance.Translate("$shield_dep_left"), position - vector2, small: false, OnShieldDeployLeft);
			}
			if (deployShield == Shield.DeployType.Right)
			{
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$button_cancel"), position + vector2, small: false, OnShieldAbortDeploy);
			}
			else
			{
				m_contextMenu.AddClickButton(m_deployShieldRightIcon, Localize.instance.Translate("$shield_dep_right"), position + vector2, small: false, OnShieldDeployRight);
			}
		}
		Gun gun = module as Gun;
		if (!(gun != null))
		{
			return;
		}
		m_contextMenu = new ContextMenu(m_guiCamera.camera);
		if (!gun.m_canDeploy)
		{
			if (gun.m_aim.m_manualTarget)
			{
				m_contextMenu.AddDragButton(m_targetIcon, Localize.instance.Translate("$gun_target"), gun.transform.TransformPoint(new Vector3(0f, 0f, 4f) * num), OnSetGunTarget);
			}
			switch (gun.GetStance())
			{
			case Gun.Stance.HoldFire:
				m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$gun_holdfire"), gun.transform.TransformPoint(new Vector3(4f, 0f, 0f) * num), small: false, OnToggleGunStance);
				break;
			case Gun.Stance.FireAtWill:
				m_contextMenu.AddClickButton(m_fireAtWillIcon, Localize.instance.Translate("$gun_fireatwill"), gun.transform.TransformPoint(new Vector3(4f, 0f, 0f) * num), small: false, OnToggleGunStance);
				break;
			}
		}
		else if (!gun.GetDeploy())
		{
			m_contextMenu.AddClickButton(m_deployIcon, Localize.instance.Translate("$gun_deploy"), gun.transform.TransformPoint(new Vector3(0f, 0f, 5f) * num), small: false, OnModuleDeploy);
		}
		else
		{
			m_contextMenu.AddClickButton(m_holdFireIcon, Localize.instance.Translate("$gun_holdfire"), gun.transform.TransformPoint(new Vector3(0f, 0f, 5f) * num), small: false, OnModuleAbortDeploy);
		}
	}

	private void OpenOrderContextMenu(OrderMarker marker)
	{
		if (m_mode != 0)
		{
			return;
		}
		Order order = marker.GetOrder();
		float num = Vector3.Distance(base.transform.position, marker.transform.position) * 0.006f;
		float num2 = 5f;
		m_contextMenu = new ContextMenu(m_guiCamera.camera);
		m_contextMenu.AddClickButton(m_removeIcon, Localize.instance.Translate("$context_remove"), marker.transform.position + new Vector3(0f - num2, 0f, num2) * num, small: false, OnRemoveOrder);
		if (order.m_type == Order.Type.MoveForward || order.m_type == Order.Type.MoveBackward)
		{
			IOrderable owner = order.GetOwner();
			if (owner.IsLastOrder(order))
			{
				m_contextMenu.AddDragButton(m_addIcon, Localize.instance.Translate("$context_add"), marker.transform.position + new Vector3(num2, 0f, num2) * num, OnAddMoveOrder);
			}
			m_contextMenu.AddDragButton(m_facingIcon, Localize.instance.Translate("$waypoint_rotate"), marker.transform.position + new Vector3(0f - num2, 0f, 0f - num2) * num, OnSetMoveOrderFaceing);
			m_contextMenu.AddClickButton(m_resetFacingIcon, Localize.instance.Translate("$waypoint_resetrot"), marker.transform.position + new Vector3(num2, 0f, 0f - num2) * num, small: false, OnResetMoveOrderFaceing);
		}
		if (order.m_type == Order.Type.Fire)
		{
			m_contextMenu.AddDragButton(m_addIcon, Localize.instance.Translate("$context_add"), marker.transform.position + new Vector3(num2, 0f, num2) * num, OnAddFireOrder);
		}
	}

	private void OnShieldDeployForward()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		Shield component = selectedNetObj.GetComponent<Shield>();
		if (!(component == null))
		{
			component.SetDeployShield(Shield.DeployType.Forward);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnShieldDeployBackward()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		Shield component = selectedNetObj.GetComponent<Shield>();
		if (!(component == null))
		{
			component.SetDeployShield(Shield.DeployType.Backward);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnShieldDeployLeft()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		Shield component = selectedNetObj.GetComponent<Shield>();
		if (!(component == null))
		{
			component.SetDeployShield(Shield.DeployType.Left);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnShieldDeployRight()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		Shield component = selectedNetObj.GetComponent<Shield>();
		if (!(component == null))
		{
			component.SetDeployShield(Shield.DeployType.Right);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnShieldAbortDeploy()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		Shield component = selectedNetObj.GetComponent<Shield>();
		if (!(component == null))
		{
			component.SetDeployShield(Shield.DeployType.None);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnModuleDeploy()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		HPModule component = selectedNetObj.GetComponent<HPModule>();
		if (!(component == null))
		{
			component.SetDeploy(deploy: true);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnModuleAbortDeploy()
	{
		NetObj selectedNetObj = GetSelectedNetObj();
		HPModule component = selectedNetObj.GetComponent<HPModule>();
		if (!(component == null))
		{
			component.SetDeploy(deploy: false);
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnSetGunTarget()
	{
		m_contextMenu = null;
		NetObj selectedNetObj = GetSelectedNetObj();
		Gun component = selectedNetObj.GetComponent<Gun>();
		if (!(component == null) && component.m_aim.m_manualTarget)
		{
			Order order = new Order(component, Order.Type.Fire, m_targetPos);
			order.SetDisplayRadius(component.GetTargetRadius(m_targetPos));
			order.SetStaticTargetOnly(component.GetStaticTargetOnly());
			order.m_fireVisual = component.GetOrderMarkerType();
			component.ClearOrders();
			component.AddOrder(order);
			PrepareDraging(m_targetPos, Input.mousePosition, order.GetMarker());
			StartDraging(Input.mousePosition);
		}
	}

	private void OnToggleGunStance()
	{
		PLog.Log("toggle stance");
		NetObj selectedNetObj = GetSelectedNetObj();
		Gun component = selectedNetObj.GetComponent<Gun>();
		if (!(component == null))
		{
			switch (component.GetStance())
			{
			case Gun.Stance.FireAtWill:
				component.SetStance(Gun.Stance.HoldFire);
				break;
			case Gun.Stance.HoldFire:
				component.SetStance(Gun.Stance.FireAtWill);
				break;
			}
			OpenModuleContextMenu(component, updateStatusWindow: false);
		}
	}

	private void OnMoveForward(Unit unitScript)
	{
		Order order = new Order(unitScript, Order.Type.MoveForward, m_targetPos);
		unitScript.ClearMoveOrders();
		unitScript.AddOrder(order);
		PrepareDraging(m_targetPos, Input.mousePosition, order.GetMarker());
		StartDraging(Input.mousePosition);
	}

	private void OnMoveBackward(Unit unitScript)
	{
		Order order = new Order(unitScript, Order.Type.MoveBackward, m_targetPos);
		unitScript.ClearMoveOrders();
		unitScript.AddOrder(order);
		PrepareDraging(m_targetPos, Input.mousePosition, order.GetMarker());
		StartDraging(Input.mousePosition);
	}

	private void OnMoveRotate(Unit unitScript)
	{
		unitScript.ClearMoveOrders();
		Order order = new Order(unitScript, Order.Type.MoveRotate, unitScript.transform.position);
		order.SetFacing(unitScript.transform.forward);
		unitScript.AddOrder(order);
		PrepareDraging(m_targetPos, Input.mousePosition, order.GetMarker());
		StartDraging(Input.mousePosition);
		m_settingMoveOrderFacing = true;
	}

	private void OnRemoveOrder()
	{
		m_contextMenu = null;
		OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
		if (!(component == null))
		{
			Order order = component.GetOrder();
			IOrderable owner = order.GetOwner();
			Unit unit = owner as Unit;
			HPModule hPModule = owner as HPModule;
			if (unit != null)
			{
				unit.RemoveOrder(order);
			}
			if (hPModule != null)
			{
				hPModule.RemoveOrder(order);
			}
			SetSelectedOrder(null);
		}
	}

	private void OnSetMoveOrderFaceing()
	{
		m_contextMenu = null;
		if (!(m_selectedOrder == null))
		{
			OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
			if (!(component == null))
			{
				PrepareDraging(m_targetPos, Input.mousePosition, m_selectedOrder);
				StartDraging(Input.mousePosition);
				m_settingMoveOrderFacing = true;
			}
		}
	}

	private void OnResetMoveOrderFaceing()
	{
		m_contextMenu = null;
		if (!(m_selectedOrder == null))
		{
			OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
			if (!(component == null))
			{
				component.GetOrder().ResetFacing();
			}
		}
	}

	private void OnAddMoveOrder()
	{
		m_contextMenu = null;
		if (!(m_selectedOrder == null))
		{
			OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
			if (!(component == null))
			{
				Order order = component.GetOrder();
				IOrderable owner = order.GetOwner();
				Order order2 = new Order(owner, order.m_type, m_targetPos);
				owner.AddOrder(order2);
				PrepareDraging(m_targetPos, Input.mousePosition, order2.GetMarker());
				StartDraging(Input.mousePosition);
			}
		}
	}

	private void OnAddFireOrder()
	{
		m_contextMenu = null;
		OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
		if (!(component == null))
		{
			Order order = component.GetOrder();
			IOrderable owner = order.GetOwner();
			Order order2 = new Order(owner, order.m_type, m_targetPos);
			order2.m_fireVisual = order.m_fireVisual;
			order2.SetStaticTargetOnly(order.GetStaticTargetOnly());
			order2.SetDisplayRadius(order.GetDisplayRadius());
			owner.AddOrder(order2);
			PrepareDraging(m_targetPos, Input.mousePosition, order2.GetMarker());
			StartDraging(Input.mousePosition);
		}
	}

	private void CloseStatusWindow_Ship()
	{
		if (m_statusWnd_Ship != null)
		{
			m_statusWnd_Ship.Close();
			m_statusWnd_Ship = null;
		}
	}

	public void ShowStatusWindow_Ship(Ship ship, bool friendly)
	{
		CloseStatusWindow_Ship();
		m_statusWnd_Ship = new StatusWnd_Ship(ship, m_guiCamera, friendly);
	}

	private void CloseStatusWindow_HPModule()
	{
		if (m_statusWnd_HPModule != null)
		{
			m_statusWnd_HPModule.Close();
			m_statusWnd_HPModule = null;
		}
	}

	public void ShowStatusWindow_HPModule(HPModule module)
	{
		CloseStatusWindow_HPModule();
		m_statusWnd_HPModule = module.CreateStatusWindow(m_guiCamera);
		Ship ship = module.GetUnit() as Ship;
		if (ship != null)
		{
			bool friendly = ship.GetOwner() == m_localPlayerID;
			ShowStatusWindow_Ship(ship, friendly);
		}
	}

	private void MoveOrder(OrderMarker markerScript, Vector3 newPos)
	{
		Order order = markerScript.GetOrder();
		IOrderable owner = order.GetOwner();
		Unit unit = owner as Unit;
		if (order.m_type == Order.Type.MoveForward || order.m_type == Order.Type.MoveBackward || order.m_type == Order.Type.MoveRotate)
		{
			if (m_settingMoveOrderFacing)
			{
				Vector3 facing = newPos - order.GetPos();
				order.SetFacing(facing);
			}
			else if (order.m_type != Order.Type.MoveRotate)
			{
				order.SetTarget(newPos);
				DebugUtils.Assert(unit != null);
				unit.SetBlockedRoute(!unit.IsMoveOrdersValid());
			}
		}
		else if (order.m_type == Order.Type.Fire)
		{
			bool key = Input.GetKey(KeyCode.LeftControl);
			if (m_hoverType == TargetType.Land || key || m_hover == null || order.GetStaticTargetOnly())
			{
				order.SetTarget(newPos);
			}
			else if (m_hoverType == TargetType.Unit || m_hoverType == TargetType.HPModule)
			{
				NetObj component = m_hover.GetComponent<NetObj>();
				Gun gun = owner as Gun;
				Vector3 localPos = m_hover.transform.InverseTransformPoint(newPos);
				order.SetTarget(component.GetNetID(), localPos);
			}
		}
	}

	private void Zoom(float zoomDelta, Vector3 zoomInDirection, float dt)
	{
		if (zoomDelta == 0f)
		{
			return;
		}
		if (zoomDelta > 0f)
		{
			base.transform.position += zoomInDirection * zoomDelta * dt * base.transform.position.y;
			if (base.transform.position.y < m_maxZoom)
			{
				float num = m_maxZoom - base.transform.position.y;
				float num2 = Vector3.Dot(Vector3.up, -zoomInDirection);
				Vector3 vector = -zoomInDirection * (num / num2);
				base.transform.position += vector;
				return;
			}
		}
		else if (zoomDelta < 0f)
		{
			base.transform.position += zoomInDirection * zoomDelta * dt * base.transform.position.y;
			if (base.transform.position.y > m_minZoom)
			{
				float num3 = m_minZoom - base.transform.position.y;
				float num4 = Vector3.Dot(Vector3.up, zoomInDirection);
				Vector3 vector2 = zoomInDirection * (num3 / num4);
				base.transform.position += vector2;
				return;
			}
		}
		NetObj selectedNetObj = GetSelectedNetObj();
		if (selectedNetObj != null)
		{
			HPModule hPModule = selectedNetObj as HPModule;
			if (hPModule != null)
			{
				OpenModuleContextMenu(hPModule, updateStatusWindow: false);
			}
		}
		if (m_selectedOrder != null)
		{
			OrderMarker component = m_selectedOrder.GetComponent<OrderMarker>();
			OpenOrderContextMenu(component);
		}
	}

	private void UpdateClipPlanes()
	{
		float y = base.transform.position.y;
		float nearClipPlane = Mathf.Max(10f, y - m_clipPlaneDistance);
		base.camera.nearClipPlane = nearClipPlane;
		float farClipPlane = Mathf.Min(950f, y + m_clipPlaneDistance);
		base.camera.farClipPlane = farClipPlane;
	}

	private float GetPinchDistance()
	{
		if (Input.touchCount == 2)
		{
			return Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
		}
		return 0f;
	}

	private void SetHighlight(GameObject obj, bool enabled)
	{
		if ((bool)obj)
		{
			HPModule component = obj.GetComponent<HPModule>();
			if (component != null)
			{
				component.SetHighlight(enabled);
			}
		}
	}

	public void SetEnabled(bool enabled)
	{
		base.camera.enabled = enabled;
		AudioManager.instance.SetSFXEnabled(enabled);
		if (!enabled)
		{
			CloseStatusWindow_HPModule();
			CloseStatusWindow_Ship();
		}
	}

	private NetObj GetSelectedNetObj()
	{
		if (m_selectedNetID >= 0)
		{
			return NetObj.GetByID(m_selectedNetID);
		}
		return null;
	}

	public void SetAllowSelection(bool allow)
	{
		m_allowSelection = allow;
	}

	public void SetAllowFlowerMenu(bool allow)
	{
		m_allowFlowerMenu = allow;
	}
}
