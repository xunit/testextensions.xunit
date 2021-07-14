using System;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TestPlatform.TestGeneration;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Data;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Logging;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Model;
using VSLangProj80;

namespace CreateUnitTest.Xunit
{
    public class XunitSolutionManager : SolutionManagerBase
    {
        readonly string visualStudioRunnerPackageVersion;
        readonly string xunitPackageVersion;

        public XunitSolutionManager(IServiceProvider serviceProvider,
                                    INaming naming,
                                    IDirectory directory,
                                    string xunitPackageVersion,
                                    string visualStudioRunnerPackageVersion)
            : base(serviceProvider, naming, directory)
        {
            this.xunitPackageVersion = xunitPackageVersion;
            this.visualStudioRunnerPackageVersion = visualStudioRunnerPackageVersion;
        }

        protected override void OnUnitTestProjectCreated(Project unitTestProject, CodeFunction2 sourceMethod)
        {
            if (unitTestProject == null)
                throw new ArgumentNullException("unitTestProject");
            if (sourceMethod == null)
                throw new ArgumentNullException("sourceMethod");

            // Add package reference for xUnit.net
            TraceLogger.LogInfo("XunitSolutionManager.OnUnitTestProjectCreated: Adding reference to NuGet packages 'xunit {0}' and 'xunit.runner.visualstudio {1}'", xunitPackageVersion, visualStudioRunnerPackageVersion);
            EnsureNuGetReference(unitTestProject, "xunit", xunitPackageVersion);
            EnsureNuGetReference(unitTestProject, "xunit.runner.visualstudio", visualStudioRunnerPackageVersion);

            // Remove any *.cs/*.vb files from the root of the project (i.e., Class1.cs/Class1.vb)
            foreach (var projectItem in unitTestProject.ProjectItems.AsEnumerable())
            {
                var extension = Path.GetExtension(projectItem.Name).ToLowerInvariant();
                if (extension == ".cs" || extension == ".vb")
                {
                    TraceLogger.LogInfo("XunitSolutionManager.OnUnitTestProjectCreated: Removing project item {0}", projectItem.Name);
                    projectItem.Delete();
                }
            }

            var vsp = unitTestProject.Object as VSProject2;
            var reference = vsp?.References.Find(GlobalConstants.MSTestAssemblyName);
            if (reference != null)
            {
                TraceLogger.LogInfo("NUnitSolutionManager.OnUnitTestProjectCreated: Removing reference to {0}", reference.Name);
                reference.Remove();
            }
        }
    }
}
