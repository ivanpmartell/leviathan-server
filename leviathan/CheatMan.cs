using System;
using PTech;
using UnityEngine;

internal class CheatMan
{
	public GameOutcome m_forceEndGame;

	public bool m_playerIsImmortal;

	public bool m_playerHasInstagibGun;

	public bool m_showFps;

	public bool m_noDamage;

	public bool m_noFogOfWar;

	public bool m_debugAi;

	private static CheatMan m_instance;

	public static CheatMan instance => m_instance;

	public CheatMan()
	{
		m_instance = this;
		ResetCheats();
		if (Debug.isDebugBuild)
		{
			m_showFps = true;
		}
	}

	public void Close()
	{
		m_instance = null;
	}

	private bool IsCheatsAvaible()
	{
		return Debug.isDebugBuild;
	}

	private void PrintHelp(Hud hud, string text)
	{
		if (hud != null)
		{
			ChatClient.ChatMessage msg = new ChatClient.ChatMessage(DateTime.Now, "Cheat", text);
			hud.AddChatMessage(ChannelID.General, msg);
		}
	}

	public void ActivateCheat(string name, Hud hud)
	{
		if (!IsCheatsAvaible())
		{
			return;
		}
		if (name == "cheats")
		{
			string text = "win - wins the game next commit\n";
			text += "lose - lose the game next commit.\n";
			text += "god - Toggle player 0 no longer takes damage.\n";
			text += "instagib - Toggle huge damage on first player.\n";
			text += "fpslock - Lock render fps to min 20.\n";
			text += "showfps - Toggle fps display.\n";
			text += "nodmg - Toggle no damage (all ship resist damage)\n";
			text += "warfog - Toggle draw of fog of war\n";
			text += "brains - Debug AI";
			PrintHelp(hud, text);
		}
		if (name == "blame")
		{
			PrintHelp(hud, "Dumping memory usage");
			Utils.DumpMemoryUsage();
		}
		if (name == "win")
		{
			m_forceEndGame = GameOutcome.Victory;
			PrintHelp(hud, "Game will be won next turn");
		}
		if (name == "lose")
		{
			m_forceEndGame = GameOutcome.Defeat;
			PrintHelp(hud, "Game will be lost next turn");
		}
		if (name == "god")
		{
			m_playerIsImmortal = !m_playerIsImmortal;
			PrintHelp(hud, "Player is immortal: " + m_playerIsImmortal);
		}
		if (name == "instagib")
		{
			m_playerHasInstagibGun = !m_playerHasInstagibGun;
			PrintHelp(hud, "Player has Instagib Gun: " + m_playerHasInstagibGun);
		}
		if (name == "nodmg")
		{
			m_noDamage = !m_noDamage;
			PrintHelp(hud, "All ships damage immune: " + m_noDamage);
		}
		if (name == "warfog")
		{
			m_noFogOfWar = !m_noFogOfWar;
			PrintHelp(hud, "Fog of War Drawer: " + m_noFogOfWar);
		}
		if (name == "brains")
		{
			m_debugAi = !m_debugAi;
			PrintHelp(hud, "Debug AI: " + m_debugAi);
		}
		if (name == "fpslock")
		{
			float num = 0.05f;
			float maximumDeltaTime = 0.2f;
			if (Time.maximumDeltaTime <= num + 0.01f)
			{
				Time.maximumDeltaTime = maximumDeltaTime;
				PrintHelp(hud, "unlocking fps");
			}
			else
			{
				Time.maximumDeltaTime = num;
				PrintHelp(hud, "locking fps");
			}
		}
		if (name == "showfps")
		{
			m_showFps = !m_showFps;
		}
	}

	public void ResetCheats()
	{
		m_forceEndGame = GameOutcome.None;
		m_playerIsImmortal = false;
		m_playerHasInstagibGun = false;
		m_noDamage = false;
	}

	public GameOutcome GetEndGameStatus()
	{
		return m_forceEndGame;
	}

	public bool GetPlayerImmortal()
	{
		return m_playerIsImmortal;
	}

	public bool GetNoDamage()
	{
		return m_noDamage;
	}

	public bool GetNoFogOfWar()
	{
		return m_noFogOfWar;
	}

	public bool GetInstaGib()
	{
		return m_playerHasInstagibGun;
	}

	public bool DebugAi()
	{
		return m_debugAi;
	}
}
