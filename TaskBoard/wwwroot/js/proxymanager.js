// Setup buttons
$('#btn_uploadProxies').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        let lines = [];
        try {
            let file = $('#proxyUploadFile')[0].files[0];
            
            let formData = new FormData();
            formData.append("inputFile", file);
            formData.append("skipCache", "0");

            let uploadResponse = await api.UploadFile(formData);
            let uploadId = uploadResponse.data;

            let groupData = CreateGroupData();
            let response = await api.ImportProxies(uploadId, groupData);
            let { results } = response.data;

            let added = results.filter(r => r.status == 0);

            lines = [
                `${added.length} proxies added`
            ];

            if (added.length == results.length) {
                let msg = lines.join('<br />');
                logger.Info(msg);
                
                await ReloadProxiesDatatable();
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
                        reason = 'Proxy is duplicated';
                        break;
                    case 3:
                        reason = 'Format issue. Validate that all fields are there';
                        break;
                    case 4:
                        reason = 'Address is invalid';
                        break;
                    case 5:
                        reason = 'Address is empty';
                        break;
                }
                lines.push(`<div>Line <strong>#${result.lineNumber}</strong> - ${reason}</div>`);
            }

            ShowImportResultModal(lines);
            await ReloadProxiesDatatable();
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }
            ShowImportResultModal(lines);
            logger.PrintException(e);
        }
    });
})

$('#btn_addProxy').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        await AddProxy();
        await ReloadProxiesDatatable();
    });
});

function CreateGroupData() {
    let groupName = $('#uploadGroup').val();
    let groupId = $('#uploadGroupId').find('option:selected').val() ?? 0;
    let proxyType = $('#uploadProxyType').find('option:selected').val() ?? 0;
    
    return { GroupName: groupName, GroupId: groupId, ProxyType: proxyType };
}

async function AddProxy() {
    
    try {
        let address = $('#proxy').val();
        let user = $('#proxy_user').val();
        let pass = $('#proxy_pass').val();
        
        let groupData = CreateGroupData();
        await api.SaveProxy(address, user, pass, groupData);
        await ReloadProxiesDatatable();
        logger.Info('Proxy added');
    } catch (e) {
        logger.PrintException(e);
    }
}

async function DeleteProxy(btn) {
    // Look for the id in the datatable
    let idColumn = $(btn).parent().siblings()[0];
    let id = $(idColumn).text();
    try {
        let response = await api.DeleteProxy(id);
        logger.Info('Proxy deleted');
        await ReloadProxiesDatatable();
    } catch (e) {
        logger.PrintException(e);
    }
}

async function Purge() {
    return await api.PurgeProxies();
}

function ShowPurgeModal() {
    $('#siteModal').find('.modal-title').text('Purge Proxies');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all proxies from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

$('#btn_modalConfirm').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            let result = await Purge();
            modal.hide();
            $('#btn_modalConfirm').hide();
            logger.Info(result.message);
            await ReloadProxiesDatatable();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);