// Setup buttons
$('#btn_uploadNames').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await nameManager.Upload('#namesUploadFile');
            await ReloadTable();

            let {added, duplicated, rejected} = result.data;
            let msg = [];

            if (added.length > 0)
                msg.push(`${added.length} names imported`);

            if (duplicated.length > 0)
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${duplicated.length} names duplicated`);

            if (rejected.length > 0) {
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${rejected.length} names rejected:`);
                for (let info of rejected)
                    msg.push(`<div>${info.email} - ${info.reason}`);
            }

            let line = msg.join('<br />');

            if (added.length > 0)
                logger.Info(line);
            else
                logger.Error(line);
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            logger.PrintException(e);
        }
    });
})

async function DeleteName(el) {
    await BlockingButtonAction(el, async () => {
        //let id = $(el).closest('td').siblings()[0].textContent;
        let idColumn = $(el).parent().siblings()[0];
        let id = $(idColumn).text();
        let firstName = $(el).parent().siblings()[1].textContent;
        let lastName = $(el).parent().siblings()[2].textContent;
        try {
            await nameManager.Delete(id)
            await ReloadTable();
            logger.Info(`Name <strong>${firstName} ${lastName}</strong> deleted`);
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            logger.PrintException(e);
        }
    });
}

$('#btn_modalConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await nameManager.Purge();
            modal.hide();
            logger.Info(result.message);
            await ReloadTable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Names');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to delete all names from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

async function ReloadTable() {
    $('#namesTable').DataTable().ajax.reload();
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let nameManager = new NameManager(api);

// Load tables
(async () => {
    try {
        $('#namesTable').dataTable({
            ajax: {
                url: '/api/names/data',
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
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteName(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'firstName'},
                {data: 'lastName'}
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