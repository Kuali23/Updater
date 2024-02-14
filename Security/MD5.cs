using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Security
{
    public static class MD5
    {
        public static string MD5Encrypt(string toEncrypt)
        {
            if (toEncrypt is null) return "";
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(toEncrypt);

            byte[] resuldt = md5.ComputeHash(bs);
            StringBuilder sb = new();
            for (int i = 0; i<resuldt.Length; i++)
            {
                sb.Append(resuldt[i].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }
    }
}
