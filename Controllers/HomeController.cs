using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Proyecto_Grupo_gris.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public HomeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, IWebHostEnvironment env, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _env = env;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        int puntos = 2450;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null) puntos = user.Puntos;
        }

        var model = new DashboardViewModel
        {
            ReciclajeTotalKg = 124,
            Crecimiento = "+15%",
            Entregas = 48,
            Puntos = puntos,
            RecolectorNombre = "Camión Recolector",
            RecolectorDireccion = "Calle Principal 123",
            Distancia = "A 500 Metros de tu hogar",
            Co2AhoradoKg = 89,
            ArbolesSalvados = 12,
            AguaAhorradaL = 340,
        };

        return View(model);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Premios()
    {
        var user = await _userManager.GetUserAsync(User);
        
        ViewBag.UserName = user?.Nombre ?? user?.Email ?? "Usuario";
        ViewBag.Puntos = user?.Puntos ?? 0;
        
        return View();
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult EditProfile()
    {
        return View();
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> EditProfile(string fullName, string pictureUrl, string phone, string city, string bio)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var claims = await _userManager.GetClaimsAsync(user);
        
        async Task UpdateClaim(string type, string value)
        {
            var claim = claims.FirstOrDefault(c => c.Type == type);
            if (claim != null) await _userManager.RemoveClaimAsync(user, claim);
            if (!string.IsNullOrEmpty(value)) await _userManager.AddClaimAsync(user, new Claim(type, value));
        }

        await UpdateClaim("FullName", fullName);
        await UpdateClaim("Picture", pictureUrl);
        await UpdateClaim("Phone", phone);
        await UpdateClaim("City", city);
        await UpdateClaim("Bio", bio);

        await _signInManager.RefreshSignInAsync(user);
        
        return RedirectToAction("Profile");
    }

    private static int _routeIndex = 0;
    private static double _totalKg = 124.0;
    private static int _entregas = 48;
    private static readonly List<(double Lat, double Lng, string Dir)> _routePoints = new List<(double, double, string)>
    {
        (-12.0464, -77.0428, "Cerca a Plaza de Armas"),
        (-12.0480, -77.0410, "Jr. de la Unión"),
        (-12.0510, -77.0430, "Av. Tacna"),
        (-12.0550, -77.0460, "Av. Wilson"),
        (-12.0520, -77.0500, "Av. Uruguay"),
        (-12.0490, -77.0530, "Av. Alfonso Ugarte"),
        (-12.0460, -77.0500, "Jr. Quilca")
    };

    [Route("api/camion")]
    [HttpGet]
    public IActionResult GetCamionUbicacion()
    {
        var random = new Random();
        var point = _routePoints[_routeIndex];
        _routeIndex = (_routeIndex + 1) % _routePoints.Count;

        // Simulamos que el peso fluctúa (puede subir o bajar)
        _totalKg += (random.NextDouble() * 2.0 - 1.0); // Entre -1.0 y +1.0 kg
        if (_totalKg < 10) _totalKg = 10; // Evitar que sea negativo

        if (_routeIndex % 2 == 0) _entregas += 1;

        var dist = 600 - (_routeIndex * 50); // Simular que se acerca

        return Json(new {
            lat = point.Lat,
            lng = point.Lng,
            nombre = "Camión Recolector",
            distancia = $"A {Math.Max(100, dist)} metros",
            direccion = point.Dir
        });
    }

    [Route("api/stats")]
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        int puntos = 2488;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null) puntos = user.Puntos;
        }

        return Json(new {
            totalKg = Math.Round(_totalKg, 1),
            entregas = _entregas,
            puntos = puntos,
            co2 = 89 + (int)(_totalKg - 124),
            arboles = 12,
            agua = 340 + (int)(_totalKg - 124) * 5
        });
    }

    [Route("api/news")]
    [HttpGet]
    public async Task<IActionResult> GetEnvironmentNews()
    {
        // Try cache first (5 minutes)
        var cacheKey = "news:environment:peru";
        try
        {
            var cachedBytes = await _cache.GetAsync(cacheKey);
            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                var cached = System.Text.Encoding.UTF8.GetString(cachedBytes);
                _logger?.LogInformation("Returning cached news items (key={key})", cacheKey);
                return Content(cached, "application/json");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error reading cache for news");
        }

        // Fuentes RSS: priorizar medios de noticias peruanos
        var feeds = new[]
        {
            // Usar la página raíz para permitir auto-descubrimiento de RSS
            "https://elcomercio.pe/",
            "https://larepublica.pe/",
            "https://rpp.pe/",
            "https://andina.pe/",
            "https://gestion.pe/",
            "https://peru21.pe/",
            "https://elperuano.pe/",
            // Google News search RSS (Perú, medio ambiente) as fallback aggregator
            "https://news.google.com/rss/search?q=medio+ambiente+site:pe&hl=es-PE&gl=PE&ceid=PE:es"
        };

        var client = _httpClientFactory.CreateClient();
        var items = new List<object>();

        // Palabras clave ambientales para filtrar titulares irrelevantes
        var envKeywords = new[] {
            // English
            "environment", "environmental", "climate", "climate change", "pollution",
            "recycle", "recycling", "biodiversity", "conservation", "wildlife",
            "ocean", "sea", "marine", "plastic", "emissions", "forest",
            "deforestation", "renewable", "sustainability", "ecosystem", "air",
            "water", "river", "soil", "habitat", "sea level", "glacier", "ice",
            // Spanish
            "medio ambiente", "ambiental", "clima", "cambio climático", "contaminación",
            "reciclaje", "biodiversidad", "conservación", "fauna", "flora",
            "océano", "mar", "marino", "plástico", "emisiones", "bosque",
            "deforestación", "renovable", "sostenibilidad", "ecosistema", "aire",
            "agua", "río", "suelo", "hábitat", "nivel del mar", "glaciar"
        };

        foreach (var feed in feeds)
        {
            try
            {
                string? resp = null;
                int attempts = 0;
                while (attempts < 3 && resp == null)
                {
                    attempts++;
                    try
                    {
                        using var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, feed);
                        req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) EcoRuta/1.0");
                        req.Headers.Accept.ParseAdd("application/rss+xml, application/xml, text/xml, text/html;q=0.9");

                        using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(8));
                        var respMsg = await client.SendAsync(req, cts.Token);
                        if (respMsg.IsSuccessStatusCode)
                        {
                            resp = await respMsg.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            _logger?.LogInformation("Feed {feed} responded {status} on attempt {attempt}", feed, respMsg.StatusCode, attempts);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Error fetching feed {feed} attempt {attempt}", feed, attempts);
                        await Task.Delay(300 * attempts);
                    }
                }

                if (string.IsNullOrEmpty(resp))
                {
                    _logger?.LogInformation("No response from feed {feed}, attempting auto-discovery", feed);
                    // Try to discover RSS from site root
                    try
                    {
                        var uri = new Uri(feed);
                        var root = uri.Scheme + "://" + uri.Host + "/";
                        string rootHtml = null;
                        try
                        {
                            using var reqRoot = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, root);
                            reqRoot.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) EcoRuta/1.0");
                            using var ctsRoot = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(6));
                            var rootResp = await client.SendAsync(reqRoot, ctsRoot.Token);
                            if (rootResp.IsSuccessStatusCode) rootHtml = await rootResp.Content.ReadAsStringAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogDebug(ex, "Error fetching root for discovery {root}", root);
                        }

                        if (!string.IsNullOrEmpty(rootHtml))
                        {
                            var m = System.Text.RegularExpressions.Regex.Match(rootHtml, "<link[^>]+type=[\"']application/rss\\+xml[\"'][^>]*href=[\"']([^\"']+)[\"']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (!m.Success)
                                m = System.Text.RegularExpressions.Regex.Match(rootHtml, "<link[^>]+type=[\"']application/rss\\+xml[\"'][^>]*href=[\"']([^\"']+)[\"']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            if (m.Success)
                            {
                                var discovered = m.Groups[1].Value;
                                if (discovered.StartsWith("/")) discovered = uri.Scheme + "://" + uri.Host + discovered;
                                if (!discovered.StartsWith("http", StringComparison.OrdinalIgnoreCase)) discovered = uri.Scheme + "://" + uri.Host + "/" + discovered;
                                _logger?.LogInformation("Discovered feed for {host}: {feed}", uri.Host, discovered);
                                try
                                {
                                    using var req2 = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, discovered);
                                    req2.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) EcoRuta/1.0");
                                    using var cts2 = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(8));
                                    var r2 = await client.SendAsync(req2, cts2.Token);
                                    if (r2.IsSuccessStatusCode) resp = await r2.Content.ReadAsStringAsync();
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogDebug(ex, "Error fetching discovered feed {discovered}", discovered);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Discovery failed for feed {feed}", feed);
                    }

                    if (string.IsNullOrEmpty(resp)) { _logger?.LogInformation("No response after discovery for feed {feed}", feed); continue; }
                }

                var doc = System.Xml.Linq.XDocument.Parse(resp);
                var xmlItems = doc.Descendants("item").Take(6);
                foreach (var it in xmlItems)
                {
                    var title = (string?)it.Element("title") ?? string.Empty;
                    var link = (string?)it.Element("link") ?? string.Empty;
                    var desc = (string?)it.Element("description") ?? string.Empty;
                    string? imageUrl = null;
                    var pub = (string?)it.Element("pubDate");
                    DateTime? pubDate = null;
                    if (DateTime.TryParse(pub, out var dt)) pubDate = dt.ToUniversalTime();

                    // Intentar extraer imagen: media:content, media:thumbnail, enclosure, o <img> en description
                    try
                    {
                        var media = System.Xml.Linq.XNamespace.Get("http://search.yahoo.com/mrss/");
                        var mediaContent = it.Elements(media + "content").FirstOrDefault();
                        if (mediaContent != null)
                            imageUrl = (string?)mediaContent.Attribute("url");

                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            var thumb = it.Elements(media + "thumbnail").FirstOrDefault();
                            if (thumb != null) imageUrl = (string?)thumb.Attribute("url");
                        }

                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            var enclosure = it.Element("enclosure");
                            if (enclosure != null) imageUrl = (string?)enclosure.Attribute("url");
                        }

                        if (string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(desc))
                        {
                            // Buscar primera imagen en el HTML de la descripción
                            var m = System.Text.RegularExpressions.Regex.Match(desc, "<img[^>]+src=\\\"([^\\\"]+)\\\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (!m.Success)
                                m = System.Text.RegularExpressions.Regex.Match(desc, "<img[^>]+src=\\\'([^\\\']+)\\\'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (m.Success) imageUrl = m.Groups[1].Value;
                        }
                    }
                    catch { imageUrl = null; }

                    // normalize protocol-relative urls
                    if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("//")) imageUrl = "https:" + imageUrl;

                    // Filtrar por palabras clave ambientales en título o descripción (case-insensitive)
                    var textToCheck = (title + " \n " + desc).ToLowerInvariant();
                    bool matched = envKeywords.Any(k => textToCheck.Contains(k, StringComparison.OrdinalIgnoreCase));
                    if (matched)
                    {
                        _logger?.LogInformation("Feed {feed} matched item: {title}", feed, title);
                        // Filtrar por Perú: buscar 'Peru', 'Perú' o dominios/periódicos peruanos
                        var peruvianDomains = new[] { "elcomercio", "andina", "rpp", "peru21", "larepublica", "elperuano", "gestión", "gestion", "ojo-publico", "ojo" };
                        var peruvianPlaces = new[] { "lima", "arequipa", "cusco", "ica", "piura", "trujillo", "huancayo", "huanuco", "puno", "tacna", "ancash", "amazonas", "san martin", "ucayali", "loreto", "ucayali" };

                        bool isPeru = false;
                        if (!string.IsNullOrEmpty(link) && peruvianDomains.Any(d => link.ToLowerInvariant().Contains(d))) isPeru = true;
                        if (!isPeru && (textToCheck.Contains("peru") || textToCheck.Contains("perú"))) isPeru = true;
                        if (!isPeru)
                        {
                            // check host of feed
                            try {
                                var uri = new Uri(link);
                                var host = uri.Host.ToLowerInvariant();
                                if (peruvianDomains.Any(d => host.Contains(d))) isPeru = true;
                            } catch { }
                        }

                        if (!isPeru)
                        {
                            // fallback: try to infer from source URL
                            if (!string.IsNullOrEmpty(feed) && peruvianDomains.Any(d => feed.ToLowerInvariant().Contains(d))) isPeru = true;
                        }

                        if (!isPeru) continue; // skip non-Peru news

                        // Try to extract a place/zone from title/description
                        string? location = null;
                        foreach (var place in peruvianPlaces)
                        {
                            if (textToCheck.Contains(place)) { location = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(place); break; }
                        }
                        if (string.IsNullOrEmpty(location)) location = "Perú";

                        items.Add(new { title, link, description = desc, pubDate, source = feed, image = imageUrl, location });
                    }
                }
                _logger?.LogInformation("Processed feed {feed}, collected {count} candidate items", feed, items.Count);
            }

            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error processing feed {feed}", feed);
                // Ignorar errores individuales de feed
            }
        }

        _logger?.LogInformation("Total filtered items count: {count}", items.Count);

        // Ordenar por fecha si existe y devolver top 6
        var ordered = items
            .Where(x => ((DateTime?)((dynamic)x).pubDate) != null)
            .OrderByDescending(x => ((dynamic)x).pubDate)
            .Cast<object>()
            .Take(6)
            .ToList();

        // Si no hay items filtrados (por el filtro estricto), caer de vuelta a devolver artículos sin filtrar (top 6)
            if (ordered.Count == 0)
        {
            // intentar devolver algunos items sin filtrar (leer nuevamente sin filtro)
            var rawItems = new List<object>();
            try
            {
                // Simple re-fetch: recorrer feeds y recoger primeros items (sin filtrar)
                foreach (var feed in feeds)
                {
                    var resp = await client.GetStringAsync(feed);
                    var doc = System.Xml.Linq.XDocument.Parse(resp);
                    var xmlItems = doc.Descendants("item").Take(6);
                    foreach (var it in xmlItems)
                    {
                        var title = (string?)it.Element("title") ?? string.Empty;
                        var link = (string?)it.Element("link") ?? string.Empty;
                        var desc = (string?)it.Element("description") ?? string.Empty;
                        var pub = (string?)it.Element("pubDate");
                        DateTime? pubDate = null;
                        if (DateTime.TryParse(pub, out var dt)) pubDate = dt.ToUniversalTime();
                        string? imageUrl = null;
                        try
                        {
                            var media = System.Xml.Linq.XNamespace.Get("http://search.yahoo.com/mrss/");
                            var mediaContent = it.Elements(media + "content").FirstOrDefault();
                            if (mediaContent != null) imageUrl = (string?)mediaContent.Attribute("url");
                            if (string.IsNullOrEmpty(imageUrl))
                            {
                                var thumb = it.Elements(media + "thumbnail").FirstOrDefault();
                                if (thumb != null) imageUrl = (string?)thumb.Attribute("url");
                            }
                            if (string.IsNullOrEmpty(imageUrl))
                            {
                                var enclosure = it.Element("enclosure");
                                if (enclosure != null) imageUrl = (string?)enclosure.Attribute("url");
                            }
                            if (string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(desc))
                            {
                                var m = System.Text.RegularExpressions.Regex.Match(desc, "<img[^>]+src=\\\"([^\\\"]+)\\\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (!m.Success)
                                    m = System.Text.RegularExpressions.Regex.Match(desc, "<img[^>]+src=\\\'([^\\\']+)\\\'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                if (m.Success) imageUrl = m.Groups[1].Value;
                            }
                        }
                        catch { imageUrl = null; }

                        rawItems.Add(new { title, link, description = desc, pubDate, source = feed, image = imageUrl });
                    }
                }
            }
            catch { }

            ordered = rawItems.Take(6).ToList();
            _logger?.LogInformation("Falling back to raw items, count: {count}", ordered.Count);
        }

        // If still empty and in development, return a small sample for UI testing
        if (ordered.Count == 0 && _env != null && _env.EnvironmentName == "Development")
        {
            _logger?.LogInformation("Returning development sample news items as fallback.");
            ordered = new List<object>
            {
                new { title = "Simulación: Limpieza comunitaria en Lima Norte", link = "#", description = "Evento local de limpieza de playa", pubDate = DateTime.UtcNow, source = "Simulado", image = (string?)null, location = "Lima" },
                new { title = "Simulación: Reforestación en Arequipa", link = "#", description = "Plantación de árboles", pubDate = DateTime.UtcNow.AddMinutes(-30), source = "Simulado", image = (string?)null, location = "Arequipa" }
            };
        }

        // Serialize and cache result
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(ordered);
            try
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                await _cache.SetAsync(cacheKey, bytes, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error caching news result");
            }

            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error serializing news items, returning as JsonResult");
            return Json(ordered);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
