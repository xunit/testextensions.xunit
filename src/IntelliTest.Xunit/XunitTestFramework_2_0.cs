using System;
using Microsoft.Pex.Engine.ComponentModel;

namespace IntelliTest.Xunit
{
    [Serializable]
    sealed class XunitTestFramework_2_0 : XunitTestFramework
    {
        public XunitTestFramework_2_0(IPexComponent host)
            : base(host,
                   name: "xunit-2.0",
                   prettyName: "xUnit.net 2.0",
                   xunitPackageVersion: "2.0.0",
                   visualStudioRunnerPackageVersion: "2.0.1")
        { }
    }
}
