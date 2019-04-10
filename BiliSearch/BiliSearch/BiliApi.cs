using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BiliSearch
{
    class BiliApi
    {
        public const string APP_KEY = "iVGUTjsxvpLeuDCf";
        public const string SECRET_KEY = "aHRmhWMLkdeMuILqORnYZocwMBpMEOdt";
        public const string BUILD = "8430";

        public static string DicToParams(Dictionary<string, string> dic, bool addVerification)
        {
            if (addVerification)
                dic = AddVerification(dic);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> item in dic.OrderBy(i => i.Key))
            {
                stringBuilder.Append("&");
                stringBuilder.Append(item.Key);
                stringBuilder.Append("=");
                stringBuilder.Append(item.Value);
            }
            return stringBuilder.ToString().Substring(1);
        }

        public static Dictionary<string, string> AddVerification(Dictionary<string, string> dic)
        {
            string baseParams = DicToParams(dic, false);
            string sign = CreateMD5Hash(baseParams + SECRET_KEY);
            dic.Add("appkey", APP_KEY);
            dic.Add("build", BUILD);
            dic.Add("sign", sign);
            return dic;
        }

        private static string CreateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
