namespace Ontec.Core.Domain.Interface.Services
{
    public interface ISignalRService
    {
        Task SendMessage(string connectionId, string message);
    }
}
