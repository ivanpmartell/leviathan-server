#define DEBUG
using System;
using System.Collections.Generic;
using PTech;
using UnityEngine;

internal class Shop
{
	public Action m_onItemBought;

	private GameObject m_shopPanel;

	private GDPBackend m_gdpBackend;

	private PTech.RPC m_rpc;

	private GameObject m_guiCamera;

	private MsgBox m_msgBox;

	private UserManClient m_userMan;

	private PackMan m_pacMan;

	private GameObject m_shopItem;

	private GameObject m_shopItemOwned;

	private UIPanel m_infoPanel;

	private UIPanel m_infoPanelOwned;

	private UIScrollList m_shopList;

	private List<GDPBackend.GDPShopItem> m_gdpOffer;

	private List<GDPBackend.GDPOwnedItem> m_gdpOwned;

	private List<ShopItemData> m_shopItemData = new List<ShopItemData>();

	private List<ShopItemData> m_listedItemData = new List<ShopItemData>();

	private ShopItemData m_selectedItem;

	private bool m_updateListFlag;

	private GameObject m_redeemDialog;

	public Shop(GameObject guiCamera, GameObject shopPanel, GDPBackend gdpBackend, PTech.RPC rpc, UserManClient userMan)
	{
		m_shopPanel = shopPanel;
		m_guiCamera = guiCamera;
		m_gdpBackend = gdpBackend;
		m_rpc = rpc;
		m_userMan = userMan;
		m_pacMan = new PackMan();
		m_rpc.Register("PackList", RPC_PackList);
		m_shopItem = Resources.Load("gui/shop/ShopItemContainer") as GameObject;
		m_shopItemOwned = Resources.Load("gui/shop/ShopItemContainer_Owned") as GameObject;
		m_infoPanel = GuiUtils.FindChildOfComponent<UIPanel>(shopPanel, "ShopInfoPanel");
		m_infoPanelOwned = GuiUtils.FindChildOfComponent<UIPanel>(shopPanel, "ShopInfoPanelOwned");
		m_shopList = GuiUtils.FindChildOfComponent<UIScrollList>(shopPanel, "ShopScrollList");
		GuiUtils.FindChildOfComponent<UIButton>(shopPanel, "resetButton").SetValueChangedDelegate(OnShopRestoreOwned);
		GuiUtils.FindChildOfComponent<UIButton>(shopPanel, "redeemCodeButton").SetValueChangedDelegate(OnOpenRedeemCodeDialog);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "FilterCampaignCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "FilterMapsCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "FilterShipsCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "HideOwnedItemsCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "FilterFlagsCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "FilterDiscCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
		GuiUtils.FindChildOfComponent<UIStateToggleBtn>(shopPanel, "FilterNewCheckbox").SetValueChangedDelegate(OnShopFilterChanged);
	}

	public void Close()
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		m_rpc.Unregister("PackList");
	}

	public void Update(float dt)
	{
		if (m_msgBox != null)
		{
			m_msgBox.Update();
		}
		if (m_updateListFlag)
		{
			m_updateListFlag = false;
			UpdateItemList();
		}
	}

	public void OnShowShop(IUIObject obj)
	{
		UIPanelTab uIPanelTab = obj as UIPanelTab;
		if (uIPanelTab.Value)
		{
			m_shopPanel.GetComponent<UIPanel>().AddTempTransitionDelegate(OnShopTransitionComplete);
		}
	}

	private void OnShopTransitionComplete(UIPanelBase panel, EZTransition transition)
	{
		if (m_gdpBackend == null || !m_gdpBackend.CanPlaceOrders())
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$store_is_locked", OnNotAvailableOK);
		}
		else
		{
			m_updateListFlag = true;
		}
	}

	private void OnNotAvailableOK()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void UpdateItemList()
	{
		if (m_gdpBackend != null)
		{
			if (m_gdpBackend.CanPlaceOrders())
			{
				m_gdpBackend.m_onGotOfferList = OnGotGDPOffers;
				m_gdpBackend.RequestOffers();
			}
		}
		else
		{
			RequestPackListFromServer();
		}
	}

	private void RequestPackListFromServer()
	{
		m_rpc.Invoke("RequestPackList");
	}

	private void OnGotGDPOffers(List<GDPBackend.GDPShopItem> offer)
	{
		DebugUtils.Assert(m_gdpBackend != null);
		m_gdpOffer = offer;
		m_gdpOwned = m_gdpBackend.RequestOwned();
		PLog.Log("offer " + m_gdpOffer.Count);
		PLog.Log("owned " + m_gdpOwned.Count);
		m_shopItemData.Clear();
		foreach (GDPBackend.GDPShopItem item in m_gdpOffer)
		{
			ContentPack pack = m_pacMan.GetPack(item.m_packName);
			if (pack == null)
			{
				PLog.LogError("Missing content pack " + item.m_packName + " used by shop item " + item.m_id + " : " + item.m_presentationKey);
			}
			bool owned = IsItemOwned(item.m_packName);
			bool newItem = false;
			string undiscountedPrice = ((!item.m_discounted) ? item.m_price : item.m_undiscountedPrice);
			ContentPack.Category type = ContentPack.Category.Ships;
			if (pack != null)
			{
				type = pack.m_type;
				newItem = pack.m_newItem;
			}
			m_shopItemData.Add(new ShopItemData(item, type, item.m_packName, owned, item.m_price, undiscountedPrice, item.m_discountPercentage, newItem));
		}
		FillShopItems();
	}

	private bool IsItemOwned(string packName)
	{
		if (m_gdpBackend != null)
		{
			foreach (GDPBackend.GDPOwnedItem item in m_gdpOwned)
			{
				if (item.m_packName == packName)
				{
					return true;
				}
			}
		}
		else
		{
			foreach (ShopItemData shopItemDatum in m_shopItemData)
			{
				if (shopItemDatum.m_name == packName && shopItemDatum.m_owned)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void RPC_PackList(PTech.RPC rpc, List<object> args)
	{
		m_shopItemData.Clear();
		int num = 0;
		int num2 = (int)args[num++];
		for (int i = 0; i < num2; i++)
		{
			int num3 = i;
			string text = (string)args[num++];
			bool flag = (bool)args[num++];
			ContentPack.Category category = (ContentPack.Category)(int)args[num++];
			double num4 = UnityEngine.Random.Range(0, 5);
			double num5 = num4;
			bool flag2 = UnityEngine.Random.value > 0.5f;
		}
		PLog.Log("got packs " + m_listedItemData.Count);
		FillShopItems();
	}

	private void GetShopListMask(out int categories, out bool hideOwned, out bool saleOnly, out bool newOnly)
	{
		UIStateToggleBtn uIStateToggleBtn = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "FilterCampaignCheckbox");
		UIStateToggleBtn uIStateToggleBtn2 = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "FilterMapsCheckbox");
		UIStateToggleBtn uIStateToggleBtn3 = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "FilterShipsCheckbox");
		UIStateToggleBtn uIStateToggleBtn4 = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "FilterFlagsCheckbox");
		UIStateToggleBtn uIStateToggleBtn5 = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "HideOwnedItemsCheckbox");
		UIStateToggleBtn uIStateToggleBtn6 = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "FilterDiscCheckbox");
		UIStateToggleBtn uIStateToggleBtn7 = GuiUtils.FindChildOfComponent<UIStateToggleBtn>(m_shopPanel, "FilterNewCheckbox");
		categories = 0;
		if (uIStateToggleBtn.StateNum == 0)
		{
			categories |= 4;
		}
		if (uIStateToggleBtn2.StateNum == 0)
		{
			categories |= 1;
		}
		if (uIStateToggleBtn3.StateNum == 0)
		{
			categories |= 2;
		}
		if (uIStateToggleBtn4.StateNum == 0)
		{
			categories |= 8;
		}
		hideOwned = uIStateToggleBtn5.StateNum == 0;
		saleOnly = uIStateToggleBtn6.StateNum == 0;
		newOnly = uIStateToggleBtn7.StateNum == 0;
	}

	private void OnShopRestoreOwned(IUIObject obj)
	{
		if (m_gdpBackend != null)
		{
			m_msgBox = MsgBox.CreateTextOnlyMsgBox(m_guiCamera, "$store_restoring_owned");
			m_gdpBackend.m_onRestoreOwnedFinished = OnRestoreOwnedFinished;
			m_gdpBackend.RestoreOwned();
		}
	}

	private void OnOpenRedeemCodeDialog(IUIObject obj)
	{
		m_redeemDialog = GuiUtils.OpenInputDialog(m_guiCamera, Localize.instance.Translate("$shop_enterredeemcode"), string.Empty, OnRedeemCancel, OnRedeemOk);
	}

	private void OnRedeemOk(string text)
	{
		UnityEngine.Object.Destroy(m_redeemDialog);
		if (m_gdpBackend != null)
		{
			m_gdpBackend.m_onRedeemRespons = OnRedeemRespons;
			m_gdpBackend.RedeemCode(text);
		}
	}

	private void OnRedeemCancel()
	{
		UnityEngine.Object.Destroy(m_redeemDialog);
	}

	private void OnRestoreOwnedFinished(bool success, string error)
	{
		if (m_msgBox != null)
		{
			m_msgBox.Close();
			m_msgBox = null;
		}
		if (success)
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$store_restore_owned_done", OnRestoreFailedOK);
		}
		else
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$store_restore_owned_failed " + error, OnRestoreFailedOK);
		}
		UpdateItemList();
	}

	private void OnRestoreFailedOK()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void OnShopFilterChanged(IUIObject obj)
	{
		FillShopItems();
	}

	private void FillShopItems()
	{
		GetShopListMask(out var categories, out var hideOwned, out var saleOnly, out var newOnly);
		float scrollPosition = m_shopList.ScrollPosition;
		m_listedItemData.Clear();
		m_shopList.ClearList(destroy: true);
		foreach (ShopItemData shopItemDatum in m_shopItemData)
		{
			if (((uint)shopItemDatum.m_type & (uint)categories) != 0 && (!shopItemDatum.m_owned || !hideOwned) && (!newOnly || shopItemDatum.m_newItem) && (!saleOnly || shopItemDatum.m_discountPercentage != 0.0))
			{
				GameObject gameObject = ((!shopItemDatum.m_owned) ? (UnityEngine.Object.Instantiate(m_shopItem) as GameObject) : (UnityEngine.Object.Instantiate(m_shopItemOwned) as GameObject));
				GuiUtils.LocalizeGui(gameObject);
				SimpleSprite sprite = GuiUtils.FindChildOfComponent<SimpleSprite>(gameObject, "Image");
				GameObject gameObject2 = GuiUtils.FindChildOf(gameObject, "NewRibbon");
				SpriteText spriteText = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "TitleLabel");
				SpriteText spriteText2 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "DescriptionLabel");
				SpriteText spriteText3 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "PriceValueLabel");
				SpriteText spriteText4 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "PriceValueLabel_Discount");
				SpriteText spriteText5 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "DiscountLabel");
				SpriteText spriteText6 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "DiscountValueLabel");
				SpriteText spriteText7 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "FlavorSmallLabel");
				SpriteText spriteText8 = GuiUtils.FindChildOfComponent<SpriteText>(gameObject, "FlavorBigLabel");
				UIButton uIButton = GuiUtils.FindChildOfComponent<UIButton>(gameObject, "MoreInfoButton");
				UIButton uIButton2 = GuiUtils.FindChildOfComponent<UIButton>(gameObject, "BuyButton");
				if (gameObject2 != null && !shopItemDatum.m_newItem)
				{
					gameObject2.transform.Translate(new Vector3(10000f, 0f, 0f));
				}
				Texture2D shopIconTexture = GuiUtils.GetShopIconTexture(shopItemDatum.m_name);
				if (shopIconTexture != null)
				{
					GuiUtils.SetImage(sprite, shopIconTexture);
				}
				spriteText.Text = Localize.instance.TranslateKey("shopitem_" + shopItemDatum.m_name + "_name");
				spriteText2.Text = Localize.instance.TranslateKey("shopitem_" + shopItemDatum.m_name + "_description");
				if (shopItemDatum.m_price == string.Empty)
				{
					spriteText3.Text = Localize.instance.Translate("$shop_free");
					spriteText4.Text = string.Empty;
					spriteText3.SetColor(Color.green);
				}
				else if (shopItemDatum.m_undiscountedPrice != shopItemDatum.m_price)
				{
					spriteText3.Text = string.Empty;
					spriteText4.Text = shopItemDatum.m_undiscountedPrice;
				}
				else
				{
					spriteText3.Text = shopItemDatum.m_price;
					spriteText4.Text = string.Empty;
				}
				if (shopItemDatum.m_discountPercentage != 0.0)
				{
					spriteText5.Text = "-" + (int)(shopItemDatum.m_discountPercentage * 100.0) + "%";
					spriteText6.Text = shopItemDatum.m_price;
				}
				else
				{
					spriteText5.Text = string.Empty;
					spriteText6.Text = string.Empty;
				}
				uIButton.SetValueChangedDelegate(OnShopItemInfo);
				if (uIButton2 != null)
				{
					uIButton2.SetValueChangedDelegate(OnShopItemBuy);
					uIButton2.controlIsEnabled = !shopItemDatum.m_owned;
				}
				GuiUtils.FixedItemContainerInstance(gameObject.GetComponent<UIListItemContainer>());
				m_shopList.AddItem(gameObject);
				m_listedItemData.Add(shopItemDatum);
			}
		}
		m_shopList.ScrollToItem(0, 0f);
		m_shopList.ScrollPosition = scrollPosition;
	}

	private void OnShopItemBuy(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		int index = component.Index;
		m_selectedItem = m_listedItemData[index];
		BuyShopItem(m_selectedItem);
	}

	private void OnShopItemInfo(IUIObject obj)
	{
		UIListItemContainer component = obj.transform.parent.GetComponent<UIListItemContainer>();
		int index = component.Index;
		m_selectedItem = m_listedItemData[index];
		OpenItemInfoDialog();
	}

	private void OpenItemInfoDialog()
	{
		ShopItemData selectedItem = m_selectedItem;
		UIPanel uIPanel = ((!selectedItem.m_owned) ? m_infoPanel : m_infoPanelOwned);
		SimpleSprite sprite = GuiUtils.FindChildOfComponent<SimpleSprite>(uIPanel.gameObject, "iconImage");
		SpriteText spriteText = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "nameLabel");
		SpriteText spriteText2 = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "descriptionLabel");
		SpriteText spriteText3 = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "longDescriptionLabel");
		UIScrollList uIScrollList = GuiUtils.FindChildOfComponent<UIScrollList>(uIPanel.gameObject, "longDescriptionScrollList");
		SpriteText spriteText4 = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "priceValueLabel");
		SpriteText spriteText5 = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "priceValueLabel_Discount");
		SpriteText spriteText6 = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "discountLabel");
		SpriteText spriteText7 = GuiUtils.FindChildOfComponent<SpriteText>(uIPanel.gameObject, "discountValueLabel");
		UIButton uIButton = GuiUtils.FindChildOfComponent<UIButton>(uIPanel.gameObject, "buyButton");
		UIButton uIButton2 = GuiUtils.FindChildOfComponent<UIButton>(uIPanel.gameObject, "closeButton");
		if (uIButton != null)
		{
			uIButton.SetValueChangedDelegate(OnShopInfoBuy);
			uIButton.controlIsEnabled = !selectedItem.m_owned;
		}
		uIButton2.SetValueChangedDelegate(OnShopInfoClose);
		Texture2D shopImageTexture = GuiUtils.GetShopImageTexture(selectedItem.m_name);
		if (shopImageTexture != null)
		{
			GuiUtils.SetImage(sprite, shopImageTexture);
		}
		spriteText.Text = Localize.instance.TranslateKey("shopitem_" + selectedItem.m_name + "_name");
		spriteText2.Text = Localize.instance.TranslateKey("shopitem_" + selectedItem.m_name + "_description");
		spriteText3.Text = Localize.instance.TranslateRecursive("$shopitem_" + selectedItem.m_name + "_longdescription");
		uIScrollList.ScrollToItem(0, 0f);
		if (selectedItem.m_price == string.Empty)
		{
			spriteText4.Text = Localize.instance.Translate("$shop_free");
			spriteText5.Text = string.Empty;
			spriteText4.SetColor(Color.green);
		}
		else if (selectedItem.m_undiscountedPrice != selectedItem.m_price)
		{
			spriteText4.Text = string.Empty;
			spriteText5.Text = selectedItem.m_undiscountedPrice;
		}
		else
		{
			spriteText4.Text = selectedItem.m_price;
			spriteText5.Text = string.Empty;
		}
		if (selectedItem.m_discountPercentage != 0.0)
		{
			spriteText6.Text = "-" + (int)(selectedItem.m_discountPercentage * 100.0) + "%";
			spriteText7.Text = selectedItem.m_price;
		}
		else
		{
			spriteText6.Text = string.Empty;
			spriteText7.Text = string.Empty;
		}
		uIPanel.BringIn();
	}

	private void OnShopInfoClose(IUIObject obj)
	{
		m_infoPanel.Dismiss();
		m_infoPanelOwned.Dismiss();
	}

	private void OnShopInfoBuy(IUIObject obj)
	{
		ShopItemData selectedItem = m_selectedItem;
		BuyShopItem(selectedItem);
		m_infoPanel.Dismiss();
		m_infoPanelOwned.Dismiss();
	}

	private void OnRedeemRespons(bool success, string errorMessage)
	{
		if (success)
		{
			if (m_onItemBought != null)
			{
				m_onItemBought();
			}
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$store_redeem_success ", OnItemBoughtOk);
		}
		else
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$store_redeem_error " + errorMessage, OnItemBoughtOk);
		}
	}

	private void OnBoughtItem(string pkgName)
	{
		if (m_onItemBought != null)
		{
			m_onItemBought();
		}
		string text = string.Empty;
		if (pkgName != string.Empty)
		{
			text = Localize.instance.TranslateKey("shopitem_" + pkgName + "_name");
		}
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$store_item_bought " + text, OnItemBoughtOk);
	}

	private void OnItemBoughtOk()
	{
		m_msgBox.Close();
		m_msgBox = null;
		UpdateItemList();
	}

	private void BuyShopItem(ShopItemData item)
	{
		if (m_gdpBackend != null)
		{
			string description = Localize.instance.TranslateKey("shopitem_" + item.m_name + "_name");
			m_gdpBackend.m_onBoughtItem = OnBoughtItem;
			m_gdpBackend.m_onOrderFailed = OnOrderFailed;
			m_gdpBackend.PlaceOrder(item.m_gdpItem, description);
		}
		else
		{
			m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$shop_bought", OnShopBuyOk);
			m_userMan.BuyPackage(item.m_name);
			UpdateItemList();
		}
	}

	private void OnOrderFailed(string error)
	{
		m_msgBox = MsgBox.CreateOkMsgBox(m_guiCamera, "$shop_buy_failed \"" + error + "\"", OnShopFailedOk);
	}

	private void OnShopFailedOk()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}

	private void OnShopBuyOk()
	{
		m_msgBox.Close();
		m_msgBox = null;
	}
}
