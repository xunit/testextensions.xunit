using System;
using System.Globalization;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.TestGeneration;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Data;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Logging;
using Microsoft.VisualStudio.TestPlatform.TestGeneration.Model;

namespace CreateUnitTest.Xunit
{
    public class XunitSolutionManager : SolutionManagerBase
    {
        readonly IServiceProvider serviceProvider;
        readonly Solution2 solution;
        readonly string visualStudioRunnerPackageVersion;
        readonly string xunitPackageVersion;

        public XunitSolutionManager(IServiceProvider serviceProvider,
                                    INaming naming,
                                    IDirectory directory,
                                    string xunitPackageVersion,
                                    string visualStudioRunnerPackageVersion)
            : base(serviceProvider, naming, directory)
        {
            this.serviceProvider = serviceProvider;
            this.xunitPackageVersion = xunitPackageVersion;
            this.visualStudioRunnerPackageVersion = visualStudioRunnerPackageVersion;

            var dte = (DTE)this.serviceProvider.GetService(typeof(SDTE));
            solution = (Solution2)dte.Solution;
        }

        protected override void OnUnitTestProjectCreated(Project unitTestProject, CodeFunction2 sourceMethod)
        {
            if (unitTestProject == null)
                throw new ArgumentNullException("unitTestProject");
            if (sourceMethod == null)
                throw new ArgumentNullException("sourceMethod");

            // Add package reference for xUnit.net
            TraceLogger.LogInfo("XunitSolutionManager.OnUnitTestProjectCreated: Adding reference to NuGet packages 'xunit' and 'xunit.runner.visualstudion' (version {0})", xunitPackageVersion);
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
        }

        protected override string UnitTestProjectTemplatePath(Project sourceProject)
        {
            var projectLanguage = VisualStudioHelper.GetProjectLanguage(sourceProject);
            string templateName;

            if (sourceProject.ProjectTypeGuids(serviceProvider).Contains(GlobalConstants.WindowsStoreAppProjectTypeGuid))
                templateName = projectLanguage == "CSharp" ? "Microsoft.CS.WinRT.UnitTestLibrary" : "Microsoft.VisualBasic.WinRT.UnitTestLibrary";
            else
                templateName = string.Format(CultureInfo.InvariantCulture, "Microsoft.{0}.ClassLibrary", projectLanguage);

            return solution.GetProjectTemplate(templateName, projectLanguage);
        }
    }
}
