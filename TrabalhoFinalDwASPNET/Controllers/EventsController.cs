using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrabalhoFinalDwASPNET.Data;
using TrabalhoFinalDwASPNET.Models;

namespace TrabalhoFinalDwASPNET.Controllers
{
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
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {


            var events = _context.Events.ToList();

            foreach (var @event in events)
            {
                var participants = _context.Participants
                    .Where(p => p.EventFK == @event.Id)
                    .ToList();

                @event.listaParticipants = participants;
            }

            return View(events);

        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(events);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,host_id,created_at,title,Description,Image,start_time,end_time,location,is_private,maxParticipants")] Events events)
        {

            if (ModelState.IsValid)
            {
                string userId = GetUserId();

                events.host_id = userId;
                events.created_at = DateTime.Now;

                _context.Add(events);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(events);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var events = await _context.Events.FindAsync(id);
            if (events == null)
            {
                return NotFound();
            }
            return View(events);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,host_id,created_at,title,Description,Image,start_time,end_time,location,is_private,maxParticipants")] Events events)
        {
            if (id != events.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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

            return View(events);
        }

        // POST: Events/Delete/5
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
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventsExists(int id)
        {
          return (_context.Events?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Participate(int eventId)
        {
            // Get the ID of the currently logged-in user
            string userId = GetUserId();

            // Check if the user is the host of the event
            var eventHost = await _context.Events
                .Where(e => e.Id == eventId)
                .Select(e => e.host_id)
                .FirstOrDefaultAsync();

            if (userId == eventHost)
            {
                ModelState.AddModelError(string.Empty, "You cannot participate in your own event.");
                var eventDetails = await _context.Events.FindAsync(eventId);

                // Return Event Details view with the model and error message
                return View("Details", eventDetails);
            }
            else
            {
                // Check the number of participants for the event
                var participantsCount = await _context.Participants
                    .CountAsync(p => p.EventFK == eventId);

                // Get the maximum participants allowed for the event
                var eventDetails = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == eventId);

                if (participantsCount >= eventDetails.maxParticipants)
                {
                    // Participants limit reached, add error to ModelState
                    ModelState.AddModelError(string.Empty, "The maximum number of participants has been reached.");
                }
                else
                {
                    // Create a new participant
                    var participant = new Participants
                    {
                        UserFK = userId,
                        EventFK = eventId
                    };


                    // Add the participant to the database
                    _context.Participants.Add(participant);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                // Retrieve the event details for the Event Details view
                var events = await _context.Events.FindAsync(eventId);

                // Return Event Details view with the model and error message (if any)
                return View("Details", events);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            // Get the ID of the currently logged-in user
            string userId = GetUserId();

            // Retrieve the events created by the user
            var myEvents = await _context.Events
                .Where(e => e.host_id == userId)
                .ToListAsync();

            return View(myEvents);
        }

        public string GetUserId()
        {
            var userId = _userManager.GetUserId(User);
            return userId;
        }
    }
}
