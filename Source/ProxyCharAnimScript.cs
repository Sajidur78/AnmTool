namespace AnmTool;
using SharpNeedle.LostWorld.Animation;
using System.Text.Json.Serialization;

public class ProxyCharAnimScript
{
    [JsonIgnore]
    public CharAnimScript Base;

    public List<SimpleAnimation> SimpleAnimations
    {
        get => Base.SimpleAnimations;
        set => Base.SimpleAnimations = value;
    }

    public List<ComplexAnimation> ComplexAnimations
    {
        get => Base.ComplexAnimations;
        set => Base.ComplexAnimations = value;
    }

    public TransitionTable Transitions
    {
        get => Base.Transitions; 
        set => Base.Transitions = value;
    }

    public ProxyCharAnimScript()
    {
        Base = new CharAnimScript();
    }

    public ProxyCharAnimScript(CharAnimScript @base)
    {
        Base = @base;
    }
}