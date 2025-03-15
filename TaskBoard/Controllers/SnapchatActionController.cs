using Microsoft.AspNetCore.Mvc;
using SnapWebModels;
using TaskBoard.Models;
using TaskBoard.Models.SnapchatActionModels;

namespace TaskBoard.Controllers;

[TypeFilter(typeof(CheckAccessDeadlineAttribute))]
[Route("api/[controller]")]
[ApiController]
public class SnapchatActionController : ApiController
{
    private const string UnauthMessage = "You are not authorized to use this action";
    private readonly ModuleEnabler _moduleEnabler;
    private readonly WorkScheduler _scheduler;
    private readonly UploadManager _uploadManager;

    public SnapchatActionController(WorkScheduler scheduler, ModuleEnabler moduleEnabler, UploadManager uploadManager)
    {
        _scheduler = scheduler;
        _moduleEnabler = moduleEnabler;
        _uploadManager = uploadManager;
    }

    // This is a good template for an action. They should state the route they will be accessed through the ajax api
    // and use an object for its arguments
    // An action needs handle the logic to perform the appropriate api call in the snapchat client and return Ok to
    // signal that the action finished properly
    // After adding an endpoint here, another piece of code needs to be added to the javascript api client to be able
    // to access this endpoint

    // POST: api/SnapchatAction/subscribe
    [HttpPost(nameof(Subscribe))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Subscribe(SubscribeArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.Subscribe)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        // Validate the arguments. BadRequest allows us to communicate back the error
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.Subscribe(arguments);

        return OkApi("Scheduled Subscribe Job", workRequest.Id);
    }
    
    // POST: api/SnapchatAction/findusersviasearch
    [HttpPost(nameof(FindUsersViaSearch))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> FindUsersViaSearch(FindUsersViaSearchArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.FindUsersViaSearch)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        // Validate the arguments. BadRequest allows us to communicate back the error
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.FindUsersViaSearch(arguments);

        return OkApi("Scheduled Find Users via Search Job", workRequest.Id);
    }
    
    // POST: api/SnapchatAction/phonetousername
    [HttpPost(nameof(PhoneToUsername))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> PhoneToUsername(PhoneSearchArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.PhoneScraper)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        // Validate the arguments. BadRequest allows us to communicate back the error
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.PhoneToUsername(arguments);

        return OkApi("Scheduled Find Users via Phone Search Job", workRequest.Id);
    }
    
    // POST: api/SnapchatAction/emailtousername
    [HttpPost(nameof(EmailToUsername))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> EmailToUsername(EmailSearchArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.EmailScraper)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        // Validate the arguments. BadRequest allows us to communicate back the error
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.EmailToUsername(arguments);

        return OkApi("Scheduled Find Users via E-Mail Search Job", workRequest.Id);
    }

    // POST: api/SnapchatAction/reportuserpublicprofilerandom
    [HttpPost(nameof(ReportUserPublicProfileRandom))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ReportUserPublicProfileRandom(ReportUserPublicProfileRandomArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.ReportUserPublicProfileRandom)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.ReportUserPublicProfileRandom(arguments);

        return OkApi("Scheduled ReportUserPublicProfileRandom Job", workRequest.Id);
    }

    // POST: api/SnapchatAction/reportuserrandom
    [HttpPost(nameof(ReportUserRandom))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ReportUserRandom(ReportUserRandomArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.ReportUserRandom)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.ReportUserRandom(arguments);

        return OkApi("Scheduled ReportUserRandom Job", workRequest.Id);
    }

    // POST: api/SnapchatAction/postdirect
    [HttpPost(nameof(PostDirect))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> PostDirect(PostDirectArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.PostDirect)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        foreach (var snap in arguments.Snaps)
        {
            if (snap.SecondsBeforeStart < 0)
                return BadRequestApi("Delay between snaps cannot be less than 0");
            
            var file = await _uploadManager.GetFile(snap.MediaFileId);
            if (file == null || !System.IO.File.Exists(file.ServerPath)) return BadRequestApi("Requested media file does not exist");
        }

        var workRequest = await _scheduler.PostDirect(arguments);

        return OkApi("Scheduled PostDirect Job", workRequest.Id);
    }

    [HttpPost(nameof(SendMessage))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SendMessage(SendMessageArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.SendMessage)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);
        
        foreach (var snap in arguments.Messages)
        {
            if (snap.SecondsBeforeStart < 0)
                return BadRequestApi("Delay between snaps cannot be less than 0");
        }
        
        var workRequest = await _scheduler.SendMessage(arguments);

        return OkApi("Scheduled SendMessage Job", workRequest.Id);
    }
    
    [HttpPost(nameof(SendMention))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SendMention(SendMentionArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.SendMessage)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.SendMention(arguments);

        return OkApi("Scheduled SendMention Job", workRequest.Id);
    }

    [HttpPost(nameof(ViewBusinessPublicStory))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ViewBusinessPublicStory(ViewBusinessPublicStoryArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.ViewBusinessPublicStory)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.ViewBusinessPublicStory(arguments);

        return OkApi("Scheduled ViewBusinessPublicStory Job", workRequest.Id);
    }
    
    [HttpPost(nameof(ReportUserStoryRandom))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ReportUserStoryRandom(ReportUserStoryRandomArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.ReportUserStoryRandom)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.ReportUserStoryRandom(arguments);

        return OkApi("Scheduled ReportUserStoryRandom Job", workRequest.Id);
    }
    
    [HttpPost(nameof(ViewPublicStory))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ViewPublicStory(ViewPublicStoryArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.ViewBusinessPublicStory)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.ViewPublicStory(arguments);

        return OkApi("Scheduled ViewPublicStory Job", workRequest.Id);
    }

    [HttpPost(nameof(AddFriend))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AddFriend(AddFriendArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.AddFriend)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.AddFriend(arguments);

        return OkApi("Scheduled AddFriend Job", workRequest.Id);
    }

    [HttpPost(nameof(Test))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Test(TestArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.Test)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var workRequest = await _scheduler.Test(arguments);

        return OkApi("Scheduled Test Job", workRequest.Id);
    }
    
    // POST: api/SnapchatAction/poststory
    [HttpPost(nameof(PostStory))]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> PostStory(PostStoryArguments arguments)
    {
        if (!await _moduleEnabler.IsEnabled(SnapWebModuleId.PostStory)) return UnauthorizedApi(UnauthMessage, null, ApiResponseCode.Unauthorized);
        var validationResult = arguments.Validate();
        if (validationResult.Exception != null) return BadRequestApi("Arguments validation failed.", validationResult.Exception);

        var file = await _uploadManager.GetFile(arguments.MediaFileId);
        if (file == null || !System.IO.File.Exists(file.ServerPath)) return Problem("Requested media file does not exist");
        
        var workRequest = await _scheduler.PostStory(arguments, file);

        return OkApi("Scheduled PostStory Job", workRequest.Id);
    }
}