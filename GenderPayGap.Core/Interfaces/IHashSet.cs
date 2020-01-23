using System.Collections.Generic;

namespace GenderPayGap.Core.Interfaces
{
    public interface IHashSet<T>
    {

        int Count { get; }
        void Add(params T[] items);
        void Remove(params T[] items);
        bool Contains(params T[] items);
        void Clear();

        IEnumerable<T> AsEnumerable();
        IList<T> ToList();

    }
}
