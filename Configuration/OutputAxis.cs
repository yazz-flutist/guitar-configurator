namespace GuitarConfiguratorSharp.NetCore.Configuration;

public interface IOutputAxis
{
    public StandardAxisType Type { get; }
    public abstract string Generate(bool xbox);
    public OutputType OutputType { get; }
}