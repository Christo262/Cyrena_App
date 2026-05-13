namespace Cyrena.Attributes
{
    /// <summary>
    /// Indicates that the associated property should have a value injected from the <see cref="Cyrena.Models.KernelComponentBase.Kernel"/>.
    /// Overrides <see cref="Microsoft.AspNetCore.Components.ComponentBase.OnParametersSet"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class KernelInjectAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the object that specifies the key of the service to inject.
        /// </summary>
        public object? Key { get; init; }
    }
}
