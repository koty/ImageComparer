using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ImageComparer.Models
{
    internal class Md5Util
    {
        public static string getMd5Hash(string fileName)
        {
            //ファイルを開く
            using (var fs = new System.IO.FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read))
            {

                //MD5CryptoServiceProviderオブジェクトを作成
                var md5 = new MD5CryptoServiceProvider();
                //ハッシュ値を計算する
                var bs = md5.ComputeHash(fs);
                //リソースを解放する
                md5.Clear();
                //ファイルを閉じる
                //byte型配列を16進数の文字列に変換
                var result = new StringBuilder();
                foreach (byte b in bs)
                {
                    result.Append(b.ToString("x2"));
                }

                //結果を表示
                return result.ToString();
            }
        }
    }
}
