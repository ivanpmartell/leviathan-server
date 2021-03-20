using System;
using System.Text;
using System.Security.Cryptography;

namespace leviathan_server
{
    public static class Utils
    {
        public static Guid GenerateGuid(string name)
		{
			using MD5 mD = MD5.Create();
			byte[] b = mD.ComputeHash(Encoding.Default.GetBytes(name));
			return new Guid(b);
		}
    }

    internal enum Type
		{
			Int,
			Bool,
			Float,
			Double,
			String,
			ByteArray,
			FloatArray,
			DoubleArray,
			StringIntDic,
			Long,
			StringArray,
			IntArray
		}

		internal enum ErrorCode
		{
			WrongUserPassword,
			UserNotVerified,
			InvalidVerificationToken,
			VersionMissmatch,
			AccountExist,
			ServerFull,
			FriendUserDoesNotExist,
			AlreadyFriend,
			UserAlreadyVerified,
			AccountDoesNotExist,
			NoError
		}

		internal enum PlatformType
		{
			None,
			WindowsPC,
			Ios,
			Android,
			Osx,
			Other
		}
}