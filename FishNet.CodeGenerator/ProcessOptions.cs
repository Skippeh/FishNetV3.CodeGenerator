using System.Collections.Generic;

namespace FishNet.CodeGenerator;

public record ProcessOptions
{
    public ICollection<string> AssemblySearchPaths { get; set; } = [];
}