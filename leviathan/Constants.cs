using UnityEngine;

internal class Constants
{
	public enum AchivementId
	{
		Ach_None = -1,
		Ach_ItFloatsAlright,
		Ach_ShipWright,
		Ach_InternationalWaters,
		Ach_NotForverAlone,
		Ach_SurvivedTheFury,
		Ach_StrongMan,
		Ach_IceCold,
		Ach_Blended,
		Ach_NoName,
		Ach_NippedAtTheHeels,
		Ach_WinWaves3,
		Ach_Deconstructed,
		Ach_AltasCrush,
		Ach_YouAreTiny,
		Ach_Trafalgar,
		Ach_PowerDunk,
		Ach_ExtremePowerDunk,
		Ach_HaveOnOnUs,
		Ach_TheGreatGiant,
		Ach_YouAreAWizard
	}

	public const bool m_debugLocalize = false;

	public const float m_groundedDamagePercentage = 0.2f;

	public const int m_repairCost = 12;

	public const int m_repairPerSupply = 10;

	public const float m_gravity = 5f;

	public const float m_damageForceMultiplier = 4f;

	public const float m_maintenanceModeEnterDelay = 1f;

	public const float m_maintenanceModeExitDelay = 6f;

	public const float m_sinkHealthThreshold = 0.35f;

	public const float m_sinkChance = 0.08f;

	public const float m_engineDamageHealthThreshold = 0.9f;

	public const float m_engineDamageChance = 0.06f;

	public const float m_bridgeDamageHealthThreshold = 0.75f;

	public const float m_bridgeDamageChance = 0.07f;

	public const float m_outOfControlHealthThreshold = 0.75f;

	public const float m_outOfControlChance = 0.07f;

	public const float m_monsterMine_DebuffSpeed = 0.5f;

	public const float m_monsterMine_Dot = 35f;

	public const int m_monsterMine_Ap = 25;

	public const float m_shipyardZoomTime = 0.5f;

	public const float m_shipyardZoomMin = 20f;

	public const float m_shipyardZoomMax = 60f;

	public const float m_shipyardMoveMaxX = 10f;

	public const float m_shipyardMoveMaxZ = 10f;

	public const float m_shipyardFleetZoom = 80f;

	public const int m_maxShipsInFleet = 8;

	public const int m_maxHardpointsOnShip = 8;

	public const float m_messageFadeTime = 0.5f;

	public const float m_messageDisplayTime = 2f;

	public const float m_messageTimeNewsflash = 4f;

	public const float m_messageTimeObjective = 4f;

	public const float m_messageTimeObjectiveDone = 3f;

	public const float m_messageTimeTurn = 0.6f;

	public const float m_messageTimeVictory = 4f;

	public const float m_tooltipShowDelay = 1f;

	public const float m_tooltipHideDelay = 4f;

	public const float m_tooltipZ = -80f;

	public const float m_standardMusicVolume = 0.5f;

	public const float m_standardSfxVolume = 1f;

	public const int m_connectTimeout = 10000;

	public const float m_androidTimeout = 300f;

	public static string[] m_languages = new string[2] { "english", "german" };

	public static readonly Color[] m_coopColors = new Color[4]
	{
		new Color(0.98f, 0.31f, 0.04f),
		new Color(0.04f, 0.46f, 0.98f),
		new Color(0.83f, 0.76f, 0.05f),
		new Color(0.18f, 0.67f, 0.16f)
	};

	public static readonly Color[] m_teamColors1 = new Color[3]
	{
		new Color(0.98f, 0.31f, 0.04f),
		new Color(0.96f, 0.12f, 0.14f),
		new Color(0.27f, 0.06f, 0.01f)
	};

	public static readonly Color[] m_teamColors2 = new Color[3]
	{
		new Color(0.03f, 0.6f, 0.99f),
		new Color(0.05f, 0.66f, 0.95f),
		new Color(0f, 0.15f, 0.36f)
	};

	public static string m_buffColor = "[#00FF00]";

	public static string m_nerfColor = "[#FF0000]";

	public static HitTextDef m_shipCriticalHit = new HitTextDef(HitTextDef.FontSize.Medium, new Color(1f, 0f, 0f));

	public static HitTextDef m_shipGlancingHit = new HitTextDef(HitTextDef.FontSize.Medium, new Color(1f, 1f, 0f));

	public static HitTextDef m_shipPiercingHit = new HitTextDef(HitTextDef.FontSize.Medium, new Color(1f, 1f, 0f));

	public static HitTextDef m_shipDeflectHit = new HitTextDef(HitTextDef.FontSize.Medium, new Color(0.8f, 0.8f, 0.8f), "$hittext_shipdeflecthit", string.Empty);

	public static HitTextDef m_shipDestroyedHit = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 0f, 0f), string.Empty, "$hittext_shipdestroyedhit");

	public static HitTextDef m_shipGroundedText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 1f), "$hittext_shipgroundedtext", string.Empty);

	public static HitTextDef m_shipSinkingWarningText = new HitTextDef(HitTextDef.FontSize.Medium, new Color(1f, 1f, 1f), "$hittext_shipsinkingwarningtext ", string.Empty);

	public static HitTextDef m_shipSinkingText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 0f), "$hittext_shipsinkingtext", string.Empty);

	public static HitTextDef m_shipOutOfControlText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 0f), "$hittext_outofcontrol", string.Empty);

	public static HitTextDef m_shipBridgeDamagedText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 0f), "$hittext_bridgedamaged", string.Empty);

	public static HitTextDef m_shipEngineDamagedText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 0f), "$hittext_enginedamaged", string.Empty);

	public static HitTextDef m_moduleCriticalHit = new HitTextDef(HitTextDef.FontSize.Small, new Color(1f, 0f, 0f));

	public static HitTextDef m_moduleGlancingHit = new HitTextDef(HitTextDef.FontSize.Small, new Color(1f, 1f, 0f));

	public static HitTextDef m_modulePiercingHit = new HitTextDef(HitTextDef.FontSize.Small, new Color(1f, 1f, 0f));

	public static HitTextDef m_moduleDeflectHit = new HitTextDef(HitTextDef.FontSize.Small, new Color(0.8f, 0.8f, 0.8f), "$hittext_moduledeflecthit", string.Empty);

	public static HitTextDef m_moduleDisabledHit = new HitTextDef(HitTextDef.FontSize.Medium, new Color(1f, 0f, 0f), string.Empty, "$hittext_moduledisabledhit");

	public static HitTextDef m_shieldAbsorbedText = new HitTextDef(HitTextDef.FontSize.Small, new Color(0.8f, 0.8f, 0.8f), "$hittext_shieldabsorbedtext", string.Empty);

	public static HitTextDef m_pointsText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 1f), string.Empty, "$hittext_points");

	public static HitTextDef m_assassinatedText = new HitTextDef(HitTextDef.FontSize.Large, new Color(1f, 1f, 0f), "$hittext_assassinatedtext", string.Empty);

	public static readonly Color m_shipYardSize_Valid = new Color(0f, 0.64f, 0.07f);

	public static readonly Color m_shipYardSize_Invalid = new Color(0.64f, 0.09f, 0f);

	public static int[] m_achivements = new int[20]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19
	};

	public static bool[] m_achivementHidden = new bool[20]
	{
		false, false, false, false, false, false, false, false, false, false,
		false, true, false, false, false, false, true, false, false, false
	};

	public static string[] m_IosDLCProductIDs = new string[5] { "maps_challenge_map1", "maps_versus_pack1", "unit_elites_pack1", "unit_marauders_pack1", "unit_commonwealth_pack1" };

	public static string[] m_IosFreeDlcPacks = new string[1] { "maps_versus_pack2" };

	public static uint m_SteamPrePurchaseDlcID = 236310u;

	public static string[] m_SteamPrePurchaseDlcPacks = new string[1] { "unit_marauders_pack1" };

	public static uint m_Steam_CommonWealth_pack2_DlcID = 245030u;

	public static string[] m_Steam_CommonWealth_pack2_DlcPacks = new string[1] { "unit_commonwealth_pack1" };

	public static double[] m_turnTimeLimits = new double[6] { 30.0, 60.0, 300.0, 3600.0, 86400.0, 604800.0 };
}
