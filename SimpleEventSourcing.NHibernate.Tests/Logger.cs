using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace SimpleEventSourcing.NHibernate.Tests
{
    public static class Logger
    {
        private static Hierarchy hierarchy;

        public static void Setup()
        {
            if (hierarchy != null)
            {
                return;
            }

            hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
            };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                LockingModel = new FileAppender.MinimalLock(),
                AppendToFile = false,
                File = @"C:\temp\EventLog.txt",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            var memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Trace;
            hierarchy.Configured = true;
        }
    }
}
