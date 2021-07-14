using System;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Data;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Model;
using VSLangProj80;

namespace CreateUnitTest.Xunit
{
    public abstract class XunitFrameworkProvider : FrameworkProviderBase
    {
        readonly string displayName;

        public XunitFrameworkProvider(IServiceProvider serviceProvider,
                                      IConfigurationSettings configurationSettings,
                                      INaming naming,
                                      IDirectory directory,
                                      string displayName,
                                      string xunitPackageVersion,
                                      string visualStudioRunnerPackageVersion)
            : base(new XunitSolutionManager(serviceProvider, naming, directory, xunitPackageVersion, visualStudioRunnerPackageVersion),
                   new XunitUnitTestProjectManager(serviceProvider, naming),
                   new XunitUnitTestClassManager(configurationSettings, naming))
        {
            this.displayName = displayName;
        }

        public override string AssemblyName => "xunit.core";

        public override string Name => displayName;
    }
}
