using System;
using System.Threading.Tasks;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.WebUI;
using Microsoft.Extensions.Logging;

namespace GenderPayGap
{

    public static class Emails
    {

        /// <summary>
        ///     Send a message to GEO distribution list
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public static async Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false)
        {
            try
            {
                await Program.MvcApplication.SendEmailQueue.AddMessageAsync(
                    new QueueWrapper(new SendGeoMessageModel {subject = subject, message = message, test = test}));
                return true;
            }
            catch (Exception ex)
            {
                Program.MvcApplication.Logger.LogError(ex, ex.Message);
            }

            return false;
        }
    }

}
