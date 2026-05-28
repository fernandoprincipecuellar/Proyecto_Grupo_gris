using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByForumPostAsync(int forumPostId, int page, int pageSize);
    Task<IEnumerable<Comment>> GetByEcoRouteAsync(int ecoRouteId, int page, int pageSize);
}
