using System;
using System.Collections.Generic;
using System.IO;

namespace PTech
{
	internal abstract class ServerBase
	{
		protected MapMan m_mapMan;

		public ServerBase(MapMan mapman)
		{
			m_mapMan = mapman;
		}

		protected abstract Game CreateGame(string name, int gameID, int campaignID, GameType gameType, string campaignName, string levelName, int nrOfPlayers, FleetSizeClass fleetSizeClass, float targetScore, double turnTime, MapInfo mapinfo);

		protected void OnGameOver(Game game)
		{
			if (game.GetGameType() == GameType.Campaign)
			{
				OnCampaignGameEnded(game);
			}
		}

		private void OnCampaignGameEnded(Game game)
		{
			GameOutcome outcome = game.GetOutcome();
			if (outcome != GameOutcome.Victory && outcome != GameOutcome.Defeat)
			{
				return;
			}
			MapInfo mapInfo = ((outcome != GameOutcome.Victory) ? m_mapMan.GetMapByName(game.GetGameType(), game.GetCampaign(), game.GetLevelName()) : m_mapMan.GetNextCampaignMap(game.GetCampaign(), game.GetLevelName()));
			if (mapInfo == null)
			{
				return;
			}
			string name = game.GetName();
			Game game2 = CreateGame(name, 0, game.GetCampaignID(), game.GetGameType(), game.GetCampaign(), mapInfo.m_name, game.GetNrOfPlayers(), game.GetFleetSizeClass(), game.GetTargetScore(), game.GetMaxTurnTime(), mapInfo);
			game.SetAutoJoinNextGameID(game2.GetGameID());
			List<User> nextGameUserList = game.GetNextGameUserList();
			foreach (User item in nextGameUserList)
			{
				item.UnlockCampaignMap(game2.GetCampaign(), mapInfo.m_name);
				game2.AddUserToGame(item, game.IsAdmin(item));
				string playerFleet = game.GetPlayerFleet(item.m_name);
				if (playerFleet != string.Empty)
				{
					game2.SetPlayerFleet(item.m_name, playerFleet);
				}
			}
		}

		protected Game CreateGameFromArray(byte[] data, string overrideGameName)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			string name = binaryReader.ReadString();
			int gameID = binaryReader.ReadInt32();
			int campaignID = binaryReader.ReadInt32();
			GameType gameType = (GameType)binaryReader.ReadInt32();
			string text = binaryReader.ReadString();
			string text2 = binaryReader.ReadString();
			int nrOfPlayers = binaryReader.ReadInt32();
			FleetSizeClass fleetSizeClass = (FleetSizeClass)binaryReader.ReadInt32();
			float targetScore = binaryReader.ReadSingle();
			double turnTime = binaryReader.ReadDouble();
			MapInfo mapByName = m_mapMan.GetMapByName(gameType, text, text2);
			if (mapByName == null)
			{
				PLog.LogError(string.Concat("Missing map info for ", gameType, " ", text, " ", text2));
				throw new Exception(string.Concat("Missing map info for ", gameType, " ", text, " ", text2));
			}
			if (overrideGameName != string.Empty)
			{
				name = overrideGameName;
			}
			Game game = CreateGame(name, gameID, campaignID, gameType, text, text2, nrOfPlayers, fleetSizeClass, targetScore, turnTime, mapByName);
			game.LoadData(binaryReader);
			return game;
		}

		protected byte[] GameToArray(Game game)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(1);
			binaryWriter.Write(game.GetName());
			binaryWriter.Write(game.GetGameID());
			binaryWriter.Write(game.GetCampaignID());
			binaryWriter.Write((int)game.GetGameType());
			binaryWriter.Write(game.GetCampaign());
			binaryWriter.Write(game.GetLevelName());
			binaryWriter.Write(game.GetMaxPlayers());
			binaryWriter.Write((int)game.GetFleetSizeClass());
			binaryWriter.Write(game.GetTargetScore());
			binaryWriter.Write(game.GetMaxTurnTime());
			game.SaveData(binaryWriter);
			return memoryStream.ToArray();
		}
	}
}
