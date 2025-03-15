class WorkManager {
    #m_Api;
    #m_Logger;

    constructor(api, logger) {
        this.#m_Api = api;
        this.#m_Logger = logger
    }

    async List() {
        this.jobs = await this.#m_Api.GetJobs();

        this.jobsByStatus = {};
        this.jobsByStatus[WorkStatus.NotRun] = [];
        this.jobsByStatus[WorkStatus.Error] = [];
        this.jobsByStatus[WorkStatus.Incomplete] = [];
        this.jobsByStatus[WorkStatus.Waiting] = [];
        this.jobsByStatus[WorkStatus.Ok] = [];
        this.jobsByStatus[WorkStatus.Cancelled] = [];

        this.jobs.forEach((j) => this.jobsByStatus[j.status].push(j));
    }

    CreateColumn(text, columnClass = "col col-xxl-2") {
        return $(`<td>${text}</td>`);
    }

    CreateWorkHeader() {
        let row = $(`<div class="row mb-1 border-bottom"></div>`);
        row.append(this.CreateColumn('Work Id', "col col-xxl-1"));
        row.append(this.CreateColumn('Work Action'));
        row.append(this.CreateColumn('Work Status'));
        row.append(this.CreateColumn('Progress'));

        return row;
    }

    GetWorkStatusString(work) {
        if (work.isFinished)
            return 'Finished';

        if (work.isScheduled) {
            var localTime = new Date(work.scheduledTime).toLocaleString('en-US');
            return `Scheduled (${localTime})`;
        }
        
        if (work.isRunning)
            return 'Running';

        return WorkStatus.ToString(work.status);
    }

    CreateWorkRow(work) {
        let row = $('<tr></tr>');
        let d = new Date(work.requestTime);
        let e = new Date(work.finishTime);
        let requestDate = ("0" + d.getDate()).slice(-2) + "-" + ("0"+(d.getMonth()+1)).slice(-2) + "-" +
            d.getFullYear() + " " + ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2);
        let finishDate = ("0" + e.getDate()).slice(-2) + "-" + ("0"+(e.getMonth()+1)).slice(-2) + "-" +
            e.getFullYear() + " " + ("0" + e.getHours()).slice(-2) + ":" + ("0" + e.getMinutes()).slice(-2);

        row.append(this.CreateColumn(`<a href="workstatus/${work.id}">${work.id}</a>`, "col col-xxl-1"));
        row.append(this.CreateColumn(`${WorkAction.TimeStatus(requestDate)}`));
        row.append(this.CreateColumn(`${WorkAction.TimeStatus(finishDate)}`));
        row.append(this.CreateColumn(`${WorkAction.ToString(work.action)}`));

        let status = this.GetWorkStatusString(work);
        row.append(this.CreateColumn(`${status}`));

        // Progress bar column
        let barCol = $('<td></td>');
        row.append(barCol);
        let progressBar = $('<div class="progress float-start w-75 progress-element"></div>');
        barCol.append(progressBar);
        let stripedClass = work.isRunning ? ' progress-bar-striped progress-bar-animated' : '';
        
        let remainingPercent = (100 / work.accountsToUse * work.accountsLeft);

        let accountsLeftBar = $(`<div class="progress-bar text-bg-secondary${stripedClass}" role="progressbar" style="width: ${remainingPercent}%" aria-valuenow="${work.accountsLeft}" aria-valuemin="0" aria-valuemax="${work.accountsToUse}" data-bs-toggle="tooltip" title="In Progress">${work.accountsLeft}</div>`);
        new bootstrap.Tooltip(accountsLeftBar);
        progressBar.append(accountsLeftBar);
        let failPercent = (100 / work.accountsToUse * work.accountsFail);
        let failBar = $(`<div class="progress-bar bg-danger${stripedClass}" role="progressbar" style="width: ${failPercent}%" aria-valuenow="${work.accountsFail}" aria-valuemin="0" aria-valuemax="${work.accountsToUse}" data-bs-toggle="tooltip" title="Failed">${work.accountsFail}</div>`);
        new bootstrap.Tooltip(failBar);
        progressBar.append(failBar);
        let successPercent = (100 / work.accountsToUse * work.accountsPass);
        let successBar = $(`<div class="progress-bar bg-success${stripedClass}" role="progressbar" style="width: ${successPercent}%" aria-valuenow="${work.accountsPass}" aria-valuemin="0" aria-valuemax="${work.accountsToUse}" data-bs-toggle="tooltip" title="Completed">${work.accountsPass}</div>`);
        new bootstrap.Tooltip(successBar);
        progressBar.append(successBar);

        // Add cancellation button
        if (work.isRunning || work.isScheduled || work.status == WorkStatus.NotRun) {
            barCol.append($(`<div class="work-cancel"><a href="#" class="text-danger" onclick="workManager.CancelWork(${work.id});"><i class="fa fa-times-circle"</i></a></div>`))
        }
        return row;
    }

    async LoadWorkUI(delayMs = 0) {
        await this.List();
        $('#workFailed').text(this.jobsByStatus[WorkStatus.Error].length);
        $('#workRunning').text(this.jobsByStatus[WorkStatus.Waiting].filter(j => j.isRunning).length);
        $('#workIncomplete').text(this.jobsByStatus[WorkStatus.Incomplete].length);
        $('#workNotRun').text(this.jobsByStatus[WorkStatus.NotRun].length);
        $('#workOk').text(this.jobsByStatus[WorkStatus.Ok].length);
        $('#workCount').text(this.jobs.length);

        let sorted = this.jobs.reverse();

        var tbody = $('#workTable > tbody');
        tbody.find('tr').remove();

        sorted.forEach(j => {
            let row = this.CreateWorkRow(j);
            tbody.append(row)
        });
    }


    async CancelWork(workId) {
        try {
            let response = await this.#m_Api.CancelWork(workId);
            this.#m_Logger.Info('Job cancelled');
            await this.LoadWorkUI(500);
        } catch (e) {
            this.#m_Logger.PrintException(e);
        }
    }
}