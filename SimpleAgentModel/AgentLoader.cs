namespace SimpleAgentModel;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public class AgentLoader
{
    public string Path { get; init; } = string.Empty;
    private Type? _agentType;

    public AgentLoader() { }

    public AgentLoader(Type t)
    {
        if (typeof(Agent).IsAssignableFrom(t))
            _agentType = t;
        else
            throw new ApplicationException($"The type {t} is not inherited from class Agent.");
    }

    private string LoadClassSource()
    {
        string spec;

        try { spec = File.ReadAllText(Path); } catch { throw new ApplicationException($"The path {Path} is not valid."); }

        string[] lines = spec.Split("\n");
        string possibleStates = lines[0].Split("//")[0];
        int nRules;
        if (!int.TryParse(lines[1].Split("//")[0], out nRules))
            throw new ApplicationException($"Error when loading agent: number of rules on line 2 isn't valid integer. \nLine 2: {lines[1]}");
        List<(int from, int to)> simpleRules = new();
        List<(bool isMin, int cut, int from, int to)> neighbourRules = new();
        string defaultRule = string.Empty;

        for (int i = 0; i < nRules; i++)
        {
            string line = lines[2 + i].Split("//")[0].ToLower();
            string[] split = line.Split("->");
            if (int.TryParse(split[0], out int left))
            {
                int.TryParse(split[1], out int right);
                simpleRules.Add((left, right));
            }
            else if (line.StartsWith("default"))
            {
                defaultRule = split[1].Replace("x", "State");
            }
            else if (line.StartsWith("min") || line.StartsWith("max"))
            {
                if (!int.TryParse(line.Split(":")[0].Split(" ")[1], out int cut))
                    throw new ApplicationException($"Error when loading agent on line {2 + i + 1}: cannot find number before ':'. Make sure it has a space before.");
                if (!int.TryParse(split[0].Split(":")[1], out left))
                    throw new ApplicationException($"Error when loading agent on line {2 + i + 1}: cannot find number after ':' and before '->'.");
                if (!int.TryParse(split[1], out int right))
                    throw new ApplicationException($"Error when loading agent on line {2 + i + 1}: cannot find number after '->'.");

                neighbourRules.Add((line.StartsWith("min"), cut, left, right));
            }
            else
                throw new ApplicationException($"Error when loading agent on line {2 + i + 1}: unknown line start.");
        }

        if (defaultRule == string.Empty)
            throw new ApplicationException($"Error when loading agent: doesn't have default case.");

        // Build the rule bodies
        var simpleRulesBuilder = new StringBuilder();
        foreach (var rule in simpleRules)
            simpleRulesBuilder.AppendLine($"            if (State == {rule.from}) {{ return {rule.to}; }}");

        var neighbourRulesBuilder = new StringBuilder();
        if (neighbourRules.Count > 0)
        {
            neighbourRulesBuilder.AppendLine("            var counts = neighbours");
            neighbourRulesBuilder.AppendLine("                .GroupBy(x => x)");
            neighbourRulesBuilder.AppendLine("                .ToDictionary(g => g.Key, g => g.Count());");
            foreach (var rule in neighbourRules)
                neighbourRulesBuilder.AppendLine(
                    $"            if (counts.ContainsKey({rule.from}) && counts[{rule.from}] {(rule.isMin ? ">=" : "<=")} {rule.cut}) {{ return {rule.to}; }}");
        }

        // Now inject into the template
        string classSource = $@"
using System;
using System.Collections.Generic;
using System.Linq;               // needed for GroupBy/ToDictionary

namespace SimpleAgentModel
{{
    public class GeneratedAgent : Agent
    {{
        public override int GetNextState(int[] neighbours)
        {{
            {simpleRulesBuilder.ToString()}
            {neighbourRulesBuilder.ToString()}
            return {defaultRule};
        }}

        protected override void FillPossibleStates()
        {{
            PossibleStates = new int[]
            {{
                {possibleStates}
            }};
        }}
    }}
}}";
        return classSource;
    }

    private void Load()
    {
        // 1) Generate source
        string classSource = LoadClassSource();

        // 2) Parse + compile
        var syntaxTree = CSharpSyntaxTree.ParseText(classSource);

        // load the assemblies
        var coreLib = typeof(object).Assembly;            // System.Private.CoreLib.dll
        var agentLib = typeof(Agent).Assembly;             // Agent class
        var collectionsLib = Assembly.Load(new AssemblyName("System.Collections"));
        var linqLib = Assembly.Load(new AssemblyName("System.Linq"));
        var systemRuntime = Assembly.Load(new AssemblyName("System.Runtime"));

        var refs = new[]
        {
        MetadataReference.CreateFromFile(coreLib.Location),
        MetadataReference.CreateFromFile(agentLib.Location),
        MetadataReference.CreateFromFile(collectionsLib.Location),
        MetadataReference.CreateFromFile(linqLib.Location),
        MetadataReference.CreateFromFile(systemRuntime.Location),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "DynGenAssembly",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            var errors = string.Join(Environment.NewLine,
                result.Diagnostics
                      .Where(d => d.Severity == DiagnosticSeverity.Error)
                      .Select(d => d.ToString()));
            throw new InvalidOperationException($"Compilation failed:{Environment.NewLine}{errors}");
        }

        // 3) Load the Type
        ms.Seek(0, SeekOrigin.Begin);
        var asm = Assembly.Load(ms.ToArray());
        _agentType = asm.GetType("SimpleAgentModel.GeneratedAgent")
            ?? throw new InvalidOperationException("Type SimpleAgentModel.GeneratedAgent not found");
    }

    public Agent Create()
    {
        if (_agentType is null)
            Load();

        return (Agent)(Activator.CreateInstance(_agentType!)
            ?? throw new ApplicationException($"Cannot instantiate agent {_agentType}."));
    }
}
