using System;

namespace UnityEngine.SocialPlatforms.Impl
{
	public class LocalUser : UserProfile, ILocalUser, IUserProfile
	{
		private IUserProfile[] m_Friends;

		private bool m_Authenticated;

		private bool m_Underage;

		public IUserProfile[] friends => m_Friends;

		public bool authenticated => m_Authenticated;

		public bool underage => m_Underage;

		public LocalUser()
		{
			m_Friends = new UserProfile[0];
			m_Authenticated = false;
			m_Underage = false;
		}

		public void Authenticate(Action<bool> callback)
		{
			ActivePlatform.Instance.Authenticate(this, callback);
		}

		public void LoadFriends(Action<bool> callback)
		{
			ActivePlatform.Instance.LoadFriends(this, callback);
		}

		public void SetFriends(IUserProfile[] friends)
		{
			m_Friends = friends;
		}

		public void SetAuthenticated(bool value)
		{
			m_Authenticated = value;
		}

		public void SetUnderage(bool value)
		{
			m_Underage = value;
		}
	}
}
