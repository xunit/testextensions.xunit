using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Data;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Model;

namespace CreateUnitTest.Xunit
{
    [Export(typeof(IFrameworkProvider))]
    public class XunitFrameworkProvider_2_2 : XunitFrameworkProvider
    {
        [ImportingConstructor]
        public XunitFrameworkProvider_2_2(IServiceProvider serviceProvider, IConfigurationSettings configurationSettings, INaming naming, IDirectory directory)
            : base(serviceProvider, configurationSettings, naming, directory,
                   displayName: "xUnit.net 2.2",
                   xunitPackageVersion: "2.2.0",
                   xunitCoreAssemblyVersionPrefix: "2.2.0.",
                   visualStudioRunnerPackageVersion: "2.2.0")
        { }
    }
}
