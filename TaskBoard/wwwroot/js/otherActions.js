async function ReportUserPublicProfileRandom(button) {
    await BlockingButtonAction(button, async () => {
        let val = $('#reportuserpublicprofilerandom_username').val();
        try {
            let args = CreateActionArguments({
                Username: val
            });
            let response = await api.ReportUserPublicProfileRandom(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function ReportUserRandom(button) {
    await BlockingButtonAction(button, async () => {
        let val = $('#reportuserrandom_username').val();
        try {
            let args = CreateActionArguments({
                Username: val
            });
            let response = await api.ReportUserRandom(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function ViewBusinessPublicStory(button) {
    await BlockingButtonAction(button, async () => {
        let username = $('#viewBusinessPublicStory_username').val();

        let args = CreateActionArguments({
            Username: username
        });

        try {
            let response = await api.ViewBusinessPublicStory(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function ReportUserStoryRandom(button) {
    await BlockingButtonAction(button, async () => {
        let username = $('#ReportUserStoryRandom_username').val();

        let args = CreateActionArguments({
            Username: username
        });

        try {
            let response = await api.ReportUserStoryRandom(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function ViewPublicStory(button) {
    await BlockingButtonAction(button, async () => {
        let username = $('#viewPublicStory_username').val();

        let args = CreateActionArguments({
            Username: username
        });

        try {
            let response = await api.ViewPublicStory(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function Subscribe(button) {
    await BlockingButtonAction(button, async () => {
        let val = $('#subscribe_username').val();

        let args = CreateActionArguments({
            Username: val
        });
        
        try {
            let response = await api.Subscribe(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

function GetTargetUsers(selector) {
    return $(selector).val().split(',').filter(e => e !== undefined && e.length > 0).map(e => e.trim());
}

async function Test(button) {
    await BlockingButtonAction(button, async () => {
        let pass = $('#test_pass').prop('checked');
        let delayMs = $('#test_delayMs').val();
        let accountsString = $('#test_accountIds').val().trim();
        let accountIds = accountsString.length == 0 ? null : accountsString.split(',');
        let isLink = $('#test_isLink').is(":checked") ? "true" : "false";
        let randomUsers = $('#test_RandomTargets').is(":checked");
        let friendsOnly = $('#test_FriendsOnly').is(":checked");
        
        let randomTargetAmount = randomUsers ? $('#testTargetAmount').val() : 0;
        let users = randomUsers ? [] : GetTargetUsers('#test_users');
        let uploadId = $('#test_inputFile').find('option:selected').val();
        
        let args = CreateActionArguments({
            Pass: pass,
            DelayMs: delayMs,
            AccountIds: accountIds,
            RandomUsers: randomUsers,
            FriendsOnly: friendsOnly,
            Users: users,
            RandomTargetAmount: randomTargetAmount,
            MediaFileId: uploadId
        });

        try {
            let response = await api.Test(args);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);