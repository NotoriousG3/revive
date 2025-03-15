using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Models;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[ApiController]
[Route("api/[controller]")]
public class AppSettingsController : ApiController
{
    private readonly ApplicationDbContext _context;

    public AppSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/GetSettings
    [HttpGet]
    public async Task<ActionResult<UserSettings>> Get()
    {
        if (_context.AppSettings == null) return NotFound();

        if (!_context.AppSettings.Any()) return NotFound();

        var settings = await _context.AppSettings.FirstAsync();
        return OkApi("", settings.ToUserSettings());
    }

    // POST: api/AppSettings
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<UserSettings>> Save(UserSettings incomingSettings)
    {
        if (_context.AppSettings == null) return Problem("Entity set 'ApplicationDbContext.AppSettings'  is null.");

        var settings = _context.AppSettings.Any() ? await _context.AppSettings.FirstAsync() : null;

        if (settings == null)
        {
            var newSettings = new AppSettings
            {
                Threads = 10,
                MaxTasks = 1,
                MaxRegisterAttempts = 10,
                EnableDebug = false,
                EnableBandwidthSaver = incomingSettings.EnableBandwidthSaver,
                EnableWebRegister = incomingSettings.EnableWebRegister,
                Timeout = incomingSettings.Timeout
            };
            settings = _context.AppSettings.Add(newSettings).Entity;
        }
        else
        {
            settings.Timeout = incomingSettings.Timeout;
            settings.EnableDebug = false;
            settings.EnableBandwidthSaver = incomingSettings.EnableBandwidthSaver;
            settings.FiveSimApiKey = incomingSettings.FiveSimApiKey;
            settings.SmsPoolApiKey = incomingSettings.SmsPoolApiKey;
            settings.NamsorApiKey = incomingSettings.NamsorApiKey;
            settings.KopeechkaApiKey = incomingSettings.KopeechkaApiKey;
            settings.TwilioApiKey = incomingSettings.TwilioApiKey;
            settings.TextVerifiedApiKey = incomingSettings.TextVerifiedApiKey;
            settings.SmsActivateApiKey = incomingSettings.SmsActivateApiKey;
            settings.ProxyScraping = incomingSettings.ProxyScraping;
            settings.ProxyChecking = incomingSettings.ProxyChecking;
            settings.EnableStealth = incomingSettings.EnableStealth;
            settings.MaxRetries = incomingSettings.MaxRetries;
            settings.EnableWebRegister = incomingSettings.EnableWebRegister;

            _context.Entry(settings).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();

        return OkApi("", settings.ToUserSettings());
    }
}