using System;
using System.IO;
using System.Security.Cryptography;

public static class AESHelper
{
    private static string AES_KEY = "&a1`";
    private static bool USE_ENCRYPTION = true;
    // Use this for initialization
    /// <summary>  
    /// AES加密(无向量)  
    /// </summary>  
    /// <param name="plainBytes">被加密的明文</param>  
    /// <param name="key">密钥</param>  
    /// <returns>密文</returns>  
    /// 
    private static Byte[] ENKEY = new Byte[32];
    public static string AESEncrypt(string Data, string key = "")
    {
        if (!USE_ENCRYPTION) return Data;
        MemoryStream mStream = new MemoryStream();
        RijndaelManaged aes = new RijndaelManaged();

        byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(Data);
        if (string.IsNullOrEmpty(key))
        {
            key = AES_KEY;
        }
        Array.Copy(System.Text.Encoding.UTF8.GetBytes(key.PadRight(ENKEY.Length)), ENKEY, ENKEY.Length);
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 128;
        //aes.Key = _key;  
        aes.Key = ENKEY;
        //aes.IV = _iV;  
        CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        try
        {
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(mStream.ToArray());
        }
        finally
        {
            cryptoStream.Close();
            mStream.Close();
            aes.Clear();
        }
    }

    /// <summary>  
    /// AES解密(无向量)  
    /// </summary>  
    /// <param name="encryptedBytes">被加密的明文</param>  
    /// <param name="key">密钥</param>  
    /// <returns>明文</returns>  
    private static Byte[] DeKey = new Byte[32];
    public static string AESDecrypt(string Data, string key = "")
    {
        if (!USE_ENCRYPTION) return Data;
        if (string.IsNullOrEmpty(Data)) return "";
        Byte[] encryptedBytes = Convert.FromBase64String(Data);
        if (string.IsNullOrEmpty(key))
        {
            key = AES_KEY;
        }
        Array.Copy(System.Text.Encoding.UTF8.GetBytes(key.PadRight(DeKey.Length)), DeKey, DeKey.Length);
        MemoryStream mStream = new MemoryStream(encryptedBytes);
        //mStream.Write( encryptedBytes, 0, encryptedBytes.Length );  
        //mStream.Seek( 0, SeekOrigin.Begin );  
        RijndaelManaged aes = new RijndaelManaged();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 128;
        aes.Key = DeKey;
        //aes.IV = _iV;  
        CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        try
        {
            byte[] tmp = new byte[encryptedBytes.Length + 32];
            int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
            byte[] ret = new byte[len];
            Array.Copy(tmp, 0, ret, 0, len);
            return System.Text.Encoding.UTF8.GetString(ret);
        }
        finally
        {
            cryptoStream.Close();
            mStream.Close();
            aes.Clear();
        }
    }
}
