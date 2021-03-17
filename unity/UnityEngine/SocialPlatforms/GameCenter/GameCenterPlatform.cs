using System;

namespace UnityEngine.SocialPlatforms.GameCenter
{
	public class GameCenterPlatform : Local
	{
		public static void ResetAllAchievements(Action<bool> callback)
		{
			Debug.Log("ResetAllAchievements - no effect in editor");
		}

		public static void ShowDefaultAchievementCompletionBanner(bool value)
		{
			Debug.Log("ShowDefaultAchievementCompletionBanner - no effect in editor");
		}
	}
}
