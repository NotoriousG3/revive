// Setup buttons
$('#btn_uploadEmails').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let file = $('#emailsUploadFile')[0].files[0];
            let formData = new FormData();
            formData.append("inputFile", file);
            formData.append("skipCache", "0");

            let uploadResponse = await api.UploadFile(formData);
            let uploadId = uploadResponse.data;
            let response = await api.ImportEmailScraper(uploadId);
            let { results } = response.data;

            let added = results.filter(r => r.status == 0);

            let lines = [
                `${added.length} email(s) added`
            ];

            if (added.length == results.length) {
                let msg = lines.join('<br />');
                logger.Info(msg);
                let email = await LoadEmails();
                await ReloadDataTable(email);
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
                        reason = 'Email address is duplicated';
                        break;
                    case 3:
                        reason = 'Email address is empty';
                        break;
					case 4:
						reason = 'Email address has invalid characters';
						break;
                }
                lines.push(`<div>Line <strong>#${result.lineNumber}</strong> - ${reason}</div>`);
            }

            ShowImportResultModal(lines);
            let email = await LoadEmails();
            await ReloadDataTable(email);
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            logger.PrintException(e);
        }
    });
})

function ReloadDataTable(Emails) {
    $('#emailsTable').DataTable().clear().rows.add(Emails).draw();
}

$('#btn_addEmail').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        let email = await AddEmail();
        ReloadDataTable(email);
    });
});

$('#btn_scrapeEmails').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
		let actionsPerAcc = $('#emails').val();
		
		EmailToUsername(actionsPerAcc);
    });
});

async function AddEmail() {
    let email = $('#email').val();

    try {
        let result = await api.SaveEmailScraper(email);
        logger.Info('Email address added');
        return result.data;
    } catch (e) {
        logger.PrintException(e);
    }
}

async function DeleteEmailScraper(btn) {
    // Look for the id in the datatable
    let idColumn = $(btn).parent().siblings()[0];
    let id = $(idColumn).text();
    try {
        let response = await api.DeleteEmailScraper(id);
        await ReloadTable();
        logger.Info('Email address deleted');
        ReloadDataTable(response.data);
    } catch (e) {
        logger.PrintException(e);
    }
}
async function EmailToUsername(actionsPerAcc){
    let address = $('#randomizerScrape').val();
    let onlyactive = $('#OnlyActive').is(":checked");
    
    let args = CreateActionArguments({
        ActionsPerAccount: actionsPerAcc, 
        Address: address,
        OnlyActive: onlyactive
    });
    
	let response = await api.EmailToUsername(args);
	if(!response) return;

    logger.PrintWorkScheduled(response);
	
	return response.data;
}

async function LoadEmails() {
    let response = await api.GetEmailScraper();
    if (!response) return;

    return response.data;
}

async function Purge() {
    return await api.PurgeEmailScraper();
}

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Emails');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all email  from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

$('#btn_modalConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await Purge();
            modal.hide();
            logger.Info(result.message);
            await ReloadDataTable({});
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);

// Load tables
(async () => {
    try {
        let email = await LoadEmails();
        $('#emailsTable').dataTable({
            data: email,
            columns: [
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteEmailScraper(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'address'},
            ]
        });
    } catch (e) {
        if (e.status == 403) {
            LogAccessError();
            return;
        }

        throw e;
    }
})();