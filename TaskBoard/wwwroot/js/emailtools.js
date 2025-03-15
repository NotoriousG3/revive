// Setup buttons
$('#btn_uploadEmails').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await emailManager.Upload('#emailsUploadFile');
            await ReloadTable();

            let {added, duplicated, rejected} = result.data;
            let msg = [];

            if (added.length > 0)
                msg.push(`${added.length} emails imported`);

            if (duplicated.length > 0)
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${duplicated.length} emails duplicated`);

            if (rejected.length > 0) {
                msg.push(`${msg.length > 0 ? '<hr />' : ''}${rejected.length} emails rejected:`);
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

async function DeleteEmail(el) {
    await BlockingButtonAction(el, async () => {
        let id = $(el).closest('td').siblings()[0].textContent;
        try {
            await emailManager.Delete(id)
            await ReloadTable();
            logger.Info(`Email <strong>${id}</strong> deleted`);
            //$('#emailsTable').DataTable().clear().rows.add(emailManager.Emails).draw();
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
            let result = await emailManager.Purge();
            modal.hide();
            logger.Info(result.message);
            await ReloadTable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge E-Mails');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to delete all <strong>unassigned</strong> e-mails from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

async function ReloadTable() {
    $('#emailsTable').DataTable().ajax.reload();
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let emailManager = new EmailManager(api);

// Load tables
(async () => {
    try {
        $('#emailsTable').dataTable({
            ajax: {
                url: '/api/email/data',
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
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteEmail(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'address'},
                {data: 'password'},
                {data: 'account.username', defaultContent: ""}
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