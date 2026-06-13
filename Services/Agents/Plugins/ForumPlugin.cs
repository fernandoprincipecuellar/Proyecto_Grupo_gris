using System.Text.Json;
using Microsoft.SemanticKernel;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;

namespace Proyecto_Grupo_gris.Services.Agents.Plugins
{
    public class ForumPlugin
    {
        private readonly IForumPostRepository _forumRepository;
        private readonly ICommentRepository _commentRepository;

        public ForumPlugin(IForumPostRepository forumRepository, ICommentRepository commentRepository)
        {
            _forumRepository = forumRepository;
            _commentRepository = commentRepository;
        }

        [KernelFunction]
        public async Task<string> SearchForumPostsAsync(string query)
        {
            try
            {
                var posts = await _forumRepository.GetPagedAsync(1, 10, query);
                var results = posts.Select(p => new
                {
                    id = p.Id,
                    tipo = p.ReportType,
                    descripcion = p.Description,
                    ubicacion = p.Location,
                    urgencia = p.Urgency,
                    fecha = p.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    likes = p.LikesCount,
                    comentarios = p.CommentsCount,
                    autor = p.User?.UserName ?? "Anónimo"
                });

                return JsonSerializer.Serialize(new
                {
                    busqueda = query,
                    total = results.Count(),
                    posts = results
                });
            }
            catch (Exception ex)
            {
                return $"Error al buscar en el foro: {ex.Message}";
            }
        }

        [KernelFunction]
        public async Task<string> GetRecentPostsAsync(int count = 5)
        {
            try
            {
                var posts = await _forumRepository.GetPagedAsync(1, count, null);
                var results = posts.Select(p => new
                {
                    id = p.Id,
                    tipo = p.ReportType,
                    descripcion = p.Description,
                    ubicacion = p.Location,
                    fecha = p.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    likes = p.LikesCount,
                    autor = p.User?.UserName ?? "Anónimo"
                });

                return JsonSerializer.Serialize(new
                {
                    total = results.Count(),
                    posts = results
                });
            }
            catch (Exception ex)
            {
                return $"Error al obtener posts recientes: {ex.Message}";
            }
        }

        [KernelFunction]
        public async Task<string> GetPostCommentsAsync(int postId)
        {
            try
            {
                var comments = await _commentRepository.GetByForumPostAsync(postId, 1, 20);
                var results = comments.Select(c => new
                {
                    id = c.Id,
                    contenido = c.Content,
                    fecha = c.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    autor = c.Author?.UserName ?? "Anónimo"
                });

                return JsonSerializer.Serialize(new
                {
                    post_id = postId,
                    total = results.Count(),
                    comentarios = results
                });
            }
            catch (Exception ex)
            {
                return $"Error al obtener comentarios del post {postId}: {ex.Message}";
            }
        }

        [KernelFunction]
        public async Task<string> GetTopPostsAsync()
        {
            try
            {
                var allPosts = await _forumRepository.GetAllAsync();
                var topPosts = allPosts
                    .OrderByDescending(p => p.LikesCount)
                    .Take(5)
                    .Select(p => new
                    {
                        id = p.Id,
                        tipo = p.ReportType,
                        descripcion = p.Description,
                        ubicacion = p.Location,
                        likes = p.LikesCount,
                        comentarios = p.CommentsCount,
                        autor = p.User?.UserName ?? "Anónimo"
                    });

                return JsonSerializer.Serialize(new
                {
                    posts_populares = topPosts
                });
            }
            catch (Exception ex)
            {
                return $"Error al obtener posts populares: {ex.Message}";
            }
        }
    }
}
