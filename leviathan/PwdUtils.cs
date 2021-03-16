using System.Linq;
using System.Security.Cryptography;
using System.Text;

internal class PwdUtils
{
	private const int keySize = 60;

	private const int saltSize = 20;

	public static void GeneratePasswordHash(string password, out byte[] key, out byte[] salt)
	{
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, 20);
		key = rfc2898DeriveBytes.GetBytes(60);
		salt = rfc2898DeriveBytes.Salt;
	}

	public static bool CheckPassword(string password, byte[] key, byte[] salt)
	{
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
		byte[] bytes = rfc2898DeriveBytes.GetBytes(key.Length);
		return bytes.SequenceEqual(key);
	}

	public static string GenerateWeakPasswordHash(string password)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes("sdfjk23409fmspep");
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, bytes);
		byte[] bytes2 = rfc2898DeriveBytes.GetBytes(60);
		return uTF8Encoding.GetString(bytes2);
	}
}
