using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FT.Data
{
    public static class Util
    {
        public static string GetFolketsTingUrl(string path)
        {
            return "http://folketsting.dk/" + path;
        }

        public static string ToUrlFriendly(this string s)
        {
            // make it all lower case
            string title = s.ToLower();
            // replace ae, oe aa
            title = title.Replace("æ", "ae").Replace("å", "aa").Replace("ø", "oe");
            // remove entities
            title = Regex.Replace(title, @"&\w+;", "");
            // remove anything that is not letters, numbers, dash, or space
            title = Regex.Replace(title, @"[^a-z0-9\-\s]", "");
            // replace spaces
            title = title.Replace(' ', '-');
            // collapse dashes
            title = Regex.Replace(title, @"-{2,}", "-");
            // trim excessive dashes at the beginning
            title = title.TrimStart(new[] { '-' });
            // if it's too long, clip it
            if (title.Length > 80)
                title = title.Substring(0, 79);
            // remove trailing dashes
            title = title.TrimEnd(new[] { '-' });
            return title;
        }

    }
}
