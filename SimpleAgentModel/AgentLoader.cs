namespace SimpleAgentModel;
using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

public class AgentLoader
{
    public string Path { get; init; } = string.Empty;
    private Type? _generatedType;

    public void Load()
    {
        var spec = File.ReadAllText(Path);

        string classSource = $@"
            namespace SimpleAgentModel;

            public class GeneratedAgent : Agent
            {{
                public override int GetNextState(int[] neighbours)
                {{
                    if (State != 2 && State != 1) // tree OR fire
                        return State;
                    if (State == 1) // burned
                        return 0;

                    foreach (var neighbour in neighbours)
                        if (neighbour == 1) return 1; // catch on fire

                    return 2; // not burning
                }}

                protected override void FillPossibleStates()
                {{
                    PossibleStates = [
                    {spec}
                    ];
                }}
            }}";

        var syntaxTree = CSharpSyntaxTree.ParseText(classSource);
        var refs = new[] {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Agent).Assembly.Location)
        };
        var compilation = CSharpCompilation.Create(
            assemblyName: "DynGenAssembly",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);
        if (!result.Success)
        {
            // handle compilation errors...
            foreach (var diag in result.Diagnostics)
                Console.Error.WriteLine(diag);
            throw new InvalidOperationException("Compilation failed");
        }

        // 4) Load and activate
        ms.Seek(0, SeekOrigin.Begin);
        var asm = Assembly.Load(ms.ToArray());
        _generatedType = asm.GetType("DynGen.GeneratedImpl")
                       ?? throw new InvalidOperationException("Type not found");
    }

    public Agent Create()
    {
        if (_generatedType is null)
            Load();
        return (Agent)Activator.CreateInstance(_generatedType!);
    }
}
