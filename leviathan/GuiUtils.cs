#define DEBUG
using UnityEngine;

internal class GuiUtils
{
	public static GameObject CreateGui(string name, GameObject guiCamera)
	{
		GameObject gameObject = Resources.Load("gui/" + name) as GameObject;
		if (gameObject == null)
		{
			PLog.LogWarning(string.Format("GuiUtils::CreateGui( {0}, {1} ) Failed !", name, (!(guiCamera == null)) ? "guiCamera" : "NULL"));
			return null;
		}
		GameObject gameObject2 = Object.Instantiate(gameObject) as GameObject;
		gameObject2.transform.parent = guiCamera.transform;
		EZScreenPlacement[] componentsInChildren = gameObject2.GetComponentsInChildren<EZScreenPlacement>(includeInactive: true);
		EZScreenPlacement[] array = componentsInChildren;
		foreach (EZScreenPlacement eZScreenPlacement in array)
		{
			eZScreenPlacement.SetCamera(guiCamera.camera);
		}
		LocalizeGui(gameObject2);
		return gameObject2;
	}

	public static void FixedItemContainerInstance(UIListItemContainer item)
	{
		AutoSpriteBase[] componentsInChildren = item.transform.GetComponentsInChildren<AutoSpriteBase>(includeInactive: true);
		AutoSpriteBase[] array = componentsInChildren;
		foreach (AutoSpriteBase autoSpriteBase in array)
		{
			autoSpriteBase.isClone = true;
		}
	}

	public static Vector3 GetDimensionsAndDestroy(string name, GameObject guiCamera)
	{
		GameObject gameObject = CreateGui(name, guiCamera);
		Utils.GetDimensionsOfGameObject(gameObject, out var width, out var height, out var depth);
		Object.DestroyImmediate(gameObject);
		return new Vector3(width, height, depth);
	}

	public static void LocalizeGui(GameObject root)
	{
		Localize instance = Localize.instance;
		Component[] componentsInChildren = root.GetComponentsInChildren<Component>(includeInactive: true);
		Component[] array = componentsInChildren;
		foreach (Component component in array)
		{
			SpriteText spriteText = component as SpriteText;
			if (spriteText != null)
			{
				spriteText.Text = instance.Translate(spriteText.Text);
			}
			UIButton uIButton = component as UIButton;
			if (uIButton != null)
			{
				uIButton.Text = instance.Translate(uIButton.Text);
			}
		}
	}

	public static T FindChildOfComponent<T>(GameObject go, string name) where T : Component
	{
		GameObject gameObject = FindChildOf(go.transform, name);
		if (gameObject != null)
		{
			return gameObject.GetComponent<T>();
		}
		return (T)null;
	}

	public static GameObject FindChildOf(GameObject go, string name)
	{
		return FindChildOf(go.transform, name);
	}

	public static GameObject FindChildOf(Transform t, string name)
	{
		if (t.gameObject.name == name)
		{
			return t.gameObject;
		}
		for (int i = 0; i < t.childCount; i++)
		{
			GameObject gameObject = FindChildOf(t.GetChild(i), name);
			if ((bool)gameObject)
			{
				return gameObject;
			}
		}
		return null;
	}

	public static GameObject FindParent(Transform t, string name)
	{
		Transform parent = t.parent;
		while ((bool)parent)
		{
			if (parent.gameObject.name == name)
			{
				return parent.gameObject;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static GameObject ValidateGetObject(GameObject gui, string name)
	{
		GameObject gameObject = null;
		Transform transform = gui.transform.FindChild(name);
		if (transform == null)
		{
			return null;
		}
		return transform.gameObject;
	}

	public static bool ValidateGuiButton(GameObject gui, string name, out GameObject go)
	{
		go = ValidateGetObject(gui, name);
		if (go == null)
		{
			PLog.Log("Failed to find GuiButton: " + name);
			return false;
		}
		return go.GetComponent<UIButton>() != null;
	}

	public static bool ValidateGuiList(GameObject gui, string name, out GameObject go)
	{
		go = ValidateGetObject(gui, name);
		if (go == null)
		{
			PLog.Log("Failed to find GuiList: " + name);
			return false;
		}
		return go.GetComponent<UIScrollList>() != null;
	}

	public static bool ValidateGuLabel(GameObject gui, string name, out GameObject go)
	{
		go = ValidateGetObject(gui, name);
		if (go == null)
		{
			PLog.Log("Failed to find GuLabel: " + name);
			return false;
		}
		return go.GetComponent<SpriteText>() != null;
	}

	public static bool ValidateSimpelSprite(GameObject gui, string name, out GameObject go)
	{
		go = ValidateGetObject(gui, name);
		if (go == null)
		{
			PLog.Log("Failed to find SimpleSprite: " + name);
			return false;
		}
		return go.GetComponent<SimpleSprite>() != null;
	}

	public static Vector3 WorldToGuiPos(Camera gameCamera, Camera guiCamera, Vector3 pos)
	{
		Vector3 result = gameCamera.WorldToViewportPoint(pos);
		result.x = (result.x - 0.5f) * 2f;
		result.y = (result.y - 0.5f) * 2f;
		result.x *= guiCamera.orthographicSize * guiCamera.aspect;
		result.y *= guiCamera.orthographicSize;
		result.z = 0f;
		result.x = (int)result.x;
		result.y = (int)result.y;
		result.x += 0.01f;
		result.y += 0.01f;
		return result;
	}

	public static Vector3 WorldToGuiPosf(Camera gameCamera, Camera guiCamera, Vector3 pos)
	{
		Vector3 result = gameCamera.WorldToViewportPoint(pos);
		result.x = (result.x - 0.5f) * 2f;
		result.y = (result.y - 0.5f) * 2f;
		result.x *= guiCamera.orthographicSize * guiCamera.aspect;
		result.y *= guiCamera.orthographicSize;
		result.z = 0f;
		return result;
	}

	public static void SetAnimationSetProgress(GameObject animationRoot, float i)
	{
		PackedSprite[] componentsInChildren = animationRoot.GetComponentsInChildren<PackedSprite>(includeInactive: true);
		int num = 0;
		PackedSprite[] array = componentsInChildren;
		foreach (PackedSprite packedSprite in array)
		{
			num += packedSprite.animations[0].GetFrameCount();
		}
		int num2 = (int)((float)num * i);
		int num3 = 0;
		PackedSprite[] array2 = componentsInChildren;
		foreach (PackedSprite packedSprite2 in array2)
		{
			int frameCount = packedSprite2.animations[0].GetFrameCount();
			if (num2 >= num3 && num2 < num3 + frameCount)
			{
				packedSprite2.Hide(tf: false);
				packedSprite2.SetFrame(0, num2 - num3);
			}
			else
			{
				packedSprite2.Hide(tf: true);
			}
			num3 += frameCount;
		}
	}

	public static Texture2D GetFlagTexture(int id)
	{
		string text = "Flags/AvatarFlag_" + id;
		Texture2D texture2D = Resources.Load(text) as Texture2D;
		if (texture2D == null)
		{
			string o = $"HUD_Player failed to load flag \"{text}\"";
			PLog.LogError(o);
		}
		return texture2D;
	}

	public static Texture2D GetAchievementIconTexture(int id, bool unlocked)
	{
		string text = id.ToString();
		if (text.Length < 2)
		{
			text = "0" + id;
		}
		text = "Achievements/Achievement_Icon_" + text;
		if (!unlocked)
		{
			text += "_Locked";
		}
		Texture2D texture2D = Resources.Load(text) as Texture2D;
		if (texture2D == null)
		{
			string o = $"HUD_Player failed to load flag \"{text}\"";
			PLog.LogError(o);
		}
		return texture2D;
	}

	public static Texture2D GetShopIconTexture(string name)
	{
		Texture2D texture2D = Resources.Load("ShopImages/" + name + "_icon") as Texture2D;
		if (texture2D == null)
		{
			PLog.LogWarning("Missing shop item icon " + name);
		}
		return texture2D;
	}

	public static Texture2D GetShopImageTexture(string name)
	{
		Texture2D texture2D = Resources.Load("ShopImages/" + name + "_image") as Texture2D;
		if (texture2D == null)
		{
			PLog.LogWarning("Missing shop item image " + name);
		}
		return texture2D;
	}

	public static Texture2D GetArmamentThumbnail(string prefabName)
	{
		return Resources.Load("ArmamentThumbnails/ArmamentThumb_" + prefabName) as Texture2D;
	}

	public static Texture2D GetShipThumbnail(string prefabName)
	{
		return Resources.Load("ClassImages/ClassImage" + prefabName) as Texture2D;
	}

	public static Texture2D GetShipSilhouette(string prefabName)
	{
		return Resources.Load("ClassSilouettes/Silouette" + prefabName) as Texture2D;
	}

	public static Texture2D GetProfileShipSilhouette(string prefabName)
	{
		return Resources.Load("Renders/Class/render_" + prefabName) as Texture2D;
	}

	public static Texture2D GetProfileArmamentThumbnail(string prefabName)
	{
		return Resources.Load("Renders/Modules/render_" + prefabName) as Texture2D;
	}

	public static void SetImage(SimpleSprite sprite, string name)
	{
		Texture2D texture2D = Resources.Load(name) as Texture2D;
		if (texture2D == null)
		{
			PLog.LogWarning("Failed to load texture " + name);
		}
		else
		{
			SetImage(sprite, texture2D);
		}
	}

	public static void SetImage(SimpleSprite sprite, Texture2D f)
	{
		float width = sprite.width;
		float height = sprite.height;
		float x = f.width;
		float y = f.height;
		sprite.SetTexture(f);
		sprite.Setup(width, height, new Vector2(0f, y), new Vector2(x, y));
		sprite.UpdateUVs();
	}

	public static void SetButtonImage(UIButton uiButton, Texture2D tex)
	{
		float width = uiButton.width;
		float height = uiButton.height;
		uiButton.SetTexture(tex);
		UVAnimation[] animations = uiButton.animations;
		foreach (UVAnimation uVAnimation in animations)
		{
			SPRITE_FRAME[] array = new SPRITE_FRAME[1] { uVAnimation.GetFrame(0) };
			array[0].uvs.Set(0f, 0f, 1f, 1f);
			uVAnimation.SetAnim(array);
		}
	}

	public static void SetButtonImageSheet(UIButton uiButton, Texture2D tex)
	{
		uiButton.SetTexture(tex);
		int num = 0;
		float width = uiButton.width;
		float height = uiButton.height;
		int width2 = tex.width;
		int height2 = tex.height;
		float num2 = width / (float)width2;
		float num3 = height / (float)height2;
		UVAnimation[] animations = uiButton.animations;
		foreach (UVAnimation uVAnimation in animations)
		{
			float left = num2 * (float)num;
			SPRITE_FRAME[] array = new SPRITE_FRAME[1] { uVAnimation.GetFrame(0) };
			array[0].uvs.Set(left, 1f - num3, num2, num3);
			uVAnimation.SetAnim(array);
			num++;
		}
	}

	public static GameObject OpenInputDialog(GameObject guiCamera, string title, string text, GenericTextInput.InputTextCancel cancel, GenericTextInput.InputTextCommit ok)
	{
		GameObject gameObject = CreateGui("GenericInputDialog", guiCamera);
		GenericTextInput component = gameObject.GetComponent<GenericTextInput>();
		DebugUtils.Assert(component != null, "Failed to create GenericTextInput, prefab does not have a GenericTextInput-script on it!");
		component.Initialize(title, "$button_cancel", "$button_ok", text, cancel, ok);
		component.AllowEmptyInput = false;
		return gameObject;
	}

	public static GameObject OpenAlertDialog(GameObject guiCamera, string title, string text)
	{
		GameObject gameObject = CreateGui("dialogs/Dialog_Alert", guiCamera);
		FindChildOf(gameObject.transform, "Header").GetComponent<SpriteText>().Text = title;
		FindChildOf(gameObject.transform, "Message").GetComponent<SpriteText>().Text = text;
		return gameObject;
	}

	public static GameObject OpenMultiChoiceDialog(GameObject guiCamera, string text, EZValueChangedDelegate cancel, EZValueChangedDelegate nosave, EZValueChangedDelegate save)
	{
		GameObject gameObject = CreateGui("MsgBoxMultichoice", guiCamera);
		FindChildOf(gameObject.transform, "TextLabel").GetComponent<SpriteText>().Text = text;
		FindChildOf(gameObject.transform, "BtnCancel").GetComponent<UIButton>().AddValueChangedDelegate(cancel);
		FindChildOf(gameObject.transform, "BtnDontSave").GetComponent<UIButton>().AddValueChangedDelegate(nosave);
		FindChildOf(gameObject.transform, "BtnSave").GetComponent<UIButton>().AddValueChangedDelegate(save);
		return gameObject;
	}

	public static bool HasPointerRecursive(UIManager uiMan, GameObject root)
	{
		IUIObject component = root.GetComponent<AutoSpriteControlBase>();
		if (component == null)
		{
			component = root.GetComponent<UIScrollList>();
		}
		if (component != null && uiMan.GetPointer(component, out var _))
		{
			return true;
		}
		int childCount = root.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			if (HasPointerRecursive(uiMan, root.transform.GetChild(i).gameObject))
			{
				return true;
			}
		}
		return false;
	}
}
