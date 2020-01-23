using System;
using System.Collections.Generic;

namespace GenderPayGap.Core.Classes
{
    [Serializable]
    public class PagedResult<T>
    {

        public List<T> Results { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageCount => ActualRecordTotal <= 0 || PageSize <= 0 ? 0 : (int) Math.Ceiling((double) ActualRecordTotal / PageSize);
        public int PageSize { get; set; }
        public long VirtualRecordTotal { get; set; }
        public long ActualRecordTotal { get; set; }

    }
}
