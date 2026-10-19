using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Modules
{
    internal static class Stacks
    {
        /// <summary>
        /// Carries out a change on a host whose every step completes at once, so the change has finished, or thrown, when
        /// this returns.
        /// </summary>
        internal static void Apply(ModuleStack stack, ModuleChange change)
        {
            stack.ApplyAsync(change).GetAwaiter().GetResult();
        }

        /// <summary>Module 0 alone on the stack, and the host's log cleared, ready for the change under test.</summary>
        internal static ModuleStack WithOne(FakeModuleHost host)
        {
            ModuleStack stack = new ModuleStack(host);

            Apply(stack, ModuleChange.Clear(0));
            host.ClearLog();

            return stack;
        }

        /// <summary>Module 1 over module 0, and the host's log cleared.</summary>
        internal static ModuleStack WithTwo(FakeModuleHost host)
        {
            ModuleStack stack = WithOne(host);

            Apply(stack, ModuleChange.Over(1));
            host.ClearLog();

            return stack;
        }

        internal static string Steps(FakeModuleHost host)
        {
            string steps = string.Join(", ", host.Log);

            return steps;
        }
    }
}
