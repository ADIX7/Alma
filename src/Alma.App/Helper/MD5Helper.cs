using System.Security.Cryptography;
using System.Text;

namespace Alma.Helper;

public static class MD5Helper
{
    public static string GetMD5Hash(string source)
    {
        using var md5Hasher = MD5.Create();
        var data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(source));
        var sBuilder = new StringBuilder();
        for (var i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
}