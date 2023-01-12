namespace LudumDareTools;

using System.Diagnostics.CodeAnalysis;

public struct LD_Rating
{
    public LD_Grade overall;
    public LD_Grade fun;
    public LD_Grade innovation;
    public LD_Grade theme;
    public LD_Grade graphics;
    public LD_Grade audio;
    public LD_Grade humor;
    public LD_Grade mood;

    public float GetTotal() =>
        overall.value + fun.value + innovation.value + theme.value +
        graphics.value + audio.value + humor.value + mood.value;

    public float GetRatedCount(bool[] opt_outs) =>
        (!opt_outs[0] && overall.value > 0 ? 1 : 0) +
        (!opt_outs[1] && fun.value > 0 ? 1 : 0) +
        (!opt_outs[2] && innovation.value > 0 ? 1 : 0) +
        (!opt_outs[3] && theme.value > 0 ? 1 : 0) +
        (!opt_outs[4] && graphics.value > 0 ? 1 : 0) +
        (!opt_outs[5] && audio.value > 0 ? 1 : 0) +
        (!opt_outs[6] && humor.value > 0 ? 1 : 0) +
        (!opt_outs[7] && mood.value > 0 ? 1 : 0);

    public override bool Equals([NotNullWhen(true)] object other)
    {
        if (other is not LD_Rating b)
            return false;

        return GetHashCode() == b.GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(overall, fun, innovation, theme, graphics, audio);
    }

    public static bool operator ==(LD_Rating left, LD_Rating right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LD_Rating left, LD_Rating right)
    {
        return !(left == right);
    }
}