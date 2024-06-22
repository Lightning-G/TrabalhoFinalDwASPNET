using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabalhoFinalDwASPNET.Data;
using TrabalhoFinalDwASPNET.Models;

namespace TrabalhoFinalDwASPNET.Controllers
{
    using ModelsTags = TrabalhoFinalDwASPNET.Models.Tags;

    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Events/Index
        // Retorna a lista de eventos
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            // Recupera todos os eventos da base de dados, incluindo suas tags e participantes
            var events = await _context.Events
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .ToListAsync();

            // Popula a lista de participantes para cada evento
            foreach (var @event in events)
            {
                var participants = await _context.Participants
                    .Where(p => p.EventFK == @event.Id)
                    .ToListAsync();

                @event.ListaParticipants = participants;
            }

            return View(events); // Retorna a view Index com a lista de eventos
        }

        // GET: Events/Details/5
        // Retorna os detalhes de um evento específico
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            // Recupera o evento com o ID fornecido, incluindo suas tags
            var events = await _context.Events
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (events == null)
            {
                return NotFound();
            }

            // Verifica se o usuário atual está participando deste evento
            var user = await _userManager.GetUserAsync(User);
            bool isParticipating = false;

            if (user != null)
            {
                isParticipating = _context.Participants.Any(p => p.EventFK == id && p.UserFK == user.Id);
            }

            // Define ViewBag para indicar se o usuário está participando deste evento
            ViewBag.IsParticipating = isParticipating;

            return View(events); // Retorna a view Details com os detalhes do evento
        }

        // GET: Events/Create
        // Retorna a view para criar um novo evento
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // Cria um novo evento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,host_id,created_at,title,Description,Image,start_time,end_time,location,is_private,maxParticipants")] Events events, string tags)
        {
            if (ModelState.IsValid)
            {
                string userId = GetUserId(); // Obtém o ID do usuário atual
                events.host_id = userId; // Define o ID do host do evento como o ID do usuário atual
                events.created_at = DateTime.Now; // Define a data de criação do evento como a data atual

                // Verifica se a data de término do evento é anterior à data de início
                if (events.end_time < events.start_time)
                {
                    ModelState.AddModelError("end_time", "A data de fim não pode ser menor que a de início");
                    return View(events);
                }

                try
                {
                    // Adiciona o evento ao contexto e salva na base de dados
                    _context.Add(events);
                    await _context.SaveChangesAsync();

                    // Adiciona as tags ao evento, se fornecidas
                    if (!string.IsNullOrEmpty(tags))
                    {
                        var tagList = tags.Split(',').Select(t => t.Trim()).Take(5).ToList();
                        foreach (var tagName in tagList)
                        {
                            // Procura a tag na base de dados ou cria uma nova se não existir
                            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName) ?? new ModelsTags { Name = tagName };
                            if (tag.Id == 0)
                            {
                                _context.Tags.Add(tag);
                                await _context.SaveChangesAsync();
                            }

                            // Cria uma associação entre o evento e a tag
                            var eventTag = new EventTag { EventId = events.Id, TagId = tag.Id };
                            _context.EventTags.Add(eventTag);
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index)); // Redireciona para a página Index de eventos
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Um erro ocorreu ao criar o evento. Por favor tente novamente.");
                    // Log the exception here for further analysis
                }
            }
            else
            {
                // Log the model errors for further analysis
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        // Log the error message
                        var errorMessage = error.ErrorMessage;
                    }
                }
            }

            return View(events); // Retorna a view Create com o modelo do evento (e erros de validação, se houver)
        }

        // GET: Events/Edit/5
        // Retorna a view para editar um evento
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            // Recupera o evento com o ID fornecido, incluindo suas tags
            var events = await _context.Events
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (events == null)
            {
                return NotFound();
            }

            // Verifica se o usuário atual é o host do evento
            if (GetUserId() != events.host_id)
            {
                return Unauthorized(); // Retorna status 401 Unauthorized se o usuário não for o host
            }

            return View(events); // Retorna a view Edit com o modelo do evento
        }

        // POST: Events/Edit/5
        // Edita um evento existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,host_id,created_at,title,Description,Image,start_time,end_time,location,is_private,maxParticipants")] Events events, List<string> tags)
        {
            if (id != events.Id)
            {
                return NotFound();
            }

            // Verifica se o usuário atual é o host do evento
            if (GetUserId() != events.host_id)
            {
                return Unauthorized(); // Retorna status 401 Unauthorized se o usuário não for o host
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Remove as tags antigas do evento
                    var oldEventTags = _context.EventTags.Where(et => et.EventId == id);
                    _context.EventTags.RemoveRange(oldEventTags);

                    // Adiciona novas tags ao evento
                    foreach (var tagName in tags ?? new List<string>())
                    {
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                        if (tag == null)
                        {
                            tag = new ModelsTags { Name = tagName };
                            _context.Tags.Add(tag);
                            await _context.SaveChangesAsync();
                        }

                        var eventTag = new EventTag { EventId = events.Id, TagId = tag.Id };
                        _context.EventTags.Add(eventTag);
                    }

                    // Atualiza o evento na base de dados
                    _context.Update(events);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventsExists(events.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index)); // Redireciona para a página Index de eventos
            }

            return View(events); // Retorna a view Edit com o modelo do evento (e erros de validação, se houver)
        }

        // GET: Events/Delete/5
        // Retorna a view para deletar um evento
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            // Recupera o evento com o ID fornecido
            var events = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (events == null)
            {
                return NotFound();
            }

            // Verifica se o usuário atual é o host do evento
            if (GetUserId() != events.host_id)
            {
                return Unauthorized(); // Retorna status 401 Unauthorized se o usuário não for o host
            }

            return View(events); // Retorna a view Delete com o modelo do evento
        }

        // POST: Events/Delete/5
        // Deleta um evento confirmado
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Events'  is null.");
            }
            var events = await _context.Events.FindAsync(id);
            if (events != null)
            {
                _context.Events.Remove(events);
            }

            // Verifica se o usuário atual é o host do evento
            if (GetUserId() != events.host_id)
            {
                return Unauthorized(); // Retorna status 401 Unauthorized se o usuário não for o host
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Redireciona para a página Index de eventos
        }

        // Verifica se um evento existe
        private bool EventsExists(int id)
        {
            return (_context.Events?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Participar de um evento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Participate(int eventId)
        {
            // Obtém o ID do usuário atualmente logado
            string userId = GetUserId();

            // Verifica se o usuário é o anfitrião do evento
            var eventHost = await _context.Events
                .Where(e => e.Id == eventId)
                .Select(e => e.host_id)
                .FirstOrDefaultAsync();

            var eventD = await _context.Events.FindAsync(eventId);

            // Verifica se o usuário já está participando no evento
            if (IsParticipating(eventId).Result)
            {
                ModelState.AddModelError(string.Empty, "Você já está participando neste evento.");

                // Retorna a view Detalhes do Evento com o modelo e a mensagem de erro
                return View("Details", eventD);
            }
            else if (DateTime.Now > eventD.start_time)
            {
                ModelState.AddModelError(string.Empty, "Este evento já começou. A participação não está mais disponível.");

                // Retorna a view Detalhes do Evento com o modelo e a mensagem de erro
                return View("Details", eventD);
            }
            else if (userId == eventHost)
            {
                ModelState.AddModelError(string.Empty, "Você não pode participar no seu próprio evento.");

                // Retorna a view Detalhes do Evento com o modelo e a mensagem de erro
                return View("Details", eventD);
            }
            else
            {
                // Verifica se o evento é privado
                if (eventD.is_private)
                {
                    ModelState.AddModelError(string.Empty, "Você não pode participar em eventos privados.");

                    // Retorna a view Detalhes do Evento com o modelo e a mensagem de erro
                    return View("Details", eventD);
                }

                // Verifica o número de participantes do evento
                var participantsCount = await _context.Participants
                    .CountAsync(p => p.EventFK == eventId);

                // Obtém o número máximo de participantes permitidos para o evento
                var eventDetails = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (participantsCount >= eventDetails.maxParticipants)
                {
                    // Limite de participantes atingido, adiciona erro ao ModelState
                    ModelState.AddModelError(string.Empty, "O número máximo de participantes foi atingido.");
                }
                else
                {
                    // Cria um novo participante
                    var participant = new Participants
                    {
                        UserFK = userId,
                        EventFK = eventId
                    };

                    // Adiciona o participante na base de dados
                    _context.Participants.Add(participant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index)); // Redireciona para a página Index de eventos
                }

                // Retorna a view Detalhes do Evento com o modelo e a mensagem de erro (se houver)
                return View("Details", eventD);
            }
        }

        // Desparticipar de um evento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desparticipate(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Verifica se o usuário está participando no evento
            var participation = await _context.Participants
                .FirstOrDefaultAsync(p => p.EventFK == eventId && p.UserFK == user.Id);

            if (participation == null)
            {
                return NotFound("Você não está participando neste evento.");
            }

            // Remove a participação do usuário no evento
            _context.Participants.Remove(participation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Events", new { id = eventId }); // Redireciona para a página de Detalhes do Evento
        }

        // GET: Events/MyEvents
        // Retorna os eventos criados pelo usuário atualmente logado
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            // Obtém o ID do usuário atualmente logado
            string userId = GetUserId();

            // Recupera os eventos criados pelo usuário
            var myEvents = await _context.Events
                .Where(e => e.host_id == userId)
                .ToListAsync();

            // Popula a lista de participantes para cada evento
            foreach (var evnt in myEvents)
            {
                var participants = await _context.Participants
                    .Where(p => p.EventFK == evnt.Id)
                    .ToListAsync();

                evnt.ListaParticipants = participants;
            }

            return View(myEvents); // Retorna a view MyEvents com os eventos criados pelo usuário atual
        }

        // Obtém o ID do usuário atualmente logado
        public string GetUserId()
        {
            var userId = _userManager.GetUserId(User);
            return userId;
        }

        // Verifica se o usuário está participando de um evento específico
        public async Task<bool> IsParticipating(int eventId)
        {
            var userId = _userManager.GetUserId(User);

            // Obtém todos os eventos nos quais o usuário está participando
            var eventHosts = await _context.Participants
                .Where(e => e.UserFK == userId)
                .Select(e => e.EventFK)
                .ToListAsync();

            return eventHosts.Contains(eventId); // Retorna verdadeiro se o usuário estiver participando no evento com o ID fornecido
        }

        // GET: Events/EventsParticipating
        // Retorna os eventos em que o usuário está participando
        [HttpGet]
        public IActionResult EventsParticipating()
        {
            // Obtém o ID do usuário atualmente logado
            string userId = GetUserId();

            // Consulta a base de dados para recuperar os eventos em que o usuário está participando
            var events = _context.Participants
                .Where(p => p.UserFK == userId)
                .Select(p => p.Event)
                .ToList();

            // Popula a lista de participantes para cada evento
            foreach (var evnt in events)
            {
                evnt.ListaParticipants = _context.Participants
                    .Where(p => p.EventFK == evnt.Id)
                    .ToList();
            }

            return View(events); // Retorna a view EventsParticipating com os eventos em que o usuário está participando
        }
    }
}
