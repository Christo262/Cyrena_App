using System.ComponentModel;

namespace Cyrena.Screens.Models;

public enum InteropMode
{
    [Description("Catpures a screenshot and shares it as a message from you.")]
    UserMessage,
    [Description("Captures a screenshot and shares directly as the result of the function.")]
    FunctionResult
}