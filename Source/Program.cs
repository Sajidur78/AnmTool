using System.Text.Json;
using AnmTool;
using SharpNeedle.LostWorld.Animation;
using SharpNeedle.Utilities;

if (args.Length == 0)
{
    Console.WriteLine("Insufficient Arguments");
    Console.WriteLine("Usage:");
    Console.WriteLine("AnmTool [anm|json]");
    return;
}

var file = args[0];

if (!File.Exists(file))
{
    Console.WriteLine($"file not found {file}");
    return;
}

if (file.EndsWith(".anm", StringComparison.InvariantCultureIgnoreCase))
{
    var script = new CharAnimScript();
    script.Read(file);
    
    using var outStream = File.Create(Path.ChangeExtension(file, ".json"));
    JsonSerializer.Serialize(outStream, new ProxyCharAnimScript(script), JsonTypeContext.SerializerOptions);
    return;
}
else if (file.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
{
    using var inStream = File.OpenRead(file);
    var script = JsonSerializer.Deserialize<ProxyCharAnimScript>(inStream, JsonTypeContext.SerializerOptions)!;
    
    script.Base.Write(Path.ChangeExtension(file, ".anm"));
}

Console.WriteLine($"Invalid file type {Path.GetExtension(file)}");