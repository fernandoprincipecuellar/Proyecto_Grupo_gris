namespace Proyecto_Grupo_gris.Services.Agents.Interfaces
{
    public interface IAgentService
    {
        Task<string> ChatAsync(string userMessage);
        Task ClearHistoryAsync();
    }
}
