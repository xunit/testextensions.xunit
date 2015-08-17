using System;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
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

        static readonly Guid GUID_UniversalWindows = new Guid("{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}");
        static readonly Guid GUID_WindowsPhoneApp81 = new Guid("{76F1466A-8B6D-4E39-A767-685A06062A39}");
        static readonly Guid GUID_WindowsStore81 = new Guid("{BC8A1FFA-BEE3-4634-8014-F334798102B3}");

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

            // TODO: Remove references to "MSTestFramework" and "MSTestFramework.Universal"

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
            string templateName;
            var projectLanguage = VisualStudioHelper.GetProjectLanguage(sourceProject);
            var isCSharp = projectLanguage == "CSharp";

            var projectTypeGuids = sourceProject.ProjectTypeGuids(serviceProvider).ToList();
            if (projectTypeGuids.Contains(GUID_WindowsStore81))
                templateName = isCSharp ? "Microsoft.CS.WinRT.UnitTestLibrary" : "Microsoft.VisualBasic.WinRT.UnitTestLibrary";
            else if (isCSharp && projectTypeGuids.Contains(GUID_WindowsPhoneApp81))  // No VB template for WPA81, have to fall back to a class library
                templateName = "Microsoft.CS.WindowsPhoneApp.UnitTestApp";
            else if (projectTypeGuids.Contains(GUID_UniversalWindows))
                templateName = isCSharp ? "Microsoft.CSharp.UAP.UnitTestProject" : "Microsoft.VisualBasic.UAP.UnitTestProject";
            else
                templateName = isCSharp ? "Microsoft.CSharp.ClassLibrary" : "Microsoft.VisualBasic.Windows.ClassLibrary";

            return solution.GetProjectTemplate(templateName, projectLanguage);
        }
    }
}
