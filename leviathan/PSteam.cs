using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using MiniJSON;

internal class PSteam : GDPBackend
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void PrintDelegate(string text);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void OfferDelegate(int id, string presentationKey, string currency, double price, bool discounted, double undiscountedPrice, int dateYear, int dateMonth, int dateDay);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void OwnedDelegate(string presentationKey, int instance);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void BoughtDelegate(string presentationKey);

	private PrintDelegate m_printCallback;

	private OfferDelegate m_offerCallback;

	private OwnedDelegate m_ownedCallback;

	private BoughtDelegate m_boughtCallback;

	private List<GDPShopItem> m_offer = new List<GDPShopItem>();

	private List<GDPOwnedItem> m_owned = new List<GDPOwnedItem>();

	private string m_redeemBaseUrl = "http://api.paradoxplaza.com/redeem/claim";

	private WebClient m_redeemWebclient;

	public PSteam(bool live)
	{
		m_printCallback = PSteamLog;
		m_offerCallback = OnOffer;
		m_ownedCallback = OnOwned;
		m_boughtCallback = OnBought;
		PLog.Log("Initializing psteam");
		string catalogue = ((!live) ? "leviathan-stage" : "leviathan");
		if (!PSteamInitialize(m_printCallback, "Leviathan", catalogue, "1.0.0", live))
		{
			PLog.LogError("Failed to initialize PSteam");
			throw new Exception("Steam setup failed");
		}
	}

	[DllImport("psteam")]
	private static extern bool PSteamInitialize(PrintDelegate logFunction, string gameName, string catalogue, string gameVersion, bool live);

	[DllImport("psteam")]
	private static extern void PSteamDeinitialize();

	[DllImport("psteam")]
	private static extern void PSteamUpdate();

	[DllImport("psteam")]
	private static extern void PSteamRequestOffers(OfferDelegate callback);

	[DllImport("psteam")]
	private static extern void PSteamRequestOwned(OwnedDelegate callback);

	[DllImport("psteam")]
	private static extern bool PSteamPlaceOrder(int offerId, string description, BoughtDelegate callback);

	[DllImport("psteam")]
	private static extern bool PSteamGetFreeOffer(int offerId, BoughtDelegate callback);

	[DllImport("psteam")]
	private static extern void PSteamUnlockAchievement(string name);

	[DllImport("psteam")]
	private static extern void PSteamOpenWebUrl(string name);

	[DllImport("psteam")]
	private static extern string PSteamGetSteamID();

	[DllImport("psteam")]
	private static extern bool PSteamIsSubscribedApp(uint appID);

	[DllImport("psteam")]
	private static extern bool PSteamIsPigsOnline();

	public override void RedeemCode(string code)
	{
		string text = PSteamGetSteamID();
		PLog.Log("Steam id " + text);
		if (Redeem(code, out var message))
		{
			m_onRedeemRespons(arg1: true, string.Empty);
		}
		else
		{
			m_onRedeemRespons(arg1: false, message);
		}
	}

	private bool Redeem(string code, out string message)
	{
		//Discarded unreachable code: IL_0035, IL_00a6
		string arg = PSteamGetSteamID();
		string format = "http://api.paradoxplaza.com/redeem/claim?universe={0}&userid={1}&code={2}";
		string address = string.Format(format, "steam", arg, code);
		try
		{
			string text = new WebClient().DownloadString(address);
			message = string.Empty;
			return true;
		}
		catch (WebException ex)
		{
			WebResponse response = ex.Response;
			HttpWebResponse httpWebResponse = (HttpWebResponse)response;
			Stream responseStream = response.GetResponseStream();
			string json = new StreamReader(responseStream).ReadToEnd();
			IDictionary dictionary = (IDictionary)Json.Deserialize(json);
			message = "error";
			if (dictionary.Contains("errorMessage"))
			{
				message = dictionary["errorMessage"] as string;
			}
			return false;
		}
	}

	private void OnDownloaded(object sender, DownloadStringCompletedEventArgs e)
	{
		PLog.Log("Respons is here...finally been waiting for hours...");
		if (!e.Cancelled && e.Error == null)
		{
			string result = e.Result;
			PLog.Log("Derp:" + e.Result);
		}
		else
		{
			PLog.Log("Error " + e.Error.ToString());
		}
	}

	public override void RequestOffers()
	{
		m_offer.Clear();
		PSteamRequestOffers(m_offerCallback);
		m_onGotOfferList(m_offer);
	}

	public override List<GDPOwnedItem> RequestOwned()
	{
		m_owned.Clear();
		PSteamRequestOwned(m_ownedCallback);
		AddDLCContent(ref m_owned);
		return m_owned;
	}

	protected void AddDLCContent(ref List<GDPOwnedItem> items)
	{
		if (PSteamIsSubscribedApp(Constants.m_SteamPrePurchaseDlcID))
		{
			AddPacks(ref items, Constants.m_SteamPrePurchaseDlcPacks);
		}
		if (PSteamIsSubscribedApp(Constants.m_Steam_CommonWealth_pack2_DlcID))
		{
			AddPacks(ref items, Constants.m_Steam_CommonWealth_pack2_DlcPacks);
		}
	}

	public override void PlaceOrder(GDPShopItem item, string description)
	{
		if (item.m_price == string.Empty)
		{
			if (!PSteamGetFreeOffer(item.m_id, m_boughtCallback))
			{
				m_onOrderFailed("unknown");
			}
		}
		else if (!PSteamPlaceOrder(item.m_id, description, m_boughtCallback))
		{
			m_onOrderFailed("unknown");
		}
	}

	public override void Close()
	{
		PLog.Log("Shutting down psteam");
		PSteamDeinitialize();
		PLog.Log("  done");
	}

	public override void Update()
	{
		PSteamUpdate();
	}

	public override void UnlockAchievement(string name)
	{
		PSteamUnlockAchievement(name);
	}

	public override void OpenWebUrl(string url)
	{
		PSteamOpenWebUrl(url);
	}

	private void PSteamLog(string text)
	{
		PLog.Log("PSTEAM: " + text);
	}

	private string FormatPrice(double price, string currency)
	{
		switch (currency)
		{
		case "EUR":
			return Localize.instance.Translate("$shop_eur") + price;
		case "USD":
			return "$" + price;
		case "GBP":
			return Localize.instance.Translate("$shop_gbp") + price;
		case "RUB":
			return Localize.instance.Translate("$shop_rub ") + price;
		default:
			PLog.LogWarning("Unkown currency: " + currency);
			return price.ToString();
		}
	}

	private void OnOffer(int id, string presentationKey, string currency, double price, bool discounted, double undiscountedPrice, int dateYear, int dateMonth, int dateDay)
	{
		PLog.Log("Got offer " + id + " " + presentationKey + " " + currency + " " + price + " " + discounted + " " + undiscountedPrice + "  " + dateYear + " " + dateMonth + " " + dateDay);
		if (presentationKey.StartsWith("leviathan."))
		{
			GDPShopItem gDPShopItem = new GDPShopItem();
			gDPShopItem.m_id = id;
			gDPShopItem.m_presentationKey = presentationKey;
			gDPShopItem.m_price = ((price != 0.0) ? FormatPrice(price, currency) : string.Empty);
			gDPShopItem.m_discounted = discounted;
			if (discounted)
			{
				gDPShopItem.m_undiscountedPrice = FormatPrice(undiscountedPrice, currency);
				gDPShopItem.m_discountPercentage = 1.0 - price / undiscountedPrice;
			}
			gDPShopItem.m_dateYear = dateYear;
			gDPShopItem.m_dateMonth = dateMonth;
			gDPShopItem.m_dateDay = dateDay;
			gDPShopItem.m_packName = GetPackName(presentationKey);
			m_offer.Add(gDPShopItem);
		}
	}

	private void OnOwned(string presentationKey, int instance)
	{
		PLog.Log("Got offer " + presentationKey + " " + instance);
		if (presentationKey.StartsWith("leviathan."))
		{
			GDPOwnedItem gDPOwnedItem = new GDPOwnedItem();
			gDPOwnedItem.m_itemType = presentationKey;
			gDPOwnedItem.m_instance = instance;
			gDPOwnedItem.m_packName = GetPackName(presentationKey);
			m_owned.Add(gDPOwnedItem);
		}
	}

	private void OnBought(string presentationKey)
	{
		PLog.Log("bought item " + presentationKey);
		string packName = GetPackName(presentationKey);
		m_onBoughtItem(packName);
	}

	private string GetPackName(string presentationKey)
	{
		int length = "leviathan.".Length;
		return presentationKey.Substring(length);
	}

	public override bool IsBackendOnline()
	{
		return PSteamIsPigsOnline();
	}
}
