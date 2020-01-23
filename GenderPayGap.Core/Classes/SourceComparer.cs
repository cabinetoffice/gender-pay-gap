using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes
{
    /// <summary>
    ///     Compares two data source types to see if one can replace the other
    /// </summary>
    public class SourceComparer
    {

        public static bool CanReplace(string source, string target)
        {
            return Parse(source) >= Parse(target);
        }

        public static bool CanReplace(string source, IEnumerable<string> targets)
        {
            foreach (string target in targets)
            {
                if (Parse(source) < Parse(target))
                {
                    return false;
                }
            }

            return true;
        }

        private static int Parse(string source)
        {
            if (source.EqualsI("admin", "administrator") || IsAdministrator(source))
            {
                return 4;
            }

            if (IsCoHo(source))
            {
                return 3;
            }

            if (source.EqualsI("user") || source.IsEmailAddress())
            {
                return 2;
            }

            if (IsDnB(source) || source.EqualsI("Manual"))
            {
                return 1;
            }

            return 0;
        }

        public static bool IsAdministrator(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Global.AdminEmails))
            {
                throw new ArgumentException("Missing AdminEmails from web.config");
            }

            return emailAddress.LikeAny(Global.AdminEmails.SplitI(";"));
        }

        public static bool IsDnB(string source)
        {
            return source.Strip(" ").EqualsI("D&B", "DNB", "dunandbradstreet", "dun&bradstreet");
        }

        public static bool IsCoHo(string source)
        {
            return source.Strip(" ").EqualsI("CoHo", "CompaniesHouse", "CompanyHouse");
        }

    }
}
