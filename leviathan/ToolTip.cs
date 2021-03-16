using UnityEngine;

public class ToolTip : MonoBehaviour
{
	public string m_toolTip;

	private AutoSpriteControlBase button;

	private ToolTipDisplay _tooltip;

	private void Start()
	{
		button = GetComponent<AutoSpriteControlBase>();
		button.AddInputDelegate(OnInput);
		_tooltip = ToolTipDisplay.GetToolTip(base.gameObject);
	}

	private void OnDestroy()
	{
		if ((bool)_tooltip)
		{
			_tooltip.StopToolTip(base.gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (!_tooltip)
		{
			_tooltip = ToolTipDisplay.GetToolTip(base.gameObject);
		}
	}

	private void SetupToolTip()
	{
		string toolTip = m_toolTip;
		if (toolTip.Length == 0)
		{
			toolTip = base.gameObject.name + "_tooltip";
			toolTip = Localize.instance.TranslateKey(toolTip);
		}
		else
		{
			toolTip = Localize.instance.Translate(toolTip);
		}
		AutoSpriteControlBase component = GetComponent<AutoSpriteControlBase>();
		Vector3 position = base.transform.position;
		position.x += component.BottomRight.x;
		position.y += component.BottomRight.y;
		_tooltip.SetupToolTip(base.gameObject, toolTip, base.transform.position, position);
	}

	public void OnInput(ref POINTER_INFO ptr)
	{
		if (_tooltip == null)
		{
			return;
		}
		if (_tooltip.GetComponent<ToolTipDisplay>().GetHelpMode())
		{
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.PRESS)
			{
				ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
				return;
			}
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
			{
				SetupToolTip();
				_tooltip.NoDelay();
				ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
				return;
			}
		}
		if (true)
		{
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.MOVE)
			{
				SetupToolTip();
			}
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.MOVE_OFF || ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
			{
				_tooltip.StopToolTip(base.gameObject);
			}
		}
	}
}
