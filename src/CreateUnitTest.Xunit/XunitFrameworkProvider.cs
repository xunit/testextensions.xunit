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
        readonly string xunitCoreAssemblyVersionPrefix;

        public XunitFrameworkProvider(IServiceProvider serviceProvider,
                                      IConfigurationSettings configurationSettings,
                                      INaming naming,
                                      IDirectory directory,
                                      string displayName,
                                      string xunitPackageVersion,
                                      string xunitCoreAssemblyVersionPrefix,
                                      string visualStudioRunnerPackageVersion)
            : base(new XunitSolutionManager(serviceProvider, naming, directory, xunitPackageVersion, visualStudioRunnerPackageVersion),
                   new XunitUnitTestProjectManager(serviceProvider, naming),
                   new XunitUnitTestClassManager(configurationSettings, naming))
        {
            this.displayName = displayName;
            this.xunitCoreAssemblyVersionPrefix = xunitCoreAssemblyVersionPrefix;
        }

        public override string AssemblyName => "xunit.core";

        public override string Name => displayName;

        public override bool IsTestProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            var vsProject2 = project.Object as VSProject2;
            if (vsProject2 != null && (project.Kind == ProjectTypes.CSharp || project.Kind == ProjectTypes.VisualBasic))
                return vsProject2.References.AsEnumerable().Any(r => r.Name == AssemblyName && r.Version.StartsWith(xunitCoreAssemblyVersionPrefix));

            return false;
        }
    }
}
