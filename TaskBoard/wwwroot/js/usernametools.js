// Setup buttons
$('#btn_uploadUsernames').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await usernameManager.Upload('#usernamesUploadFile');
            await ReloadTable();

            let {added, duplicated, rejected} = result.data;
            let msg = [];

            if (added.length > 0)
                msg.push(`${added.length} usernames imported`);

            if (duplicated.length > 0)
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${duplicated.length} usernames duplicated`);

            if (rejected.length > 0) {
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${rejected.length} usernames rejected:`);
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

async function DeleteUserName(el) {
    await BlockingButtonAction(el, async () => {
        //let id = $(el).closest('td').siblings()[0].textContent;
        let idColumn = $(el).parent().siblings()[0];
        let id = $(idColumn).text();
        let userName = $(el).parent().siblings()[1].textContent;
        try {
            await usernameManager.Delete(id)
            await ReloadTable();
            logger.Info(`Username <strong>${userName}</strong> deleted`);
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
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
            let result = await usernameManager.Purge();
            modal.hide();
            logger.Info(result.message);
            await ReloadTable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Usernames');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to delete all names from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

async function ReloadTable() {
    $('#usernamesTable').DataTable().ajax.reload();
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let usernameManager = new UsernameManager(api);

// Load tables
(async () => {
    try {
        $('#usernamesTable').dataTable({
            ajax: {
                url: '/api/usernames/data',
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
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteUserName(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'userName'}
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