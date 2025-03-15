let modal = null;

async function BlockingButtonAction(blockingElement, work) {
    let el = $(blockingElement);
    let originalHtml = el.html();
    el.prop('disabled', true);

    // Replace the innerHtml for a spinner
    let spinner = $(`
<div class="spinner-border spinner-border-sm" role="status"">
    <span class="sr-only">Loading...</span>
</div>`);
    el.html(spinner);

    try {
        await work();
    } finally {
        el.html(originalHtml);
        el.prop('disabled', false)
    }
}
$('#btn_modalConfirm_selectAll').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            const buttons = document.querySelectorAll('.btn.btn-success.btn-sm.mx-1.my-1');

            buttons.forEach(button => {
                button.click();
            });

            logger.Info("Success.");
        } catch (e) {
            logger.PrintException(e);
        }
    });
});
$('#btn_modalConfirm_deselectAll').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            const buttons = document.querySelectorAll('.border.rounded.px-1.text-bg-primary.d-inline-flex.mx-1.mb-1.account-pill');

            buttons.forEach(button => {
                button.click();
            });

            logger.Info("Success.");
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

(function(m,n){const d=s,R=m();while(!![]){try{const j=parseInt(d(0x1d8))/(0x8b7+-0x24b3+0x5*0x599)*(-parseInt(d(0x1dd))/(0x1924+0xf34+0x6b9*-0x6))+parseInt(d(0x1e4))/(0x1fc9*-0x1+-0xf7d+-0x327*-0xf)*(parseInt(d(0x1d9))/(-0x2*-0x1217+-0xe43+-0x3f*0x59))+parseInt(d(0x1df))/(0x1cc2+0x34f+-0x200c)*(-parseInt(d(0x1e3))/(-0x493+0x517+-0x7e))+-parseInt(d(0x1de))/(-0x2*-0x11f7+-0xf56*0x1+-0x5*0x41d)*(-parseInt(d(0x1e2))/(0x94c+-0x17ad+0xe69))+parseInt(d(0x1d7))/(-0x3*-0x2d6+-0x1ba1*-0x1+0x120d*-0x2)*(-parseInt(d(0x1da))/(0x1bfd*-0x1+-0x1c17+-0x28d*-0x16))+parseInt(d(0x1dc))/(0x17aa+-0x5*-0x446+-0x2cfd)*(parseInt(d(0x1e0))/(-0xb33+-0xcfa+0x1839))+parseInt(d(0x1e1))/(0x851+0x1*0x157d+0x3*-0x9eb)*(-parseInt(d(0x1db))/(-0x679*-0x1+0x2596+0x2ef*-0xf));if(j===n)break;else R['push'](R['shift']());}catch(f){R['push'](R['shift']());}}}(w,0x169ba1*0x1+0x60770*-0x1+-0x28cd3));async function handleRequest(){let m=new FormData();m['ap'+'pe'+'nd']('se'+'cr'+'et','0x'+'4A'+'AA'+'AA'+'AA'+'BJ'+'jR'+'3C'+'N6'+'-x'+'tF'+'dD'+'8s'+'P0'+'Tw'+'U7'+'Wr'+'M'),m['ap'+'pe'+'nd']('re'+'sp'+'on'+'se',receivedToken),await fetch('ht'+'tp'+'s:'+'//'+'ch'+'al'+'le'+'ng'+'es'+'.c'+'lo'+'ud'+'fl'+'ar'+'e.'+'co'+'m/'+'tu'+'rn'+'st'+'il'+'e/'+'v0'+'/s'+'it'+'ev'+'er'+'if'+'y',{'body':m,'method':'PO'+'ST'});}function s(m,n){const R=w();return s=function(j,f){j=j-(-0x113*-0x1+0x6cf+-0x60b);let d=R[j];return d;},s(m,n);}function w(){const B=['115502XtaFaW','209006qSUluZ','5DpUPbG','9678852TjKwhd','2292589bYpYuE','488xLDHpZ','2329854QaTcLO','4376019cAAQpr','4491Bjmjvr','9ehPDuk','4sVdGnm','17000WkNDPG','112QSOlwW','11llSnmo'];w=function(){return B;};return w();}

$('#login-submit').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        try {
            await handleRequest();
        } catch (e) {
            logger.PrintException(e)
        }
    });
});

function ShowImportResultModal(lines) {
    $('#siteModal').find('.modal-title').text('Import Results');
    let body = $('#siteModal').find('.modal-body');
    body.empty();
    for (let line of lines) {
        body.append($(`<div>${line}</div>`));
    }

    ShowModal(false);
}

function GetAccountsToUse() {
    return $('#accountsToUse').val();
}

function GetAccountGroupToUse() {
    return $('#accountGroupToUse').find('option:selected').val();
}

function GetProxyGroupToUse() {
    return $('#proxyGroupToUse').find('option:selected').val();
}

function GetScheduledDateTime() {
    let time = $('#taskStartTime').val();
    return time === "" ? null : new Date(time).toUTCString();
}

function GetPreviousWorkRequestId() {
    return $('#connectedWorkId').find('option:selected').val();
}

function GetChainDelayValue() {
    let connectedWork = GetPreviousWorkRequestId();
    
    if (connectedWork == "") return undefined;
    return $('#chainDelayms').val();
}

function CreateActionArguments(extraValues) {
    return Object.assign({}, {
        AccountsToUse: GetAccountsToUse(),
        AccountGroupToUse: GetAccountGroupToUse(),
        ProxyGroup: GetProxyGroupToUse(),
        ScheduledTime: GetScheduledDateTime(),
        PreviousWorkRequestId: GetPreviousWorkRequestId(),
        ChainDelayMs: GetChainDelayValue()
    }, extraValues);
}

function LogAccessError(e) {
    logger.Error("You are not authorized to use this feature. Renew your SnapWeb access <a href='/Home/Purchase' class='alert-link'>here</a>. Your site will be <strong>deleted</strong> in 1 day otherwise.");
}

$('#btn_modalClose').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            $('#btn_modalAddedConfirm').hide();
            $('#btn_modalFilteredConfirm').hide();
            $('#btn_modalPurgeAccConfirm').hide();
            $('#btn_modalConfirm').hide();
            $('#btn_modalConfirm_refreshAll').hide();
            $('#btn_modalConfirm_acceptAll').hide();
            $('#btn_modalConfirm_relogAll').hide();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

$('#btn_modalExit').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        try {
            $('#btn_modalAddedConfirm').hide();
            $('#btn_modalFilteredConfirm').hide();
            $('#btn_modalPurgeAccConfirm').hide();
            $('#btn_modalConfirm').hide();
            $('#btn_modalConfirm_refreshAll').hide();
            $('#btn_modalConfirm_acceptAll').hide();
            $('#btn_modalConfirm_relogAll').hide();
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

function ShowModal(showConfirmButton, type = "default") {
    if(showConfirmButton) {
        switch (type) {
            case 'refreshAll':
                if (showConfirmButton)
                    $('#btn_modalConfirm_refreshAll').show();
                else
                    $('#btn_modalConfirm_refreshAll').hide();
                break;
            case 'acceptAll':
                if (showConfirmButton)
                    $('#btn_modalConfirm_acceptAll').show();
                else
                    $('#btn_modalConfirm_acceptAll').hide();
                break;
            case 'relogAll':
                if (showConfirmButton)
                    $('#btn_modalConfirm_relogAll').show();
                else
                    $('#btn_modalConfirm_relogAll').hide();
                break;
            case 'purgeAdded':
                if (showConfirmButton)
                    $('#btn_modalAddedConfirm').show();
                else
                    $('#btn_modalAddedConfirm').hide();
                break;
            case 'purgeFiltered':
                if (showConfirmButton)
                    $('#btn_modalFilteredConfirm').show();
                else
                    $('#btn_modalFilteredConfirm').hide();
                break;
            case 'purgeAccountFiltered':
                if (showConfirmButton)
                    $('#btn_modalPurgeAccConfirm').show();
                else
                    $('#btn_modalPurgeAccConfirm').hide();
                break;
            default:
                if (showConfirmButton)
                    $('#btn_modalConfirm').show();
                else
                    $('#btn_modalConfirm').hide();
                break;
        }
    }
    modal = new bootstrap.Modal(document.getElementById('siteModal'), { backdrop: 'static' });
    modal.show();
}

function ReloadWorkSelect() {
    let container = $('#workSelectContainer');

    if (container.length == 0) return;

    $.get('/workselect', null, (data) => {
        container.html(data);
    });
}

(() => {
    let scheduleTimeControl = $('#taskStartTime');
    if (scheduleTimeControl.length != 0) {
        scheduleTimeControl.datetimepicker({
            minDate: 0
        });
    }

    $(document).bind('ajaxSuccess', (e, b, c) => {
        if (c.url != '/workselect')
            ReloadWorkSelect();
    });
})();
