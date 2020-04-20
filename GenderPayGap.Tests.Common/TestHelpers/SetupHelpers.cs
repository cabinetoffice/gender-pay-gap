using Autofac;
using GenderPayGap.BusinessLogic.LogRecords;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using Moq;

namespace GenderPayGap.Tests.Common.TestHelpers
{

    public static class SetupHelpers
    {

        //TODO: Possible seperate this method for WebUI and Webjobs?
        public static void SetupMockLogRecordGlobals(ContainerBuilder builder = null)
        {
            // Used by WebUI
            Global.ManualChangeLog = new Mock<ILogRecordLogger>().Object;
            Global.RegistrationLog = new Mock<ILogRecordLogger>().Object;
            Global.SearchLog = new Mock<ILogRecordLogger>().Object;

            if (builder != null)
            {
                builder.RegisterInstance(Global.ManualChangeLog).Keyed<ILogRecordLogger>(Filenames.ManualChangeLog).SingleInstance();
                builder.RegisterInstance(Global.RegistrationLog).Keyed<ILogRecordLogger>(Filenames.RegistrationLog).SingleInstance();
                builder.RegisterInstance(Global.SearchLog).Keyed<ILogRecordLogger>(Filenames.SearchLog).SingleInstance();

                builder.RegisterInstance(Mock.Of<IRegistrationLogRecord>()).SingleInstance();
            }
        }

    }

}
