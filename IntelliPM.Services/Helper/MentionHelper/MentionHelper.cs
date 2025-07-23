using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntelliPM.Services.Helper.MentionHelper
{
    internal class MentionHelper
    {
        public static List<int> ExtractMentionedAccountIds(string content)
        {
            var mentionedIds = new List<int>();
            var matches = Regex.Matches(content ?? "", @"@(\d+)");
            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out int id))
                {
                    mentionedIds.Add(id);
                }
            }
            return mentionedIds.Distinct().ToList();
        }
    }
}
