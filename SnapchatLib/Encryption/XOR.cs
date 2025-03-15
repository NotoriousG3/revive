using System.Text;

namespace SnapchatLib.Encryption
{
    internal class XOR
    {
        public static string Encrypt(string szPlainText, int szEncryptionKey)
        {
            StringBuilder szInputStringBuild = new StringBuilder(szPlainText);
            StringBuilder szOutStringBuild = new StringBuilder(szPlainText.Length);
            char Textch;
            for (int iCount = 0; iCount < szPlainText.Length; iCount++)
            {
                Textch = szInputStringBuild[iCount];
                Textch = (char)(Textch ^ szEncryptionKey);
                szOutStringBuild.Append(Textch);
            }
            return Base64.Base64Encode(szOutStringBuild.ToString());
        }
        public static string Decrypt(string szPlainText, int szEncryptionKey)
        {
            var decoded_string = Base64.Base64Decode(szPlainText);
            StringBuilder szInputStringBuild = new StringBuilder(decoded_string);
            StringBuilder szOutStringBuild = new StringBuilder(decoded_string.Length);
            char Textch;
            for (int iCount = 0; iCount < decoded_string.Length; iCount++)
            {
                Textch = szInputStringBuild[iCount];
                Textch = (char)(Textch ^ szEncryptionKey);
                szOutStringBuild.Append(Textch);
            }
            return szOutStringBuild.ToString();
        }
    }
}
