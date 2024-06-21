using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrabalhoFinalDwASPNET.Data;
using TrabalhoFinalDwASPNET.Data.Migrations;
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

        // GET: Events
        // Retorna a lista de eventos
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
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

            return View(events);
        }

        // GET: Events/Details/5
        // Retorna os detalhes de um evento específico
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var events = await _context.Events
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (events == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            bool isParticipating = false;

            if (user != null)
            {
                isParticipating = _context.Participants.Any(p => p.EventFK == id && p.UserFK == user.Id);
            }

            ViewBag.IsParticipating = isParticipating;

            return View(events);
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
                string userId = GetUserId();
                events.host_id = userId;
                events.created_at = DateTime.Now;

                if (events.end_time < events.start_time)
                {
                    ModelState.AddModelError("end_time", "A data de fim não pode ser menor que a de inicio");
                    return View(events);
                }

                try
                {
                    _context.Add(events);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(tags))
                    {
                        var tagList = tags.Split(',').Select(t => t.Trim()).Take(5).ToList();
                        foreach (var tagName in tagList)
                        {
                            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName) ?? new ModelsTags { Name = tagName };
                            if (tag.Id == 0)
                            {
                                _context.Tags.Add(tag);
                                await _context.SaveChangesAsync();
                            }

                            var eventTag = new EventTag { EventId = events.Id, TagId = tag.Id };
                            _context.EventTags.Add(eventTag);
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
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
            return View(events);
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

            var events = await _context.Events
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (events == null)
            {
                return NotFound();
            }

            if (GetUserId() != events.host_id)
            {
                return Unauthorized();
            }

            return View(events);
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

            if (GetUserId() != events.host_id)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Remove as tags antigas
                    var oldEventTags = _context.EventTags.Where(et => et.EventId == id);
                    _context.EventTags.RemoveRange(oldEventTags);

                    // Adiciona novas tags
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
                return RedirectToAction(nameof(Index));
            }
            return View(events);
        }

        // GET: Events/Delete/5
        // Retorna a view para deletar um evento
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var events = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (events == null)
            {
                return NotFound();
            }

            if (GetUserId() != events.host_id)
            {
                return Unauthorized();
            }

            return View(events);
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

            if (GetUserId() != events.host_id)
            {
                return Unauthorized();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

                    // Adiciona o participante ao banco de dados
                    _context.Participants.Add(participant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // Recupera os detalhes do evento para a view Detalhes do Evento
                var events = await _context.Events.FindAsync(eventId);

                // Retorna a view Detalhes do Evento com o modelo e a mensagem de erro (se houver)
                return View("Details", events);
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

            var participation = await _context.Participants
                .FirstOrDefaultAsync(p => p.EventFK == eventId && p.UserFK == user.Id);

            if (participation == null)
            {
                return NotFound("Você não está participando neste evento.");
            }

            _context.Participants.Remove(participation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Events", new { id = eventId });
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

            var events = await _context.Events
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .ToListAsync();

            // Popula a lista de participantes para cada evento
            foreach (var evnt in myEvents)
            {
                var participants = await _context.Participants
                    .Where(p => p.EventFK == evnt.Id)
                    .ToListAsync();

                evnt.ListaParticipants = participants;
            }

            return View(myEvents);
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
            var eventHosts = await _context.Participants
                .Where(e => e.UserFK == userId)
                .Select(e => e.EventFK)
                .ToListAsync();

            return eventHosts.Contains(eventId);
        }

        // GET: Events/EventsParticipating
        // Retorna os eventos em que o usuário está participando
        [HttpGet]
        public IActionResult EventsParticipating()
        {
            // Obtém o ID do usuário atualmente logado
            string userId = GetUserId();

            // Consulta o banco de dados para recuperar os eventos em que o usuário está participando
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

            return View(events);
        }
    }
}
