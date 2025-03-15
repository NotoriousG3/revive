// Setup buttons
$('#btn_uploadPhones').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let file = $('#phonesUploadFile')[0].files[0];
            let formData = new FormData();
            formData.append("inputFile", file);
            formData.append("skipCache", "0");

            let uploadResponse = await api.UploadFile(formData);
            let uploadId = uploadResponse.data;
            let response = await api.ImportPhoneScraper(uploadId);
            let { results } = response.data;

            let added = results.filter(r => r.status == 0);

            let lines = [
                `${added.length} phone number(s) added`
            ];

            if (added.length == results.length) {
                let msg = lines.join('<br />');
                logger.Info(msg);
                let phone = await LoadPhones();
                await ReloadDataTable(phone);
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
                        reason = 'Phone number is duplicated';
                        break;
                    case 3:
                        reason = 'Phone number is empty';
                        break;
					case 4:
						reason = 'Phone number has invalid characters';
						break;
                }
                lines.push(`<div>Line <strong>#${result.lineNumber}</strong> - ${reason}</div>`);
            }

            ShowImportResultModal(lines);
            let phone = await LoadPhones();
            await ReloadDataTable(phone);
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            logger.PrintException(e);
        }
    });
})

function ReloadDataTable(Phones) {
    $('#phonesTable').DataTable().clear().rows.add(Phones).draw();
}

$('#btn_addPhone').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        let phone = await AddPhone();
        ReloadDataTable(phone);
    });
});

$('#btn_scrapePhones').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
		let countryCode = $('#randomCountries').val();
		let actionsPerAcc = $('#numbers').val();
		let randomizer = "No";
        let onlyactive = $('#OnlyActive').is(":checked");
		
		if($('#randomizerScrape').is(":checked")){
			randomizer = "randomize";
		}
		
		PhoneToUsername(actionsPerAcc, randomizer, countryCode, onlyactive);
    });
});

async function AddPhone() {
	let countrycode = $('#countrycode').val().toUpperCase();
    let phone = $('#phone').val();

    try {
        let result = await api.SavePhoneScraper(phone, countrycode);
        logger.Info('Phone number added');
        return result.data;
    } catch (e) {
        logger.PrintException(e);
    }
}

async function DeletePhone(btn) {
    // Look for the id in the datatable
    let idColumn = $(btn).parent().siblings()[0];
    let id = $(idColumn).text();
    try {
        let response = await api.DeletePhoneScraper(id);
        logger.Info('Phone number deleted');
        ReloadDataTable(response.data);
    } catch (e) {
        logger.PrintException(e);
    }
}
async function PhoneToUsername(actionsPerAcc, randomizer, countryCode, onlyactive){
    let args = CreateActionArguments({
        ActionsPerAccount: actionsPerAcc, 
        Randomizer: randomizer, 
        Number: "random", 
        CountryCode: countryCode,
        OnlyActive: onlyactive
    });
    
	let response = await api.PhoneToUsername(args);
	if(!response) return;
	
	logger.Info('PhoneToUsername started.');
	
	return response.data;
}

async function LoadPhones() {
    let response = await api.GetPhoneScraper();
    if (!response) return;

    return response.data;
}

async function Purge() {
    return await api.PurgePhoneScraper();
}

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Phones');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all phone number(s) from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
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
        let phone = await LoadPhones();
        $('#phonesTable').dataTable({
            data: phone,
            columns: [
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeletePhone(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'countryCode'},
				{data: 'number'}
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