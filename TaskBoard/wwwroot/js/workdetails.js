let currentPageSelector = '#pagination_current';
let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);

async function LoadWorkLogs(workId, page, targetElement) {
    let logger = new Logger(`${targetElement}`, false);
    let response = await api.GetWorkLogs(workId, page);
    let logs = response.entries;

    // Set pagination buttons
    $('#pagination_previous').toggleClass('disabled', response.previousPageNumber == null);
    $('#pagination_next').toggleClass('disabled', response.nextPageNumber == null);
    $('#pagination_current').text(response.pageNumber + 1);

    $(targetElement).empty();
    logs.forEach((log) => {
        let msg = `${log.time} - ${log.message}`;
        // This follows the LogLevel enum in c#
        switch (log.logLevel) {
            case 0:
            case 1:
                logger.Debug(msg);
                break;
            case 2:
                logger.Info(msg);
                break;
            case 4:
                logger.Error(msg);
                break;
            default:
                logger.Info(msg)
        }
    });
}

async function LoadLogsPage(dir) {
    let currentPage = parseInt($(currentPageSelector).text()) - 1;
    let targetPage = currentPage + dir;
    await LoadWorkLogs(WORKID, targetPage, LOGSCONTAINER);
}

function TimeoutRefresh() {
    setTimeout(() => {
        location = '';
    }, 15000);
}

(() => {
    LoadWorkLogs(WORKID, 0, LOGSCONTAINER);
    if (ISRUNNING) TimeoutRefresh()
})();