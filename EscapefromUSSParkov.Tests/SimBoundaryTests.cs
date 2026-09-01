using System;
using System.Reflection;

namespace EscapefromUSSParkov.Tests;

/// <summary>
/// Guards the load-bearing rule of the three-project split: EscapefromUSSParkov.Sim
/// must never reference the engine. This test fails the build the moment a Godot
/// dependency leaks into the simulation assembly. See
/// .claude/knowledge/decisions/009-sim-view-separation.md.
/// </summary>
public sealed class SimBoundaryTests
{
    [Fact]
    public void SimAssembly_HasNoGodotReference()
    {
        Assembly sim = Assembly.Load("EscapefromUSSParkov.Sim");

        Assert.DoesNotContain(
            sim.GetReferencedAssemblies(),
            a => a.Name is not null
                && a.Name.Contains("Godot", StringComparison.OrdinalIgnoreCase));
    }
}
