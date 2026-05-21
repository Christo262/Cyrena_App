namespace Cyrena.Models
{
    /// <summary>
    /// Inidicates that a <see cref="Microsoft.SemanticKernel.FunctionResultContent.Result"/> is suppressible.
    /// This will call Suppress after AI has interacted with the result in order to reduce context size
    /// </summary>
    public interface ISuppressibleResult
    {
        string Suppress();
    }
}
