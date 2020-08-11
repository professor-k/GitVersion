﻿using System.IO;
using GitVersion.Command;
using GitVersion.Infrastructure;

namespace GitVersion.Normalization
{
    [Command("normalize", "Normalizes the git repository for GitVersion calculations.")]
    public class NormalizeOptions : GitVersionOptions
    {
        [Option("--work-dir", "The working directory with the git repository")]
        public DirectoryInfo WorkDir { get; set; }
    }
}