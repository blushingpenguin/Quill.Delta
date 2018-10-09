using System.Text.RegularExpressions;

namespace Quill.Delta
{
    public static class UrlHelpers
    {
        static readonly Regex s_leadingSpaceRe = new Regex("^\\s*", RegexOptions.Compiled);
        static readonly Regex s_whiteListRe = new Regex("^\\s*((|https?|s?ftp|file|blob|mailto|tel):|#|\\/|data:image\\/)", RegexOptions.Compiled);
        
        public static string Sanitize(string str)
        {
            var val = s_leadingSpaceRe.Replace(str, "");
            if (s_whiteListRe.IsMatch(val))
            {
                return val;
            }
            return $"unsafe:{val}";
        }
    }
}
