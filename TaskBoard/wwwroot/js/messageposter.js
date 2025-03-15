function GetTargetUsers(selector) {
    return $(selector).val().split(',').filter(e => e !== undefined && e.length > 0).map(e => e.trim());
}

async function PostDirect(button) {
    await BlockingButtonAction(button, async () => {
		let randomUsers = $('#postDirect_RandomTargets').is(":checked") ? "true" : "false";
		let friendsOnly = $('#postDirect_FriendsOnly').is(":checked") ? "true" : "false";
        let postRandomTargetAmount = $('#postDirectTargetAmount').val();
        let users = GetTargetUsers('#postDirect_users');
		let countryFilter = $('#countryFilter').val();
		let raceFilter = $('#raceFilter').val();
		let genderFilter = $('#genderFilter').val();
        let rotateLinkPer = $('#postDirect_RotateAmount').val();
        
        let snaps = [];
        // gather the array of snaps to send
        $('.snapData').each((_, el) => {
           let mediaId = $(el).find('select').find('option:selected').val();
           let swipeUpUrl = $(el).find('input[name="swipeUpUrl"]').val();
           let secondsDelay = $(el).find('input[name="snapDelay"]').val();
           
           snaps.push({ MediaFileId: mediaId, PostDirectSwipeUpUrl: swipeUpUrl == "" ? null : swipeUpUrl, SecondsBeforeStart: secondsDelay });
        });
        
        let args = CreateActionArguments({
            Snaps: snaps,
            Users: users, 
            RandomUsers: randomUsers, 
            FriendsOnly: friendsOnly, 
            RandomTargetAmount: postRandomTargetAmount,
			CountryFilter: countryFilter,
			RaceFilter: raceFilter,
			GenderFilter: genderFilter,
            RotateLinkEvery: rotateLinkPer
        });

        try {
            let response = await api.PostDirect(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function SendMessage(button) {
    await BlockingButtonAction(button, async () => {
		let randomUsers = $('#sendMessage_RandomTargets').is(":checked") ? "true" : "false";
        let friendsOnly = $('#sendMessage_FriendsOnly').is(":checked") ? "true" : "false";
        let useMacros = $('#sendMessage_EnableMacros').is(":checked") ? "true" : "false";
		let randomTargetAmount = $('#sendMessageTargetAmount').val();
		let countryFilter = $('#countryFilter_Message').val();
		let raceFilter = $('#raceFilter_Message').val();
		let genderFilter = $('#genderFilter_Message').val();
        let users = GetTargetUsers('#sendMessage_users');

        let messages = [];
        // gather the array of snaps to send
        $('.messageData').each((_, el) => {
            let message = $(el).find('input[name="Message"]').val();
            let islink = $(el).find('input[name="isLink"]').is(":checked") ? "true" : "false";
            let secondsDelay = $(el).find('input[name="messageDelay"]').val();

            messages.push({ Message: message, IsLink: islink, SecondsBeforeStart: secondsDelay });
        });
        
        let args = CreateActionArguments({
            Messages: messages,
            Users: users, 
            RandomUsers: randomUsers, 
            FriendsOnly: friendsOnly, 
            RandomTargetAmount: randomTargetAmount,
			CountryFilter: countryFilter,
			RaceFilter: raceFilter,
			GenderFilter: genderFilter,
            EnableMacros: useMacros
        });

        try {
            let response = await api.SendMessage(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function SendMention(button) {
    await BlockingButtonAction(button, async () => {
        let user = $('#sendMention_user').val();
		let randomUsers = $('#sendMention_RandomTargets').is(":checked") ? "true" : "false";
        let users = GetTargetUsers('#sendMention_users');
		let friendsOnly = $('#sendMention_FriendsOnly').is(":checked") ? "true" : "false";
		let randomTargetAmount = $('#sendMentionTargetAmount').val();
		let countryFilter = $('#countryFilter_Mentioned').val();
		let raceFilter = $('#raceFilter_Mentioned').val();
		let genderFilter = $('#genderFilter_Mentioned').val();
		
        let args = CreateActionArguments({
            User: user, 
            Users: users, 
            RandomUsers: randomUsers, 
            FriendsOnly: friendsOnly, 
            RandomTargetAmount: randomTargetAmount,
			CountryFilter: countryFilter,
			RaceFilter: raceFilter,
			GenderFilter: genderFilter
        });
		
        try {
            let response = await api.SendMention(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function AddFriend(button) {
    await BlockingButtonAction(button, async () => {
        let users = GetTargetUsers('#addFriend_users');
        let friendsPerAccount = $('#addFriends_friendsPerAccount').val();
        let randomTargets = $('#addFriend_RandomTargets').is(":checked") ? "true" : "false";
        let countryFilter = $('#countryFilter').val();
        let raceFilter = $('#raceFilter').val();
        let genderFilter = $('#genderFilter').val();
        let addDelay = $('#addFriends_Delay').val();
        let MinFriends = $('#addFriends_minFriends').val();
        let MaxFriends = $('#addFriends_maxFriends').val();
        
        let args = CreateActionArguments({
            Users: users,
            FriendsPerAccount: friendsPerAccount,
            AddDelay: addDelay,
            RandomUsers: randomTargets,
            CountryFilter: countryFilter,
            RaceFilter: raceFilter,
            GenderFilter: genderFilter,
            MinFriends: MinFriends,
            MaxFriends: MaxFriends
        });

        try {
            let response = await api.AddFriend(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function QuickAdd(button) {
    await BlockingButtonAction(button, async () => {
        let Delay = $('#quickFriends_Delay').val();
        let MaxAdds = $('#quickFriends_Max').val();
        let MinFriends = $('#quickFriends_minFriends').val();
        let MaxFriends = $('#quickFriends_maxFriends').val();

        let args = CreateActionArguments({
            UseAllAccounts: true,
            MaxAdds: MaxAdds,
            AddDelay: Delay,
            MinFriends: MinFriends,
            MaxFriends: MaxFriends
        });

        try {
            let response = await api.QuickAdd(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function RemoveFriend(button) {
    await BlockingButtonAction(button, async () => {
        let Delay = $('#removeFriends_Delay').val();

        let args = CreateActionArguments({
            UseAllAccounts: true,
            AddDelay: Delay
        });

        try {
            let response = await api.RemoveFriends(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function AcceptFriend(button) {
    await BlockingButtonAction(button, async () => {
        let Delay = $('#acceptFriends_Delay').val();
        let MaxAdds = $('#acceptFriends_Max').val();
        let acceptMessage = $('#acceptSendMessage_message').val();
        
        let snaps = [];
        // gather the array of snaps to send
        $('.snapData').each((_, el) => {
            let mediaId = $(el).find('select').find('option:selected').val();
            let swipeUpUrl = $(el).find('input[name="swipeUpUrl"]').val();
            let secondsDelay = $(el).find('input[name="snapDelay"]').val();

            snaps.push({ MediaFileId: mediaId, PostDirectSwipeUpUrl: swipeUpUrl == "" ? null : swipeUpUrl, SecondsBeforeStart: secondsDelay });
        });
        
        let args = CreateActionArguments({
            Snaps: snaps,
            UseAllAccounts: true,
            MaxAdds: MaxAdds,
            AddDelay: Delay,
            AcceptMessage: acceptMessage
        });

        try {
            let response = await api.AcceptAll(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function RefreshFriends(button) {
    await BlockingButtonAction(button, async () => {

        let args = CreateActionArguments({
            UseAllAccounts: true
        });

        try {
            let response = await api.RefreshAll(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function ExportFriends(button) {
    await BlockingButtonAction(button, async () => {
        let email = $('#exportEmail').val();
        
        let args = CreateActionArguments({
            ExportEmail: email,
            UseAllAccounts: true
        });

        try {
            let response = await api.ExportFriends(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function RelogAccounts(button) {
    await BlockingButtonAction(button, async () => {

        let args = CreateActionArguments({
            UseAllAccounts: true
        });

        try {
            let response = await api.RelogAccounts(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function PostStory(button) {
    await BlockingButtonAction(button, async () => {
        let uploadId = $('#postStory-inputFile-0').find('option:selected').val();
        let users = GetTargetUsers('#postStory_mentioned');
        let swipeUpUrl = $('#postStory-swipeUpUrl-0').val();
        
        let args = CreateActionArguments({
            MediaFileId: uploadId, 
            SwipeUpUrl: swipeUpUrl, 
            Mentioned: users
        });

        try {
            let response = await api.PostStory(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

function RemoveSnap(e) {
    $(e).parent('div').remove();
    $('#snapsContainer').find('.snapTitle').each((idx, el) => {
        $(el).text(`Snap #${idx + 1}`);
    });
}

function AddSnapControls(showNoMediaLink = true, iteration = 1) {
    $.get(`/getmediaselect?controlId=postDirect&showNoMediaLink=${showNoMediaLink}&showSwipeUpUrl=true&iteration=${iteration}&showDelayField=true`, (data) => {
        let div = $(`<div class="border rounded mb-2 ps-2"><strong><span class="snapTitle">Snap #${iteration}</span></strong><a class="text-danger ms-3" onClick="RemoveSnap(this)">Remove</a><div class="snapData p-3">${data}</div></div>`);
        $('#snapsContainer').append(div);
    });
}

function RemoveMessage(e) {
    $(e).parent('div').remove();
    $('#messageContainer').find('.messageTitle').each((idx, el) => {
        $(el).text(`Message #${idx + 1}`);
    });
}

$('#btn_addSnap').on('click', () => {
    let current = $('.snapData').length;
    AddSnapControls(false, current + 1);
});

function AddMessageControls(iteration = 1) {
    $.get(`/getmessageselect?controlId=sendMessage&iteration=${iteration}&showDelayField=true`, (data) => {
        let div = $(`<div class="border rounded mb-2 ps-2"><strong><span class="messageTitle">Message #${iteration}</span></strong><a class="text-danger ms-3" onClick="RemoveMessage(this)">Remove</a><div class="messageData p-3">${data}</div></div>`);
        $('#messageContainer').append(div);
    });
}

$('#btn_addMessage').on('click', () => {
    let current = $('.messageData').length;
    AddMessageControls(current + 1);
});

(() => {
    //AddSnapControls();
})();


let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let accountManager = new AccountManager(api, logger);