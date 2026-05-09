namespace Cyrena.Synthesis.Contracts
{
    /// <summary>
    /// Structured argument system for F# scripts.
    /// Scripts access arguments by name with type-safe accessors.
    /// This replaces raw positional string[] args with a self-describing,
    /// permission-aware, validation-friendly argument model.
    /// </summary>
    public interface ICapabilityArgs
    {
        /// <summary>
        /// Gets a string argument by name.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>The string value, or empty string if not found.</returns>
        string GetString(string name);

        /// <summary>
        /// Gets an Int32 argument by name.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>The parsed integer value, or 0 if not found or invalid.</returns>
        int GetInt32(string name);

        /// <summary>
        /// Gets a boolean argument by name.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>The parsed boolean value, or false if not found or invalid.</returns>
        bool GetBoolean(string name);

        /// <summary>
        /// Gets a double argument by name.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <returns>The parsed double value, or 0.0 if not found or invalid.</returns>
        double GetDouble(string name);

        /// <summary>
        /// Deserializes a JSON argument by name into a strongly typed object.
        /// </summary>
        /// <typeparam name="T">The target type for deserialization.</typeparam>
        /// <param name="name">The argument name.</param>
        /// <returns>The deserialized object, or default(T) if not found or invalid.</returns>
        T GetJson<T>(string name);

        /// <summary>
        /// Checks whether an argument with the given name exists.
        /// </summary>
        bool Has(string name);

        /// <summary>
        /// Gets the raw string value of an argument without type conversion.
        /// </summary>
        string? GetRaw(string name);

        /// <summary>
        /// All argument names available in this context.
        /// </summary>
        IReadOnlyList<string> Names { get; }

        /// <summary>
        /// The total number of arguments.
        /// </summary>
        int Count { get; }
    }
}
