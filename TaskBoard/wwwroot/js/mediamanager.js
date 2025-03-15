// Setup buttons
$('#btn_upload').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        let file = $('#inputfile')[0].files[0];

        if (file == undefined) {
            logger.Error("Please, select a file");
            return;
        }

        // Check if the file has been uploaded first
        try {
            await api.IsFileInServer(file.name);
            
            logger.Error("Media is already in the server");
        } catch (e) {
            if (e.status == 403) {
                LogAccessError();
                return;
            }

            if (e.status !== 404) {
                // Exit for other error codes
                logger.PrintException(e);
                return;
            }

            // We do the upload
            let formData = new FormData();
            formData.append("inputFile", file);
            
            try {
                await api.UploadFile(formData);
                
                await ReloadDataTable();
                
                logger.Info("Media uploaded");
            } catch (e) {
                if (e.status == 403) {
                    LogAccessError();
                    return;
                }
                
                logger.PrintException(e);
            }
        }
    });
})

function ReloadDataTable(files) {
    $('#fileTable').DataTable().ajax.reload();
}

function ConfirmFileDelete(callback) {
    // Set the confirm button
    let confirmButton = $('#btn_modalConfirm');
    confirmButton.unbind();
    confirmButton.removeAttr('onclick');
    confirmButton.on('click', async (e) => {
        await BlockingButtonAction(e.target, async () => {
            await callback();
            modal.hide();
        });
    });

    let modalElement = $('#siteModal');
    modalElement.find('.modal-title').text('Delete file with SCHEDULED jobs');
    let body = modalElement.find('.modal-body');
    body.empty();
    body.append($('<p>You are about to delete a file that is used by scheduled jobs. This will delete the file and <strong class="text-danger">CANCEL</strong> all jobs that depend on this file.</p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

async function DeleteFile(btn) {
    // Look for the id in the datatable
    let idColumn = $(btn).parent().siblings()[0];
    let usedByRunningColumn = $(btn).parent().siblings()[3];
    let runningJobs = parseInt($(usedByRunningColumn).text());
    
    if (runningJobs > 0) {
        logger.Error("The file is currently in use. Cancel any jobs which might be using the file and try again");
        return;
    }
    let usedByScheduledColumn = $(btn).parent().siblings()[4];
    let pendingJobs = parseInt($(usedByScheduledColumn).text());
    let id = $(idColumn).text();
    
    let deleteFunction = async () => {
        try {
            let response = await api.DeleteFile(id);
            logger.Info('Media deleted');
            ReloadDataTable(response.data);
        } catch (e) {
            logger.PrintException(e);
        }
    }
    // If we have pending jobs, we are going to cancel all of them when deleting this file, so we'll ask for confirmation
    if (pendingJobs > 0) {
        ConfirmFileDelete(deleteFunction);
    } else {
        await deleteFunction();
    }
}

async function Purge() {
    let response = await api.PurgeFiles();
    if (!response) return;
    
    logger.Info("Media files deleted");
    ReloadDataTable(response.data);
}

function ShowPurgeModal() {
    let confirmButton = $('#btn_modalConfirm');
    confirmButton.unbind();
    confirmButton.removeAttr('onclick');
    confirmButton.on('click', async (e) => {
        await BlockingButtonAction(e.target, async () => {
            try {
                let result = await Purge();
                modal.hide();
                logger.Info(result.message);
                await ReloadDataTable();
            } catch (e) {
                logger.PrintException(e);
            }
        });
    });
    
    $('#siteModal').find('.modal-title').text('Purge Media');
    var body = $('#siteModal').find('.modal-body');
    body.empty();
    body.append($('<p>You are about to completely delete all your uploaded media from the system.</p><p><strong>THIS ACTION IS IRREVERSIBLE</strong></p>'))
    body.append($('<p>Are you sure you want to continue?</p>'));

    ShowModal(true);
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);

// Load tables
(async () => {
    try {
        $('#fileTable').dataTable({
            ajax: {
                url: '/api/upload/getfiles',
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
                {data: null, defaultContent: '<button type=button class="btn btn-danger" onClick="DeleteFile(this)"><i class="fa fa-trash"></i></button>'},
                {data: 'id'},
                {data: 'name'},
                {data: 'sizeString'},
                {data: 'usedByRunning'},
                {data: 'usedByScheduled'}
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