using System.Threading.Tasks;

namespace GenderPayGap.Core.Interfaces
{
    public interface IQueue
    {

        string Name { get; }

        Task AddMessageAsync<TInstance>(TInstance instance);

        Task AddMessageAsync(string message);

    }
}
