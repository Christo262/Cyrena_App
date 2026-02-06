using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Cyrena.Contracts;
using Cyrena.Models;

namespace Cyrena.Runtime.Services
{
    internal class DeveloperContext : IDeveloperContext
    {
        private readonly List<IConversationListener> _listeners;
        public DeveloperContext(Project project, ProjectPlan plan, IConnection connection)
        {
            _listeners = new List<IConversationListener>();
            Project = project;
            ProjectPlan = plan;
            Connection = connection;

            ErrorRole = new("Log.Error");
            WarningRole = new("Log.Warn");
            SuccessRole = new("Log.Success");
            InfoRole = new("Log.Info");
        }

        public Project Project { get; }
        public ProjectPlan ProjectPlan { get; }
        public IConnection Connection { get; }
        public Kernel Kernel { get; internal set; } = default!;

        public ChatHistory KernelHistory { get; internal set; } = default!;
        public ChatHistory DisplayHistory { get; internal set; } = default!;

        public AuthorRole ErrorRole { get; }
        public AuthorRole WarningRole { get; }
        public AuthorRole SuccessRole { get; }
        public AuthorRole InfoRole { get; }

        public void AddMessage(AuthorRole role, string message)
        {
            if (role == AuthorRole.User || role == AuthorRole.Assistant)
            {
                KernelHistory.AddMessage(role, message);
                DisplayHistory.AddMessage(role, message);
                _listeners.ForEach(e => e.OnDisplayHistoryChanged());
            }
            else if (role == AuthorRole.Tool || role == AuthorRole.System)
            {
                KernelHistory.AddMessage(role, message);
            }
            else if (role == ErrorRole || role == WarningRole || role == SuccessRole || role == InfoRole)
            {
                DisplayHistory.AddMessage(role, message);
                _listeners.ForEach(e => e.OnDisplayHistoryChanged());
            }
        }

        public void AttachListener(IConversationListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void LogError(string message)
        {
            AddMessage(ErrorRole, message);
        }

        public void LogInfo(string message)
        {
            AddMessage(InfoRole, message);
        }

        public void LogSuccess(string message)
        {
            AddMessage(SuccessRole, message);
        }

        public void LogWarning(string message)
        {
            AddMessage(WarningRole, message);
        }

        public void OnStreamToken(string token)
        {
            _listeners.ForEach(e => e.OnStreamToken(token));
        }

        public bool Handling { get; private set; }

        public void HandleStart()
        {
            Handling = true;
            _listeners.ForEach(e => e.OnHandleStart());
        }
        public void HandleEnd()
        {
            {
                Handling = false;
                ResetKernelHistory();
                _listeners.ForEach(e => e.OnHandleComplete());
            }
        }

        private Task? _handle { get; set; }
        private CancellationTokenSource? _token { get; set; }
        public void Handle(AuthorRole role, string message)
        {
            if (_handle != null)
            {
                if (_handle.IsCompleted == false)
                    return;
                _handle.Dispose();
                _handle = null;
            }
            if (_token != null)
            {
                _token.Cancel();
                _token.Dispose();
            }
            Handling = true;
            _token = new CancellationTokenSource();
            _handle = Task.Run(async () =>
            {
                try
                {
                    await Connection.HandleAsync(message, role, this, _token.Token);
                }
                catch (Exception ex)
                {
                    this.LogError(ex.Message);
                }
            }, _token.Token);
        }

        public void ResetKernelHistory()
        {
            var history = new ChatHistory();
            var hs = KernelHistory.Where(x => x.Role == AuthorRole.System);
            history.AddRange(hs);
            var usr = KernelHistory.LastOrDefault(x => x.Role == AuthorRole.User);
            var ass = KernelHistory.LastOrDefault(x => x.Role == AuthorRole.Assistant);
            if (usr != null && ass != null && !string.IsNullOrEmpty(ass.Content)) //to prevent timeouts from the LLM causing problems
            {
                history.Add(usr);
                history.Add(ass);
            }
            KernelHistory = history;
        }
    }
}
