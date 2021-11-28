using System;
using System.Security.Cryptography;
using System.Text;

namespace vrhero.VirtualHere
{
    public partial class Utils
    {
        public static string HashMd5(string input)
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            var md5 = MD5.Create();
            md5.TransformFinalBlock(buffer, 0, buffer.Length);
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
