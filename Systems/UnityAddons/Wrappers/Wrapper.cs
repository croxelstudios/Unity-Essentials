public class Wrapper
{
    protected virtual bool IsNull()
    {
        return false;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(Wrapper a, Wrapper b)
    {
        if (ReferenceEquals(a, null))
            return ReferenceEquals(b, null);

        if (ReferenceEquals(b, null))
            return a.IsNull();

        return a.Equals(b);
    }

    public static bool operator !=(Wrapper a, Wrapper b)
    {
        return !(a == b);
    }
}
