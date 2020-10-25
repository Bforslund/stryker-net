﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Stryker.Core.Baseline;
using Stryker.Core.Logging;
using Stryker.Core.Mutators;
using Stryker.Core.Options.Options;
using Stryker.Core.Reporters;
using Stryker.Core.TestRunners;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Stryker.Core.Options
{
    public class StrykerOptions
    {
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;

        public bool DevMode { get; }

        public string BasePath { get; }
        public string SolutionPath { get; }
        public string OutputPath { get; }

        public LogOptions LogOptions { get; }
        public MutationLevel MutationLevel { get; }
        public Thresholds Thresholds { get; }

        public int AdditionalTimeoutMS { get; }
        public LanguageVersion LanguageVersion { get; }
        public TestRunner TestRunner { get; set; }

        public int ConcurrentTestrunners { get; }
        public string ProjectUnderTestNameFilter { get; }
        public IEnumerable<string> TestProjects { get; }

        public bool CompareToDashboard { get; }
        public IEnumerable<Reporter> Reporters { get; }

        public BaselineProvider BaselineProvider { get; }
        public string AzureFileStorageUrl { get; }
        public string AzureSAS { get; }

        public bool DiffEnabled { get; }
        public string GitDiffTarget { get; }
        public IEnumerable<Mutator> ExcludedMutations { get; }
        public IEnumerable<Regex> IgnoredMethods { get; }
        public IEnumerable<FilePattern> FilePatterns { get; }
        public OptimizationFlags Optimizations { get; }
        public string OptimizationMode { get; }

        public string DashboardUrl { get; } = "https://dashboard.stryker-mutator.io";
        public string DashboardApiKey { get; }
        public string ProjectName { get; }
        public string ModuleName { get; }
        public string ProjectVersion { get; }

        public IEnumerable<FilePattern> DiffIgnoreFiles { get; }

        public string FallbackVersion { get; }


        public StrykerOptions(
            ILogger logger = null,
            IFileSystem fileSystem = null,
            string basePath = "",
            string solutionPath = null,

            string logLevel = "info",
            bool logToFile = false,

            string mutationLevel = null,

            int thresholdHigh = 80,
            int thresholdLow = 60,
            int thresholdBreak = 0,

            int additionalTimeoutMS = 5000,
            string languageVersion = "latest",
            string testRunner = "vstest",

            int? maxConcurrentTestRunners = null,
            string projectUnderTestNameFilter = "",
            IEnumerable<string> testProjects = null,

            bool compareToDashboard = false,
            IEnumerable<string> reporters = null,

            string baselineStorageLocation = null,
            string azureFileStorageUrl = null,
            string azureSAS = null,

            IEnumerable<string> excludedMutations = null,
            IEnumerable<string> ignoredMethods = null,
            bool devMode = false,
            string coverageAnalysis = "perTest",
            bool abortTestOnFail = true,
            bool disableSimultaneousTesting = false,
            IEnumerable<string> mutate = null,
            bool diff = false,
            string gitDiffTarget = "master",
            string dashboardApiKey = null,
            string dashboardUrl = "https://dashboard.stryker-mutator.io",
            string projectName = null,
            string moduleName = null,
            string projectVersion = null,
            string fallbackVersion = null,
            IEnumerable<string> diffIgnoreFiles = null)
        {
            _logger = logger;
            _fileSystem = fileSystem ?? new FileSystem();

            DevMode = new DevModeInput(devMode).Value;

            BasePath = new BasePathInput(_fileSystem, basePath).Value;
            SolutionPath = new SolutionPathInput(_fileSystem, solutionPath).Value;
            OutputPath = new OutputPathInput(_logger, _fileSystem, BasePath).Value;

            LogEventLevel LogOptionLevel = new LogOptionLevelInput(logLevel).Value;
            bool LogOptionToFile = new LogOptionToFileInput(logToFile, OutputPath).Value;
            LogOptions = new LogOptions(LogOptionLevel, LogOptionToFile, OutputPath);

            MutationLevel = new MutationLevelInput(mutationLevel).Value;

            var highTreshhold = new ThresholdsHighInput(thresholdHigh, thresholdLow).Value;
            var lowTreshhold = new ThresholdsLowInput(thresholdHigh, thresholdLow).Value;
            var breakTreshhold = new ThresholdsBreakInput(thresholdBreak).Value;
            Thresholds = new Thresholds(highTreshhold, lowTreshhold, breakTreshhold);

            AdditionalTimeoutMS = new AdditionalTimeoutMsInput(additionalTimeoutMS).Value;
            LanguageVersion = new LanguageVersionInput(languageVersion).Value;
            TestRunner = new TestRunnerInput(testRunner).Value;

            ConcurrentTestrunners = new ConcurrentTestrunnersInput(_logger, maxConcurrentTestRunners).Value;
            ProjectUnderTestNameFilter = new ProjectUnderTestNameFilterInput(projectUnderTestNameFilter).Value;
            TestProjects = new TestProjectsInput(testProjects).Value;

            CompareToDashboard = new CompareToDashboardInput(compareToDashboard).Value;
            var reportersList = new ReportersInput(reporters);
            Reporters = reportersList.ReportersList(CompareToDashboard);
            BaselineProvider = new BaselineProviderInput(baselineStorageLocation, Reporters.Contains(Reporter.Dashboard)).Value;
            AzureFileStorageUrl = new AzureFileStorageUrlInput(azureFileStorageUrl, BaselineProvider).Value;
            AzureSAS = new AzureFileStorageSasInput(azureSAS, BaselineProvider).Value;
            /* --- */

            var dashboardEnabled = CompareToDashboard || Reporters.Contains(Reporter.Dashboard);

            DashboardUrl = new DashboardUrlInput(dashboardUrl).Value;
            DashboardApiKey = new DashboardApiKeyInput(dashboardApiKey, dashboardEnabled).Value;
            ProjectName = new ProjectNameInput(projectName, dashboardEnabled).Value;

            DiffEnabled = new DiffEnabledInput(diff).Value;
            GitDiffTarget = new GitDiffTargetInput(gitDiffTarget, DiffEnabled).Value;
            DiffIgnoreFiles = new DiffIgnoreFilePatternsInput(diffIgnoreFiles).Value;

            FallbackVersion = new FallbackVersionInput(fallbackVersion, gitDiffTarget).Value;
            ProjectVersion = new ProjectVersionInput(projectVersion, FallbackVersion, dashboardEnabled, CompareToDashboard).Value;
            ModuleName = new ModuleNameInput(moduleName).Value;

            FilePatterns = new MutateInput(mutate).Value;
            IgnoredMethods = new IgnoredMethodsInput(ignoredMethods).Value;
            ExcludedMutations = new ExcludedMutatorsInput(excludedMutations).Value;

            OptimizationMode = new OptimizationModeInput(coverageAnalysis).Value;
            Optimizations = new OptimizationsInput(OptimizationMode, abortTestOnFail, disableSimultaneousTesting).Value;
        }
    }
}
