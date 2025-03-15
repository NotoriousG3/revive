// Setup buttons
$('#btn_uploadMacros').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await macroManager.Upload('#macrosUploadFile');
            await ReloadTable();

            let {added, duplicated, rejected} = result.data;
            let msg = [];

            if (added.length > 0)
                msg.push(`${added.length} macros imported`);

            if (duplicated.length > 0)
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${duplicated.length} macros duplicated`);

            if (rejected.length > 0) {
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${rejected.length} macros rejected:`);
                for (let info of rejected)
                    msg.push(`<div>${info.macro} - ${info.reason}`);
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

async function DeleteMacro(el) {
    await BlockingButtonAction(el, async () => {
        let idColumn = $(el).parent().siblings()[0];
        let id = $(idColumn).text();
        let macro = $(el).parent().siblings()[1].textContent;
        try {
            await macroManager.Delete(id)
            await ReloadTable();
            logger.Info(`Macro <strong>${macro}</strong> deleted`);
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
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
            let result = await macroManager.Purge();
            modal.hide();
            logger.Info(result.message);
            await ReloadTable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Macros');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to delete all macros from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

async function ReloadTable() {
    $('#macrosTable').DataTable().ajax.reload();
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let macroManager = new MacroManager(api);

// Load tables
(async () => {
    try {
        $('#macrosTable').dataTable({
            ajax: {
                url: '/api/macros/data',
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
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteMacro(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'text'},
                {data: 'replacement'},
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