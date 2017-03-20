using System;
using Microsoft.Pex.Engine.ComponentModel;

namespace IntelliTest.Xunit
{
    [Serializable]
    sealed class XunitTestFramework_2_2 : XunitTestFramework
    {
        public XunitTestFramework_2_2(IPexComponent host)
            : base(host,
                   name: "xunit-2.2",
                   prettyName: "xUnit.net 2.2",
                   xunitPackageVersion: "2.2.0",
                   visualStudioRunnerPackageVersion: "2.2.0")
        { }
    }
}
