using System;
using EnvDTE;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Model;

namespace CreateUnitTest.Xunit
{
    public class XunitUnitTestProjectManager : UnitTestProjectManagerBase
    {
        public XunitUnitTestProjectManager(IServiceProvider serviceProvider, INaming naming)
            : base(serviceProvider, naming)
        { }

        public override string FrameworkNamespace(Project sourceProject)
        {
            return "Xunit";
        }
    }
}
