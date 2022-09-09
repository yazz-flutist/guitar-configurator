namespace GuitarConfiguratorSharp.NetCore.Configuration;

public interface IOutputButton
{
    public int Index(bool xbox);
    public OutputType OutputType { get; }
    string Generate(bool xbox);
}