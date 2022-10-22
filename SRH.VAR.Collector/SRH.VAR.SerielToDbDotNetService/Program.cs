using log4net.Config;
using System;
using Topshelf;

namespace SRH.VAR.SerielToDbDotNetService
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var rc = HostFactory.Run(hostConfigurator => 
            {
                hostConfigurator.SetDescription("Seriel to Database writer for GVV");
                hostConfigurator.SetDisplayName("SRH.VAR.Service");
                hostConfigurator.SetServiceName("SRH.VAR.Service");

                hostConfigurator.Service<GvvLogger>(s => 
                {
                    s.ConstructUsing(name => new GvvLogger());
                    s.WhenStarted(tc => tc.Start()); 
                    s.WhenStopped(tc => tc.Stop());
                });
                hostConfigurator.RunAsLocalSystem();
                hostConfigurator.UseLog4Net();

            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}