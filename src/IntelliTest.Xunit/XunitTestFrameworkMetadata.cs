using System;
using Microsoft.ExtendedReflection.Metadata;
using Microsoft.ExtendedReflection.Metadata.Builders;
using Microsoft.ExtendedReflection.Metadata.Names;
using Microsoft.ExtendedReflection.Utilities.Safe;
using Microsoft.ExtendedReflection.Utilities.Safe.Diagnostics;
using Microsoft.Pex.Framework;

namespace IntelliTest.Xunit
{
    static class XunitTestFrameworkMetadata
    {
        public const string Namespace_Xunit = "Xunit";
        public const string Namespace_XunitSdk = "Xunit.Sdk";

        public static readonly ShortAssemblyName Assembly_XunitAssert;
        public static readonly ShortAssemblyName Assembly_XunitCore;
        public static readonly MethodName Ctor_FactAttribute;
        public static readonly MethodName Ctor_TraitAttribute;
        public static readonly Lazy<Method> Method_PexAssertInconclusive;
        public static readonly MethodDefinitionName MethodDefinition_AssertEqual;
        public static readonly MethodDefinitionName MethodDefinition_AssertNotEqual;
        public static readonly MethodDefinitionName MethodDefinition_AssertNotNull;
        public static readonly MethodDefinitionName MethodDefinition_AssertNull;
        public static readonly MethodDefinitionName MethodDefinition_AssertTrue;
        public static readonly PropertyName Property_Skip;
        public static readonly TypeName Type_Assert;
        public static readonly TypeName Type_AssertException;
        public static readonly TypeName Type_FactAttribute;
        public static readonly TypeName Type_TheoryAttribute;
        public static readonly TypeName Type_TraitAttribute;

        static XunitTestFrameworkMetadata()
        {
            Assembly_XunitAssert = ShortAssemblyName.FromName("xunit.assert");
            Assembly_XunitCore = ShortAssemblyName.FromName("xunit.core");

            Type_Assert = MakeTypeName("Assert");
            Type_AssertException = MakeTypeName("XunitException", Namespace_XunitSdk, Assembly_XunitAssert);
            Type_FactAttribute = MakeTypeName("FactAttribute");
            Type_TheoryAttribute = MakeTypeName("TheoryAttribute");
            Type_TraitAttribute = MakeTypeName("TraitAttribute");

            Ctor_FactAttribute = MetadataBuilderHelper.Attributes.ConstructorName(Type_FactAttribute.Definition);
            Ctor_TraitAttribute = MetadataBuilderHelper.Attributes.ConstructorName(Type_TraitAttribute.Definition);

            Method_PexAssertInconclusive = new Lazy<Method>(() => MakePexAssertInconclusive());

            MethodDefinition_AssertEqual = MakeAssertEqual();
            MethodDefinition_AssertNotEqual = MakeAssertNotEqual();
            MethodDefinition_AssertNotNull = MakeAssertNotNull();
            MethodDefinition_AssertNull = MakeAssertNull();
            MethodDefinition_AssertTrue = MakeAssertTrue();

            Property_Skip = new PropertyDefinitionName(Assembly_XunitCore, -1, Type_FactAttribute.Definition, "Skip", SystemTypes.String.SerializableName).SelfInstantiation;
        }

        static MethodDefinitionName MakeAssertEqual()
        {
            var firstGenericParameter = TypeName.MakeGenericMethodParameter(0);
            return MethodDefinitionName.FromTypeMethod(
                0,
                Type_Assert.Definition,
                true,
                "Equal",
                new string[] { "T" },
                SystemTypes.Void.SerializableName,
                new ParameterDefinitionName(firstGenericParameter, "expected", 0, false, ParameterDirection.ByValueOrRef),
                new ParameterDefinitionName(firstGenericParameter, "actual", 1, false, ParameterDirection.ByValueOrRef)
            );
        }

        static MethodDefinitionName MakeAssertNotEqual()
        {
            var firstGenericParameter = TypeName.MakeGenericMethodParameter(0);
            return MethodDefinitionName.FromTypeMethod(
                0,
                Type_Assert.Definition,
                true,
                "Equal",
                new string[] { "T" },
                SystemTypes.Void.SerializableName,
                new ParameterDefinitionName(firstGenericParameter, "expected", 0, false, ParameterDirection.ByValueOrRef),
                new ParameterDefinitionName(firstGenericParameter, "actual", 1, false, ParameterDirection.ByValueOrRef)
            );
        }

        static MethodDefinitionName MakeAssertNotNull()
        {
            return MethodDefinitionName.FromTypeMethod(
                0,
                Type_Assert.Definition,
                true,
                "NotNull",
                SafeArray.Empty<string>(),
                SystemTypes.Void.SerializableName,
                new ParameterDefinitionName(SystemTypes.Object.SerializableName, "condition", 0, false, ParameterDirection.ByValueOrRef)
            );
        }

        static MethodDefinitionName MakeAssertNull()
        {
            return MethodDefinitionName.FromTypeMethod(
                0,
                Type_Assert.Definition,
                true,
                "Null",
                SafeArray.Empty<string>(),
                SystemTypes.Void.SerializableName,
                new ParameterDefinitionName(SystemTypes.Object.SerializableName, "object", 0, false, ParameterDirection.ByValueOrRef)
            );
        }

        static MethodDefinitionName MakeAssertTrue()
        {
            return MethodDefinitionName.FromTypeMethod(
                0,
                Type_Assert.Definition,
                true,
                "True",
                SafeArray.Empty<string>(),
                SystemTypes.Void.SerializableName,
                new ParameterDefinitionName(SystemTypes.Bool.SerializableName, "condition", 0, false, ParameterDirection.ByValueOrRef)
            );
        }

        static Method MakePexAssertInconclusive()
        {
            return MetadataFromReflection.GetType(typeof(PexAssert)).GetMethod("Inconclusive", SystemTypes.String);
        }

        static TypeName MakeTypeName(string name, string @namespace = null, ShortAssemblyName assembly = null)
        {
            SafeDebug.AssumeNotNullOrEmpty(name, "name");

            return TypeDefinitionName.FromName(assembly ?? Assembly_XunitCore, -1, false, @namespace ?? Namespace_Xunit, name).SelfInstantiation;
        }
    }
}
