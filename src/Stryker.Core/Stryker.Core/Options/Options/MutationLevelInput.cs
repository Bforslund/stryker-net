﻿using Microsoft.CodeAnalysis.CSharp;
using Stryker.Core.Exceptions;
using Stryker.Core.Mutators;
using System;
using System.Collections.Generic;

namespace Stryker.Core.Options.Options
{
    public class MutationLevelInput : ComplexStrykerInput<MutationLevel, string>
    {
        static MutationLevelInput()
        {
            HelpText = $"Specifies what mutations will be placed in your project. | { FormatOptionsString(DefaultInput, (IEnumerable<MutationLevel>)Enum.GetValues(DefaultValue.GetType())) }";
            DefaultValue = MutationLevel.Standard;
        }

        public override StrykerInput Type => StrykerInput.MutationLevel;

        public MutationLevelInput(string mutationLevel)
        {
            if (mutationLevel is { })
            {
                if (Enum.TryParse(mutationLevel, true, out MutationLevel level))
                {
                    Value = level;
                }
                else
                {
                    throw new StrykerInputException($"The given mutation level ({mutationLevel}) is invalid.");
                }
            }
        }
    }
}
