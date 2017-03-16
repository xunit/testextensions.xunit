using Microsoft.ExtendedReflection.ComponentModel;
using Microsoft.Pex.Engine.ComponentModel;
using Microsoft.Pex.Engine.TestFrameworks;
using Microsoft.Pex.Framework.Packages;

namespace IntelliTest.Xunit
{
    public class XunitTestFrameworkPackageAttribute : PexPackageAttributeBase
    {
        public override string Name => "XunitTestFrameworkPackage";

        protected override void Initialize(IEngine engine)
        {
            base.Initialize(engine);

            var testFrameworkService = engine.GetService<IPexTestFrameworkManager>();
            var host = testFrameworkService as IPexComponent;

            testFrameworkService.AddTestFramework(new XunitTestFramework_2_0(host));
			testFrameworkService.AddTestFramework(new XunitTestFramework_2_1(host));
		}
    }
}
