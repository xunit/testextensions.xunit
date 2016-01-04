using System;
using Microsoft.Pex.Engine.ComponentModel;

namespace IntelliTest.Xunit
{
    [Serializable]
    sealed class XunitTestFramework_2_1 : XunitTestFramework
    {
        public XunitTestFramework_2_1(IPexComponent host)
            : base(host,
                   name: "xunit-2.1",
                   prettyName: "xUnit.net 2.1",
                   xunitPackageVersion: "2.1.0",
                   visualStudioRunnerPackageVersion: "2.1.0")
        { }
    }
}
