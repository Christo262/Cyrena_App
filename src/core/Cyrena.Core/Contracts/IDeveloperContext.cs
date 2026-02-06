using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Models;

namespace Cyrena.Contracts
{
    public interface IDeveloperContext
    {
        Project Project { get; }
        ProjectPlan ProjectPlan { get; }
        IConnection Connection { get; }
        Kernel Kernel { get; }

        ChatHistory KernelHistory { get; }
        ChatHistory DisplayHistory { get; }

        void AttachListener(IConversationListener listener);
        void AddMessage(AuthorRole role, string message);
        void OnStreamToken(string token);

        bool Handling { get; }

        AuthorRole ErrorRole { get; }
        AuthorRole WarningRole { get; }
        AuthorRole SuccessRole { get; }
        AuthorRole InfoRole { get; }

        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogSuccess(string message);

        void HandleStart();
        void HandleEnd();
        void Handle(AuthorRole role, string message);

        void ResetKernelHistory();
    }
}
