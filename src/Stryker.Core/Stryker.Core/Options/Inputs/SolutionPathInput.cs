﻿using Stryker.Core.Exceptions;
using System.IO.Abstractions;

namespace Stryker.Core.Options.Inputs
{
    public class SolutionPathInput : SimpleStrykerInput<string>
    {
        static SolutionPathInput()
        {
            Description = @"Full path to your solution file. The solution file is needed to build the project and resolve dependencies for
    .net framework but can optionally be used for .net core. Path can be relative from test project or full path.";
        }

        public override StrykerInput Type => StrykerInput.BasePath;

        public SolutionPathInput(IFileSystem fileSystem, string solutionPath)
        {
            if (solutionPath is { })
            {
                if (!fileSystem.File.Exists(solutionPath))  //validate file existance and maintain moq
                {
                    throw new StrykerInputException("Given solution path does not exist: {0}", solutionPath);
                }

                Value = solutionPath;
            }
        }
    }
}
