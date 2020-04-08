using System;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.Core.Abstractions
{

    public abstract class AEmailProvider
    {

        public AEmailProvider(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual bool Enabled { get; } = true;

        public abstract Task<SendEmailResult> SendEmailAsync<TModel>(string emailAddress, string templateId, TModel parameters, bool test);

        public ILogger Logger { get; }
        
    }

}
