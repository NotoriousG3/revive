let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let workManager = new WorkManager(api, logger);

let refreshInterval;

async function LoadUI() {
    try {
        await workManager.LoadWorkUI();
    } catch (e) {
        if (e.status == 403) {
            LogAccessError();
            clearInterval(refreshInterval);
            return;
        }

        throw e;
    }
}

(async () => {
    await LoadUI();
    refreshInterval = setInterval(async () => await LoadUI(), 5000);
})();
