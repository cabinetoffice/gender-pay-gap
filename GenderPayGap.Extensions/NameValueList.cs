using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GenderPayGap.Extensions
{
    [Serializable]
    public class NameValueList : List<NameValueElement>
    {

        public string this[string name]
        {
            get
            {
                NameValueElement item = this.FirstOrDefault(nve => nve.Name.EqualsI(name));
                return item == null ? null : item.Value;
            }
            set
            {
                NameValueElement item = this.FirstOrDefault(nve => nve.Name.EqualsI(name));
                if (item == null)
                {
                    item = new NameValueElement(name, value);
                }
                else
                {
                    item.Value = value;
                }
            }
        }

        public int Count(string name = null, params string[] excludeNames)
        {
            var c = 0;
            foreach (NameValueElement item in this)
            {
                if (!string.IsNullOrWhiteSpace(name) && !item.Name.EqualsI(name))
                {
                    continue;
                }

                if (excludeNames != null && excludeNames.Length > 0 && excludeNames.ContainsI(item.Name))
                {
                    continue;
                }

                c++;
            }

            return c;
        }

        public NameValueList Copy()
        {
            var copy = new NameValueList();
            copy.AddRange(this.Select(item => new NameValueElement(item.Name, item.Value)));
            return copy;
        }

        public string ToQueryString()
        {
            var data = "";
            foreach (NameValueElement item in this)
            {
                if (string.IsNullOrWhiteSpace(item.Value))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(data))
                {
                    data += "&";
                }

                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    data += WebUtility.UrlEncode(item.Value);
                }
                else
                {
                    data += WebUtility.UrlEncode(item.Name) + "=" + WebUtility.UrlEncode(item.Value);
                }
            }

            return data;
        }

        public bool Equals(NameValueList target)
        {
            if (target == null || base.Count != target.Count())
            {
                return false;
            }

            return target.All(item => this[item.Name] == item.Value);
        }

    }
}
