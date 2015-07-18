using Microsoft.VisualStudio.TestPlatform.TestGeneration.Data;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Model;

namespace CreateUnitTest.Xunit
{
    public class XunitUnitTestClassManager : UnitTestClassManagerBase
    {
        public XunitUnitTestClassManager(IConfigurationSettings configurationSettings, INaming naming)
            : base(configurationSettings, naming)
        { }

        public override string AssertionFailure => @"Assert.True(false, ""This test needs an implementation"")";

        public override string TestClassAttribute => string.Empty;

        public override string TestMethodAttribute => "Fact";
    }
}
