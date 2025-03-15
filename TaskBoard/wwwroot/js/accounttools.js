let tb = null;
let div = null;
let oldName = null;

function statusConvert(original) {
    switch(original){
        case "OKAY":
            return 0;
        case "BAD_PROXY":
            return 1;
        case "NEEDS_RELOG":
            return 2;
        case "RATE_LIMITED":
            return 3;
        case "LOCKED":
            return 4;
        case "BANNED":
            return 5;
        case "NEEDS_FRIEND_REFRESH":
            return 6;
        case "BUSY":
            return 7;
        case "NEEDS_CHECKED":
            return 8;
        default:
            return "o";
    }
}

function validationConvert(original){
    switch(original){
        case "NotValidated":
            return 0;
        case "Validated":
            return 1;
        case "FailedValidation":
            return 2;
        case "PartiallyValidated":
            return 3;
        default:
            return "o";
    }
}

function osConvert(original) {
    switch(original){
        case "ios":
            return 0;
        case "android":
            return 1;
        default:
            return "o";
    }
}

$(document).keypress(function(event){
    var keycode = (event.keyCode ? event.keyCode : event.which);
    var properRegex = new RegExp("^[a-zA-Z0-9\._\-]+$");
    
    if(!properRegex.test(tb.val().trim())){
        logger.Error("Invalid name!");
        return;
    }
    else if(tb.val().trim().length == 0){
        logger.Error("You can't have a blank name!");
        return;    
    }else{
        if(keycode == '13'){
            var newName = tb.val().trim().toLowerCase();
            div.text(newName);
            
            
            let idColumn = $(div).closest('td').prev().text();
            
            accountManager.ChangeUsername(idColumn, oldName, newName);
        }
    }
});

$(document).on('click', '.editUsername', function() {
    div = $(this);
    tb = div.find('input:text');
    if (tb.length) {
        div.text(tb.val());
    } else {
        tb = $('<input>').prop({
            'type': 'text',
            'value': div.text()
        });
        oldName = div.text();
        div.empty().append(tb);
        tb.focus();
    }
});

$('#btn_modalPurgeAccConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        //let os = $('#filterOS').val();
        let emailvalidation = $('#filterEmailValidation').val();
        let phonevalidation = $('#filterPhoneValidation').val();
        let status = $('#filterStatus').val();
        let hasadded = $('#filterHasAdded').val();

        try {
            let args = CreateActionArguments({
                //OS: osConvert(os),
                EmailValidation: validationConvert(emailvalidation),
                PhoneValidation: validationConvert(phonevalidation),
                Status: statusConvert(status),
                hasAdded: hasadded
            });
            let result = await PurgeFilteredAccount(args);
            $('#btn_modalPurgeAccConfirm').hide();
            modal.hide();
            logger.Info(result.message);
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

async function PurgeFilteredAccount(args) {
    return await api.PurgeFilteredAccounts(args);
}

// Setup buttons
$('#btn_createAccount').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        try {
            // These follows CreateAccountArguments.cs
            let creationArguments = {
                OSSelection: GetSelectedOS(),
                PhoneVerificationService: GetVerificatorChoice(),
                EmailVerificationService: GetEmailVerificatorChoice(),
                CountryISO: GetCountryISO(),
                Gender: GetGender(),
                FirstName: GetFirstName(),
                LastName: GetLastName(),
                NameCreationService: GetNameService(),
                UserNameCreationService: GetUserNameService(),
                CustomBitmojiSelection: GetCustomBitmoji(),
                BitmojiSelection: GetBitmoji(),
                BoostScore: GetBoostScore(),
                CustomPassword: GetCustomPassword()
            };

            let response = await accountManager.CreateMultiple($('#accountsToCreate').val(), creationArguments);
            logger.PrintWorkScheduled(response);
        } catch (e) {
            // We want to show a link to the settings page
            if (e.responseJSON?.code == Api.ResponseCode.NoPhoneVerificationApiSet) {
                let msg = `${e.responseJSON.message}. Please set the appropriate API key in your <a href="settings" class="alert-link">Settings</a>`;
                logger.Error(msg)
                return;
            }
            logger.PrintException(e);
        }
    });
});

$('#btn_uploadAccounts').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let groupName = $('#uploadGroup').val();
            let groupId = $('#uploadGroupId').find('option:selected').val() ?? 0;
            let response = await accountManager.Upload('#accountsUploadFile', groupName, groupId);
            let { results } = response;
            
            let added = results.filter(r => r.status == 0);

            let lines = [
                `${added.length} accounts added`
            ];

            if (added.length == results.length) {
                let msg = lines.join('<br />');
                logger.Info(msg);
                await ReloadAccountDatatable();
                return;
            }

            for (let result of results) {
                if (result.status == 0) continue;
                
                let reason = "";
                switch(result.status) {
                    case 1:
                        reason = 'Unexpected error';
                        break;
                    case 2:
                        reason = 'Format issue. Validate that all fields are there';
                        break;
                    case 3:
                        reason = `Account <strong>${result.account.username}</strong> is duplicated`;
                        break;
                }
                lines.push(`<div>Line <strong>#${result.lineNumber}</strong> - ${reason}</div>`);
            }
            
            ShowImportResultModal(lines);
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
})

$('#btn_modalConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            $('#btn_modalConfirm').hide();
            let result = await accountManager.Purge();
            modal.hide();
            logger.Info(result.message);
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalConfirm_relogAll').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            $('#btn_modalConfirm_relogAll').hide();
            let result = await accountManager.RelogAll();
            modal.hide();
            logger.Info(result.message);
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalConfirm_cleanAll').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            await new Promise(r => setTimeout(r, 1000));
            await ReloadAccountDatatable();
            logger.Info("Success.");
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalConfirm_refreshAll').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            $('#btn_modalConfirm_refreshAll').hide();
            let result = await accountManager.RefreshAll();
            modal.hide();
            logger.Info(result.message);
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalConfirm_acceptAll').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
			let accountstouse = await accountManager.GetTotalAccounts();
			let args = CreateActionArguments({
				AccountsToUse: accountstouse,
				UseAllAccounts: true
			});
			$('#btn_modalConfirm_acceptAll').hide();
            // These follows CreateAccountArguments.cs


            let response = await accountManager.AcceptAll(args);
			
            logger.PrintWorkScheduled(response);
        } catch (e) {
            // We want to show a link to the settings page
            if (e.responseJSON?.code == Api.ResponseCode.NoPhoneVerificationApiSet) {
                let msg = `${e.responseJSON.message}. Please set the appropriate API key in your <a href="settings" class="alert-link">Settings</a>`;
                logger.Error(msg)
                return;
            }
            logger.PrintException(e);
        }
	});
});

$('#onlyWithMaxFriends').on('click', (e) => {
    SwitchExportAccountUrl($('#onlyWithMaxFriends').is(':checked'));
});

$('#onlyExportBadAccounts').on('click', (e) => {
    SwitchExportAccountUrl($('#onlyExportBadAccounts').is(':checked'));
});

$('#btn_modalExportAccountFiltered').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        //let os = $('#filterOS').val();
        let emailvalidation = $('#filterEmailValidation').val();
        let phonevalidation = $('#filterPhoneValidation').val();
        let status = $('#filterStatus').val();
        let hasadded = $('#filterHasAdded').val();

        try {
            let result = await ExportAccountFiltered(emailvalidation, phonevalidation, status, hasadded);
            download(result, "accounts.txt", "plain/text");
            logger.Info("Exported accounts.");
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

function download(content, filename, contentType)
{
    if(!contentType) contentType = 'application/octet-stream';
    var a = document.createElement('a');
    var blob = new Blob([content], {'type':contentType});
    a.href = window.URL.createObjectURL(blob);
    a.download = filename;
    a.click();
}

async function ExportAccountFiltered(emailvalidation, phonevalidation, status, hasadded) {
    return await api.ExportFilteredAccount(emailvalidation, phonevalidation, status, hasadded);
}

function ShowPurgeAccountFilterModal() {
    $('#siteModal').find('.modal-title').text('Purge Filtered Accounts');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all filter defined accounts from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "purgeAccountFiltered");
}

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Accounts');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all accounts from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

function ShowCleanModal() {
    $('#siteModal').find('.modal-title').text('Purge/Export Bad Accounts');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all flagged accounts from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "cleanAll");
}

function ShowRelogModal() {
    $('#siteModal').find('.modal-title').text('Relog Accounts');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely relog all accounts in the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "relogAll");
}

function ShowRefreshFriendsModal() {
    $('#siteModal').find('.modal-title').text('Refresh Accounts');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely refresh friends on all accounts in the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "refreshAll");
}

function ShowAcceptFriendsModal() {
    $('#siteModal').find('.modal-title').text('Accept Friends All Accounts');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to accept friends on all accounts in the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "acceptAll");
}

function SwitchExportAccountUrl(onlyMaxFriends) {
    let url = onlyMaxFriends ? '/api/account/export/maxfriendsonly' : '/api/account/export';
    $('#btn_exportAccounts').attr('href', url);
}

function GetSelectedOS() {
    let value = $('input[name="accountsOs"]:checked').val();
    return value == undefined ? 0 : value; 
}

function GetFirstName() {
    return $('#firstname').val();
}

function GetLastName() {
    return $('#lastname').val();
}

function GetGender() {
    return $('#gender').find(':selected').val();
}

function GetBitmoji() {
    return $('#bitmoji').find(':selected').val();
}

function GetCustomBitmoji() {
    return $('#CustomBitmoji').find(':selected').val();
}

function GetBoostScore() {
    return $('#boostMessages').val();
}

function GetCustomPassword() {
    return $('#customPassword').val();
}

function GetSelectedSnapchatVersion() {
    return $('#snapchatVersion').find(':selected').val();
}

function GetCountryISO() {
    let data = $('#phoneCountry').countrySelect("getSelectedCountryData");
    return data.iso2;
}

function GetNameService() {
    return $('input[name="nameCreationService"]:checked').val();
}

function GetUserNameService() {
    return $('input[name="usernameCreationService"]:checked').val();
}

function GetVerificatorChoice() {
    return $('input[name="phoneVerificationService"]:checked').val();
}

function GetEmailVerificatorChoice() {
    return $('input[name="emailVerificationService"]:checked').val();
}

async function DeleteAccount(btn) {
    await BlockingButtonAction(btn, async () => {
        // Look for the id in the datatable
        let idColumn = $(btn).parent().siblings()[0];
        let id = $(idColumn).text();
        let nameColumn = $(btn).parent().siblings()[1];
        let name = $(nameColumn).text();
        
        try {
            await accountManager.Delete(id)

            logger.Info(`Deleted account <strong>${name}</strong>`)
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function RelogAccount(btn) {
    await BlockingButtonAction(btn, async () => {
        // Look for the id in the datatable
        let idColumn = $(btn).parent().siblings()[0];
        let id = $(idColumn).text();
        let nameColumn = $(btn).parent().siblings()[1];
        let name = $(nameColumn).text();
        
        try {
            await accountManager.Relog(id)

            logger.Info(`Relog account <strong>${name}</strong>`)
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

async function LoadFriends(btn) {
    await BlockingButtonAction(btn, async () => {
        // Look for the id in the datatable
        let idColumn = $(btn).parent().siblings()[0];
        let id = $(idColumn).text();
        let nameColumn = $(btn).parent().siblings()[1];
        let name = $(nameColumn).text();
        

        try {
            await accountManager.LoadFriends(id)

            logger.Info(`LoadFriends for account <strong>${name}</strong>`)
            await ReloadAccountDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
}

function padTo2Digits(num) {
    return num.toString().padStart(2, '0');
}

function convertMsToTime(milliseconds) {
    let seconds = Math.floor(milliseconds / 1000);
    let minutes = Math.floor(seconds / 60);
    let hours = Math.floor(minutes / 60);

    seconds = seconds % 60;
    minutes = minutes % 60;

    // 👇️ If you don't want to roll hours over, e.g. 24 to 00
    // 👇️ comment (or remove) the line below
    // commenting next line gets you `24:00:00` instead of `00:00:00`
    // or `36:15:31` instead of `12:15:31`, etc.
    hours = hours % 24;

    return `${padTo2Digits(hours)}h ${padTo2Digits(minutes)}m ${padTo2Digits(
        seconds,
    )}s`;
}

function LoadSnapchatVersions(os) {
    $.get('/getsnapchatversionselect/horizontal', { os }, (data) => {
        $('#snapchatVersionSelect').html(data);
    });
}

$('input[name="accountsOs"]').on('change', () => {
    let selectedValue = $('input[name="accountsOs"]:checked').val();
    LoadSnapchatVersions(selectedValue);
});

LoadSnapchatVersions(GetSelectedOS());

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let accountManager = new AccountManager(api, logger);

// Load tables
(async () => {
    $("#phoneCountry").countrySelect({
        onlyCountries: [ "ru", "nl", "us", "gb" ],
        defaultCountry: "ru"
    });
})();