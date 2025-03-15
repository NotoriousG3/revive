// Setup buttons

$('#btn_uploadUsers').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let file = $('#usersUploadFile')[0].files[0];
            let formData = new FormData();
            formData.append("inputFile", file);
            formData.append("skipCache", "0");

            let uploadResponse = await api.UploadFile(formData);
            let uploadId = uploadResponse.data;
            let response = await api.ImportTargetUsers(uploadId);
            let { results } = response.data;

            let added = results.filter(r => r.status == 0);

            let lines = [
                `${added.length} users added`
            ];

            if (added.length == results.length) {
                let msg = lines.join('<br />');
                logger.Info(msg);
                await ReloadDataTable();
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
                        reason = 'User is duplicated';
                        break;
                    case 3:
                        reason = 'Username is empty';
                        break;
                }
                lines.push(`<div>Line <strong>#${result.lineNumber}</strong> - ${reason}</div>`);
            }

            ShowImportResultModal(lines);
            await ReloadDataTable();
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            logger.PrintException(e);
        }
    });
})

function ReloadDataTable() {
    $('#targetsTable').DataTable().ajax.reload();
}

$('#btn_addUser').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        let users = await AddTargetUser();
        ReloadDataTable(users);
    });
});

async function AddTargetUser() {
    let username = $('#username').val();

    try {
        let result = await api.SaveTargetUser(username);
        logger.Info('User added');
        return result.data;
    } catch (e) {
        logger.PrintException(e);
    }
}

async function DeleteTargetUser(btn) {
    // Look for the id in the datatable
    let idColumn = $(btn).parent().siblings()[0];
    let id = $(idColumn).text();
    try {
        let response = await api.DeleteTargetUser(id);
        logger.Info('User deleted');
        ReloadDataTable(response.data);
    } catch (e) {
        logger.PrintException(e);
    }
}

async function Purge() {
    return await api.PurgeTargetUsers();
}

async function PurgeAdded() {
    return await api.PurgeAddedTargetUsers();
}

async function PurgeFiltered(args) {
    return await api.PurgeFilteredTargetUsers(args);
}

async function ExportFiltered(CountryCode, Gender, Race, Added, Searched) {
    return await api.ExportFilteredTargetUsers(CountryCode, Gender, Race, Added, Searched);
}

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Target Users');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all target users from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

function ShowPurgeFilterModal() {
    $('#siteModal').find('.modal-title').text('Purge Filtered Target Users');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all filter defined target users from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "purgeFiltered");
}

function ShowPurgeAddedModal() {
    $('#siteModal').find('.modal-title').text('Purge Added Target Users');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all added target users from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true, "purgeAdded");
}

$('#btn_modalAddedConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await PurgeAdded();
            $('#btn_modalAddedConfirm').hide();
            modal.hide();
            logger.Info(result.message);
            await ReloadDataTable({});
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalFilteredConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        let country = $('#filterCountryCode').val();
        let gender = $('#filterGender').val();
        let race = $('#filterRace').val();
        let added = $('#filterAdded').val();
        let searched = $('#filterSearched').val();

        try {
            let args = CreateActionArguments({
                Searched: searched,
                CountryCode: country,
                Gender: gender,
                Race: race,
                Added: added
            });
            let result = await PurgeFiltered(args);
            $('#btn_modalFilteredConfirm').hide();
            modal.hide();
            logger.Info(result.message);
            await ReloadDataTable({});
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

$('#btn_modalExportFiltered').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        let country = $('#filterCountryCode').val();
        let gender = $('#filterGender').val();
        let race = $('#filterRace').val();
        let added = $('#filterAdded').val();
        let searched = $('#filterSearched').val();

        try {
            let result = await ExportFiltered(country, gender, race, added, searched);
            await ReloadDataTable({});
            download(result, "targetusers.txt", "plain/text");
            logger.Info("Exported targets.");
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await Purge();
            $('#btn_modalConfirm').hide();
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

function CountryCheck(Data) {
    alert(Data.text());
    return Data.valueOf('countryCode');
}

// Load tables
(async () => {
    try {
        $('#targetsTable').dataTable({
            ajax: {
                url: '/api/targetuser/data',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                content: 'json',
                data: function (d) {
                    return JSON.stringify(d);
                }
            },
            search: {
                return: true
            },
            processing: true,
            serverSide: true,
            columns: [
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteTargetUser(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'countryCode'},
				{data: 'gender'},
				{data: 'race'},
                {data: 'userID'},
                {data: 'username'},
                {data: 'added'},
                {data: 'searched'},
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