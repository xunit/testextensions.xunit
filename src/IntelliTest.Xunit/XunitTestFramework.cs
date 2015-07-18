using System;
using System.IO;
using System.Linq;
using Microsoft.ExtendedReflection.Asserts;
using Microsoft.ExtendedReflection.Collections;
using Microsoft.ExtendedReflection.Emit;
using Microsoft.ExtendedReflection.Metadata;
using Microsoft.ExtendedReflection.Metadata.Builders;
using Microsoft.ExtendedReflection.Metadata.Interfaces;
using Microsoft.ExtendedReflection.Metadata.Names;
using Microsoft.ExtendedReflection.Monitoring;
using Microsoft.ExtendedReflection.Utilities.Safe.Diagnostics;
using Microsoft.Pex.Engine;
using Microsoft.Pex.Engine.ComponentModel;
using Microsoft.Pex.Engine.TestFrameworks;

namespace IntelliTest.Xunit
{
    /// <summary>
    /// Base class for test frameworks (v2 and higher). Each supported
    /// test framework should derive from this and provide unique names
    /// and package versions for the xunit package to be installed.
    /// </summary>
    public abstract class XunitTestFramework : TestFrameworkBase
    {
        readonly string name;
        readonly string prettyName;
        readonly string visualStudioRunnerPackageVersion;
        readonly string xunitPackageVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
        /// </summary>
        /// <param name="host">The IntelliTest host</param>
        /// <param name="name">The name of the test framework</param>
        /// <param name="prettyName">The display name of the test framework</param>
        /// <param name="xunitPackageVersion">The package version of xunit to be installed</param>
        /// <param name="visualStudioRunnerPackageVersion">The package version of xunit.runner.visualstudio to be installed</param>
        public XunitTestFramework(IPexComponent host,
                                  string name,
                                  string prettyName,
                                  string xunitPackageVersion,
                                  string visualStudioRunnerPackageVersion)
            : base(host)
        {
            this.name = name;
            this.prettyName = prettyName;
            this.xunitPackageVersion = xunitPackageVersion;
            this.visualStudioRunnerPackageVersion = visualStudioRunnerPackageVersion;
        }

        /// <inheritdoc/>
        public override TypeName AssertionExceptionType
        {
            get { return XunitTestFrameworkMetadata.Type_AssertException; }
        }

        /// <inheritdoc/>
        public override IIndexable<IAssertMethodFilter> AssertMethodFilters
        {
            get { return Indexable.One<IAssertMethodFilter>(XunitAssertMethodFilter.Instance); }
        }

        /// <inheritdoc/>
        public override bool AttributeBased
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool SupportsProjectBitness(Bitness bitness)
        {
            return true;
        }

        /// <inheritdoc/>
        public override string Name
        {
            get { return name; }
        }

        /// <inheritdoc/>
        public override string PrettyName
        {
            get { return prettyName; }
        }

        /// <inheritdoc/>
        public override ICountable<ShortReferenceAssemblyName> References
        {
            get
            {
                var xunit = new ShortReferenceAssemblyName(ShortAssemblyName.FromName("xunit"), xunitPackageVersion, AssemblyReferenceType.NugetReference);
                var xunitRunner = new ShortReferenceAssemblyName(ShortAssemblyName.FromName("xunit.runner.visualstudio"), visualStudioRunnerPackageVersion, AssemblyReferenceType.NugetReference);

                return Indexable.Two(xunit, xunitRunner);
            }
        }

        /// <inheritdoc/>
        public override bool SupportsTestReflection
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool SupportsPartialClasses
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override bool SupportsStaticTestMethods
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public override void EmitInconclusive(MethodBodyBuilder body, string message)
        {
            SafeDebug.AssumeNotNull(body, "body");

            body.Push(message);
            body.CallStatic(XunitTestFrameworkMetadata.Method_PexAssertInconclusive.Value);
            body.Statement();
        }

        /// <inheritdoc/>
        public override bool IsFixture(TypeDefinition target)
        {
            SafeDebug.AssumeNotNull(target, "target");

            return !target.IsInterface
                && !target.IsEnumType
                && !target.IsAbstract
                && target.DeclaredVisibility == Visibility.Public
                && target.IsExported
                && target.GenericTypeParametersCount == 0;
        }

        /// <inheritdoc/>
        public override bool IsFixtureIgnored(TestFrameworkTestSelection testSelection, TypeEx fixtureType, out string ignoreMessage)
        {
            // xUnit.net does not support skipping test classes
            ignoreMessage = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool IsParameterizedTest(TypeDefinition fixtureType, MethodDefinition method)
        {
            SafeDebug.AssumeNotNull(method, "method");

            return AttributeHelper.IsDefined(method, XunitTestFrameworkMetadata.Type_TheoryAttribute);
        }

        /// <inheritdoc/>
        public override bool IsTest(TypeDefinition fixtureType, MethodDefinition method)
        {
            SafeDebug.AssumeNotNull(method, "method");

            return AttributeHelper.IsDefined(method, XunitTestFrameworkMetadata.Type_FactAttribute);
        }

        /// <inheritdoc/>
        public override bool IsTestIgnored(TestFrameworkTestSelection testSelection, MethodDefinition method, out string ignoreMessage)
        {
            SafeDebug.AssumeNotNull(testSelection, "testSelection");
            SafeDebug.AssumeNotNull(method, "method");

            var fact = method.DeclaredAttributes
                             .FirstOrDefault(a => a.Constructor.SerializableName == XunitTestFrameworkMetadata.Ctor_FactAttribute);

            if (fact != null)
            {
                var skip = fact.NamedArguments.FirstOrDefault(na => na.Name == "Skip");
                if (skip != null)
                {
                    ignoreMessage = ((MetadataExpression.StringExpression)skip.Value).Value;
                    return true;
                }
            }

            ignoreMessage = null;
            return false;
        }

        /// <inheritdoc/>
        public override void MarkIgnored(MethodDefinitionBuilder method, string message)
        {
            SafeDebug.AssumeNotNull(method, "method");

            var fact = method.CustomAttributes
                             .Cast<CustomAttributeBuilder>()
                             .FirstOrDefault(a => a.Constructor.SerializableName == XunitTestFrameworkMetadata.Ctor_FactAttribute);

            if (fact != null)
            {
                fact.AddNamedArgument(XunitTestFrameworkMetadata.Property_Skip, MetadataExpression.String(message));
                return;
            }

            SafeDebug.FailUnreachable();
        }

        /// <inheritdoc/>
        public override void MarkTestClass(TypeDefinitionBuilder type) { }

        /// <inheritdoc/>
        public override void MarkTestMethod(PexExplorationBase exploration,
                                            IPexGeneratedTest test,
                                            MethodDefinitionBuilder method)
        {
            SafeDebug.AssumeNotNull(test, "test");
            SafeDebug.AssumeNotNull(method, "method");

            method.CustomAttributes.Add(new CustomAttributeBuilder(XunitTestFrameworkMetadata.Ctor_FactAttribute));
        }

        /// <inheritdoc/>
        public override bool TryGetAssemblyName(out ShortAssemblyName assemblyName)
        {
            assemblyName = XunitTestFrameworkMetadata.Assembly_XunitCore;
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetAssemblySetupTeardownMethods(AssemblyEx assembly, out Method setup, out Method teardown)
        {
            // xUnit.net does not have an assembly setup and teardown
            setup = teardown = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool TryGetAssertAreEqual(IType type, out IMethod method)
        {
            method = InstantiationHelper.Instantiate(XunitTestFrameworkMetadata.MethodDefinition_AssertEqual, Indexable.Empty<IType>(), Indexable.One(type));
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetAssertAreNotEqual(IType type, out IMethod method)
        {
            method = InstantiationHelper.Instantiate(XunitTestFrameworkMetadata.MethodDefinition_AssertNotEqual, Indexable.Empty<IType>(), Indexable.One(type));
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetAssertIsNotNull(out IMethod method)
        {
            method = XunitTestFrameworkMetadata.MethodDefinition_AssertNotNull.Instantiate(TypeName.NoTypes, TypeName.NoTypes);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetAssertIsNull(out IMethod method)
        {
            method = XunitTestFrameworkMetadata.MethodDefinition_AssertNull.Instantiate(TypeName.NoTypes, TypeName.NoTypes);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetAssertIsTrue(out IMethod method)
        {
            method = XunitTestFrameworkMetadata.MethodDefinition_AssertTrue.Instantiate(TypeName.NoTypes, TypeName.NoTypes);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetFixtureSetupTeardownMethods(TypeEx type, out Method fixtureSetup, out Method fixtureTeardown, out Method testSetup, out Method testTeardown)
        {
            // Constructor and Dispose are supported natively
            testSetup = testTeardown = fixtureSetup = fixtureTeardown = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool TryMarkOwner(MethodDefinitionBuilder method, string owner)
        {
            SafeDebug.AssumeNotNull(method, "method");
            SafeDebug.AssumeNotNull(owner, "owner");

            method.CustomAttributes.Add(new CustomAttributeBuilder(XunitTestFrameworkMetadata.Ctor_TraitAttribute,
                                                                   MetadataExpression.String("Owner"),
                                                                   MetadataExpression.String(owner)));

            return true;
        }

        /// <inheritdoc/>
        public override bool TryReadExpectedException(ICustomAttributeProviderEx target, out TypeEx exceptionType)
        {
            // xUnit.net does not have an [ExpectedException] attribute
            exceptionType = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool TryGetTestProjectTemplate(ILanguage language, out string templateName, out string languageName, out string[] unwantedFiles)
        {
            var templateLanguageName = ToTemplateLanguageName(language.FileExtension.ToLowerInvariant());
            if (templateLanguageName == null)
                return base.TryGetTestProjectTemplate(language, out templateName, out languageName, out unwantedFiles);

            templateName = string.Format("Microsoft.{0}.ClassLibrary", templateLanguageName);
            languageName = language.Name;
            unwantedFiles = new[] { string.Format("Class1{0}", language.FileExtension) };
            return true;
        }

        string ToTemplateLanguageName(string fileExtension)
        {
            if (fileExtension == ".cs")
                return "CSharp";
            if (fileExtension == ".vb")
                return "VisualBasic";

            return null;
        }
    }
}
