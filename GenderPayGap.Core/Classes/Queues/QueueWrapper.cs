using Newtonsoft.Json;

namespace GenderPayGap.Core.Classes
{
    public class QueueWrapper
    {

        public QueueWrapper(object message)
        {
            Message = JsonConvert.SerializeObject(message);
            Type = message.GetType().ToString();
        }

        public string Type { get; set; }

        public string Message { get; set; }

    }
}
