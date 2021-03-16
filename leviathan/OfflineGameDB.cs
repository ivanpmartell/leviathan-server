using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal class OfflineGameDB
{
	public List<GamePost> GetGameList()
	{
		List<GamePost> list = new List<GamePost>();
		string[] files = Directory.GetFiles(Application.persistentDataPath);
		string[] array = files;
		foreach (string text in array)
		{
			if (System.IO.Path.GetExtension(text) == ".gam")
			{
				GamePost item = LoadGamePost(text);
				list.Add(item);
			}
		}
		return list;
	}

	public List<GamePost> GetReplayList()
	{
		List<GamePost> list = new List<GamePost>();
		string[] files = Directory.GetFiles(Application.persistentDataPath);
		string[] array = files;
		foreach (string text in array)
		{
			if (System.IO.Path.GetExtension(text) == ".rep")
			{
				GamePost item = LoadGamePost(text);
				list.Add(item);
			}
		}
		return list;
	}

	private GamePost LoadGamePost(string fileName)
	{
		FileStream fileStream = new FileStream(fileName, FileMode.Open);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		int count = binaryReader.ReadInt32();
		byte[] data = binaryReader.ReadBytes(count);
		GamePost gamePost = new GamePost();
		gamePost.FromArray(data);
		fileStream.Close();
		return gamePost;
	}

	private string GetGameFileName(int gameID)
	{
		return Application.persistentDataPath + "/game" + gameID + ".gam";
	}

	private string GetReplayFileName(int gameID)
	{
		return Application.persistentDataPath + "/replay_" + gameID + ".rep";
	}

	public byte[] LoadGame(int gameID, bool replay)
	{
		string path = ((!replay) ? GetGameFileName(gameID) : GetReplayFileName(gameID));
		FileStream fileStream = new FileStream(path, FileMode.Open);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		int count = binaryReader.ReadInt32();
		byte[] array = binaryReader.ReadBytes(count);
		int count2 = binaryReader.ReadInt32();
		byte[] result = binaryReader.ReadBytes(count2);
		fileStream.Close();
		return result;
	}

	public bool SaveGame(GamePost post, byte[] gameData, bool replay)
	{
		string text;
		if (replay)
		{
			text = GetReplayFileName(post.m_gameID);
			if (File.Exists(text))
			{
				PLog.LogError("File exist " + text);
				return false;
			}
		}
		else
		{
			text = GetGameFileName(post.m_gameID);
		}
		FileStream fileStream = new FileStream(text, FileMode.Create);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		byte[] array = post.ToArray();
		binaryWriter.Write(array.Length);
		binaryWriter.Write(array);
		binaryWriter.Write(gameData.Length);
		binaryWriter.Write(gameData);
		fileStream.Close();
		return true;
	}

	public void RemoveGame(int gameID)
	{
		string gameFileName = GetGameFileName(gameID);
		try
		{
			File.Delete(gameFileName);
		}
		catch
		{
			PLog.LogError("Failed to remove offline game " + gameID);
		}
	}

	public void RemoveReplay(int gameID)
	{
		string replayFileName = GetReplayFileName(gameID);
		try
		{
			File.Delete(replayFileName);
		}
		catch
		{
			PLog.LogError("Failed to remove offline replay " + gameID);
		}
	}
}
