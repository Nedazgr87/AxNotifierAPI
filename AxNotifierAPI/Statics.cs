using System.Security.Cryptography;
using System.Text;

namespace AxNotifierAPI
{
    public static class Statics
    {
        public static string ConnectionString = @"Data Source=65.108.14.168;Database=AxTraderDb;User Id=sa;Password=Amir1993;MultipleActiveResultSets=true";

        private static string CreateMd5(string input)
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            MD5 md5 = new MD5CryptoServiceProvider();
            var result = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var t in result)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }


        public static bool CheckKey(string key)
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var md5 = CreateMd5(date);
            md5 = md5.Replace("0", "");
            return string.Equals(key, md5, StringComparison.CurrentCultureIgnoreCase);
        }

    }
}
