namespace Cyrena.Contracts
{
    public interface IConversationListener
    {
        void OnDisplayHistoryChanged();
        void OnHandleStart();
        void OnHandleComplete();
        void OnStreamToken(string token);
    }
}
