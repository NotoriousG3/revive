// Setup buttons
$('#btn_uploadKeywords').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let file = $('#keywordsUploadFile')[0].files[0];
            let formData = new FormData();
            formData.append("inputFile", file);
            formData.append("skipCache", "0");

            let uploadResponse = await api.UploadFile(formData);
            let uploadId = uploadResponse.data;
            let response = await api.ImportKeywords(uploadId);
            let { results } = response.data;

            let added = results.filter(r => r.status == 0);

            let lines = [
                `${added.length} keywords added`
            ];

            if (added.length == results.length) {
                let msg = lines.join('<br />');
                logger.Info(msg);
                let keywords = await LoadKeywords();
                await ReloadDataTable(keywords);
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
                        reason = 'Keyword is duplicated';
                        break;
                    case 3:
                        reason = 'Keyword is empty';
                        break;
					case 4:
						reason = 'Keyword has invalid characters';
						break;
                }
                lines.push(`<div>Line <strong>#${result.lineNumber}</strong> - ${reason}</div>`);
            }

            ShowImportResultModal(lines);
            let keywords = await LoadKeywords();
            await ReloadDataTable(keywords);
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            logger.PrintException(e);
        }
    });
})

function ReloadDataTable(Keywords) {
    $('#keywordsTable').DataTable().clear().rows.add(Keywords).draw();
}

$('#btn_addKeyword').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        let keywords = await AddKeyword();
        ReloadDataTable(keywords);
    });
});

$('#btn_scrapeKeywords').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
		let randomArg = $('#randomizerScrape').val();
        let actionsPerAcc = $('#keywords').val();
        let onlyactive = $('#OnlyActive').is(":checked");
        let searchdelay = $('#SearchDelay').val();
		
		FindUsersViaSearch(actionsPerAcc, randomArg, onlyactive, searchdelay);
    });
});

async function AddKeyword() {
    let keyword = $('#keyword').val();

    try {
        let result = await api.SaveKeyword(keyword);
        logger.Info('Keyword added');
        return result.data;
    } catch (e) {
        logger.PrintException(e);
    }
}

async function DeleteKeyword(btn) {
    // Look for the id in the datatable
    let idColumn = $(btn).parent().siblings()[0];
    let id = $(idColumn).text();
    try {
        let response = await api.DeleteKeyword(id);
        logger.Info('Keyword deleted');
        ReloadDataTable(response.data);
    } catch (e) {
        logger.PrintException(e);
    }
}
async function FindUsersViaSearch(actionsPerAcc, randArg, onlyactive, searchdelay){
    let args = CreateActionArguments({
        ActionsPerAccount: actionsPerAcc, 
        Keyword: randArg,
        OnlyActive: onlyactive,
        SearchDelay: searchdelay
    });
    
	let response = await api.FindUsersViaSearch(args);
    
	if(!response) return;
	
	logger.Info('FindUsersViaSearch started.');
	
	return response.data;
}

async function LoadKeywords() {
    let response = await api.GetKeywords();
    if (!response) return;

    return response.data;
}

async function Purge() {
    return await api.PurgeKeywords();
}

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Keywords');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all keywords from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
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
        let keywords = await LoadKeywords();
        $('#keywordsTable').dataTable({
            data: keywords,
            columns: [
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteKeyword(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'name'},
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