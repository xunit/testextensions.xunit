using System.Collections.Generic;
using Microsoft.ExtendedReflection.Asserts;
using Microsoft.ExtendedReflection.Metadata;
using Microsoft.ExtendedReflection.Utilities.Safe.Diagnostics;

namespace IntelliTest.Xunit
{
    sealed class XunitAssertMethodFilter : IAssertMethodFilter
    {
        // This is a mapping of assertion method to the most common parameter count
        static readonly Dictionary<string, int> parameterCount = new Dictionary<string, int>
        {
            // BooleanAsserts.cs
            { "False", 1 },
            { "True", 1 },
            // CollectionAssert.cs
            { "All", 2 },
            { "Contains", 2 },
            { "DoesNotContain", 2 },
            { "Empty", 1 },
            { "NotEmpty", 1 },
            { "Single", 1 },
            // EqualityAsserts.cs
            { "Equal", 2 },
            { "NotEqual", 2 },
            { "NotStrictEqual", 2 },
            { "StrictEqual", 2 },
            // ExceptionAsserts.cs
            { "Throws", 1 },
            { "ThrowsAny", 1 },
            { "ThrowsAnyAsync", 1 },
            { "ThrowsAsync", 1 },
            // IdentityAsserts.cs
            { "NotSame", 2 },
            { "Same", 2 },
            // NullAsserts.cs
            { "NotNull", 1 },
            { "Null", 1 },
            // PropertyAsserts.cs
            { "PropertyChanged", 3 },
            // RangeAsserts.cs
            { "InRange", 3 },
            { "NotInRange", 3 },
            // SetAsserts.cs
            { "ProperSubset", 2 },
            { "ProperSuperset", 2 },
            { "Subset", 2 },
            { "Superset", 2 },
            // StringAsserts.cs
            { "DoesNotMatch", 2 },
            { "EndsWith", 2 },
            { "Matches", 2 },
            { "StartsWith", 2 },
            // TypeAsserts.cs
            { "IsAssignableFrom", 1 },
            { "IsNotType", 1 },
            { "IsType", 1 },
        };

        public static readonly XunitAssertMethodFilter Instance = new XunitAssertMethodFilter();

        XunitAssertMethodFilter() { }

        public bool IsAssertMethod(MethodDefinition method, out int usefulParameters)
        {
            SafeDebug.AssumeNotNull(method, "method");

            TypeDefinition type;
            if (method.TryGetDeclaringType(out type) && type.SerializableName == XunitTestFrameworkMetadata.Type_Assert.Definition)
                if (parameterCount.TryGetValue(method.ShortName, out usefulParameters))
                    return true;

            usefulParameters = -1;
            return false;
        }
    }
}
