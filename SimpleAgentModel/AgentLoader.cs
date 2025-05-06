namespace SimpleAgentModel;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Loads Agent from file.
/// Creates new instances of that agent.
/// </summary>
public class AgentLoader
{
    public string Path { get; init; } = string.Empty;
    public Type? AgentType { get; private set; }

    public AgentLoader() { }

    public AgentLoader(Type t)
    {
        if (typeof(Agent).IsAssignableFrom(t))
            AgentType = t;
        else
            throw new ApplicationException($"The type {t} is not inherited from class Agent.");
    }

    private string LoadClassSource()
    {
        // ===== DECLARATIONS =====
        string spec;
        List<(int from, int to)> simpleRules = new();
        List<(bool isMin, int cut, int neigh, int from, int to)> neighbourRules = new();
        string defaultRule = string.Empty;
        string[] lines;
        string possibleStates;
        string statesProb;
        int nRules;
        // the known file structure
        (string Comment, string RuleSimple, string RuleExtend, string RuleInside, string Section) Separator = ("//", "->", ":", " ", "\n");
        (string NeighbourMIN, string NeighbourMAX, string Default) RuleType = ("min", "max", "default");
        (int PossibleStates, int StatesProbabilities, int NumberOfRules, int Rules) Indexes = (0, 1, 2, 3);
        (string File, string Program) DefaultStateRewrite = ("x", "State");

        // ===== LOAD FILE =====
        try { spec = File.ReadAllText(Path); } catch { throw new ApplicationException($"The path {Path} is not valid."); }
        lines = spec.Split(Separator.Section);

        // ===== GET ALL INFO FROM FILE =====
        // STATES
        possibleStates = lines[Indexes.PossibleStates].Split(Separator.Comment)[0];
        statesProb = lines[Indexes.StatesProbabilities].Split(Separator.Comment)[0].Replace(",", "f,").TrimEnd() + "f";
        // RULES COUNT
        if (!int.TryParse(lines[Indexes.NumberOfRules].Split(Separator.Comment)[0], out nRules))
            throw new ApplicationException($"Error when loading agent: number of rules on line {Indexes.NumberOfRules} isn't valid integer. \nLine: {lines[Indexes.NumberOfRules]}");
        // RULES
        for (int i = 0; i < nRules; i++)
        {
            int left, right, cut, neigh;
            string line = lines[Indexes.Rules + i].Split(Separator.Comment)[0].ToLower();
            string[] split = line.Split(Separator.RuleSimple);

            if (int.TryParse(split[0], out left))
            {
                int.TryParse(split[1], out right);
                simpleRules.Add((left, right));
            }
            else if (line.StartsWith(RuleType.Default))
            {
                defaultRule = split[1].Replace(DefaultStateRewrite.File, DefaultStateRewrite.Program);
            }
            else if (line.StartsWith(RuleType.NeighbourMIN) || line.StartsWith(RuleType.NeighbourMAX))
            {
                if (!int.TryParse(line.Split(Separator.RuleExtend)[0].Split(Separator.RuleInside)[1], out cut))
                    throw new ApplicationException($"Error when loading agent on line {Indexes.Rules + i + 1}: cannot find number before '{Separator.RuleExtend}'. Make sure it has a '{Separator.RuleInside}' before.");
                if (!int.TryParse(line.Split(Separator.RuleExtend)[1], out neigh))
                    throw new ApplicationException($"Error when loading agent on line {Indexes.Rules + i + 1}: cannot find number in between '{Separator.RuleExtend}'.");
                if (!int.TryParse(split[0].Split(Separator.RuleExtend)[2], out left))
                    throw new ApplicationException($"Error when loading agent on line {Indexes.Rules + i + 1}: cannot find number after '{Separator.RuleExtend}' and before '{Separator.RuleSimple}'.");
                if (!int.TryParse(split[1], out right))
                    throw new ApplicationException($"Error when loading agent on line {Indexes.Rules + i + 1}: cannot find number after '{Separator.RuleSimple}'.");

                neighbourRules.Add((line.StartsWith(RuleType.NeighbourMIN), cut, neigh, left, right));
            }
            else
                throw new ApplicationException($"Error when loading agent on line {Indexes.Rules + i + 1}: unknown line start.");
        }

        if (defaultRule == string.Empty)
            throw new ApplicationException($"Error when loading agent: doesn't have default case.");

        // ===== STRING BUILDING =====
        var simpleRulesBuilder = new StringBuilder();
        foreach (var rule in simpleRules)
            simpleRulesBuilder.AppendLine($"if (State == {rule.from}) {{ return {rule.to}; }}");

        var neighbourRulesBuilder = new StringBuilder();
        if (neighbourRules.Count > 0)
        {
            neighbourRulesBuilder.AppendLine("neighbours[4] = -420; var counts = neighbours.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());");
            foreach (var rule in neighbourRules)
                neighbourRulesBuilder.AppendLine(
                    $"if (State == {rule.from} && counts.ContainsKey({rule.neigh}) && counts[{rule.neigh}] {(rule.isMin ? ">=" : "<=")} {rule.cut}) {{ return {rule.to}; }}");
        }

        // ===== PUTTING IT TOGETHER =====
        return $@"
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
            StatesProbabilities = new float[]
            {{
                {statesProb}
            }};
        }}
    }}
}}";
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
        AgentType = asm.GetType("SimpleAgentModel.GeneratedAgent")
            ?? throw new InvalidOperationException("Type SimpleAgentModel.GeneratedAgent not found");
    }

    public Agent Create()
    {
        if (AgentType is null)
            Load();

        return (Agent)(Activator.CreateInstance(AgentType!)
            ?? throw new ApplicationException($"Cannot instantiate agent {AgentType}."));
    }
}
