using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Portability;
using BenchmarkDotNet.Tests.XUnit;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Toolchains.Mono;

namespace BenchmarkDotNet.IntegrationTests
{
    public class MonoTests : BenchmarkTestExecutor
    {
        [FactDotNetCoreOnly("It's impossible to reliably detect the version of NativeAOT if the process is not a .NET Core or NativeAOT process")]
        public void Mono60IsSupported()
        {
            if (!RuntimeInformation.Is64BitPlatform()) // NativeAOT does not support 32bit yet
                return;
            if (ContinuousIntegration.IsGitHubActionsOnWindows()) // no native dependencies installed
                return;
            if (ContinuousIntegration.IsAppVeyorOnWindows()) // too time consuming for AppVeyor (1h limit)
                return;
            //if (NativeAotRuntime.GetCurrentVersion().RuntimeMoniker < RuntimeMoniker.NativeAot70) // we can't target net6.0 and use .NET 7 ILCompiler anymore (#2080)
            //return;

            var config = ManualConfig.CreateEmpty()
                .AddJob(Job.Dry
                    .WithRuntime(MonoRuntime.Mono60) // we test against latest version for current TFM to make sure we avoid issues like #1055
                    .WithToolchain(MonoToolchain.From(new NetCoreAppSettings("net6.0", null, "mono60"))));
            CanExecute<MonoBenchmark>(config);
        }

        public class MonoBenchmark
        {
            internal const string EnvVarKey = "AVX2_IsSupported";

            [Benchmark]
            public void Check()
            {
                if (!RuntimeInformation.IsNativeAOT)
                    throw new Exception("This is NOT NativeAOT");
#if NET6_0_OR_GREATER
            if (System.Runtime.Intrinsics.X86.Avx2.IsSupported != bool.Parse(Environment.GetEnvironmentVariable(EnvVarKey)))
                throw new Exception("Unexpected Instruction Set");
#endif
            }
        }
    }
}
