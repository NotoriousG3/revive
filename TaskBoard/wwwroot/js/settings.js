class Settings {
    #m_Api

    constructor(api) {
        this.#m_Api = api;
    }

    async LoadSettings() {
        let response = await this.#m_Api.GetSettings();
		
        if (!response) return;

        let {data} = response;
		
        this.ApiKey = data.apiKey;
        this.Timeout = data.timeout;
        this.EnableBandwidthSaver = data.enableBandwidthSaver;
        this.EnableWebRegister = data.enableWebRegister;
        this.EnableStealth = data.enableStealth;
        this.FiveSimApiKey = data.fiveSimApiKey;
        this.SmsPoolApiKey = data.smsPoolApiKey;
        this.NamsorApiKey = data.namsorApiKey;
		this.KopeechkaApiKey = data.kopeechkaApiKey;
        this.TwilioApiKey = data.twilioApiKey;
        this.TextVerifiedApiKey = data.textVerifiedApiKey;
        this.SmsActivateApiKey = data.smsActivateApiKey;
        this.ProxyScraping = data.proxyScraping;
        this.ProxyChecking = data.proxyChecking;
        this.MaxRetries = data.maxRetries
    }

    async LoadFromDb() {
        await Promise.all([this.LoadSettings()]);
    }

    ShowSettings() {
        $('#apikey').val(this.ApiKey);
        $('#timeout').val(this.Timeout);
        $('#enableBandwidthSaver').prop('checked', this.EnableBandwidthSaver);
        $('#enableWebRegister').prop('checked', this.EnableWebRegister);
        $('#enableStealth').prop('checked', this.EnableStealth);
        $('#proxyScraping').prop('checked', this.ProxyScraping);
        $('#proxyChecking').prop('checked', this.ProxyChecking);
        $('#fiveSimApiKey').val(this.FiveSimApiKey);
        $('#smsPoolApiKey').val(this.SmsPoolApiKey);
        $('#namsorApiKey').val(this.NamsorApiKey);
        $('#kopeechkaApiKey').val(this.KopeechkaApiKey);
        $('#twilioApiKey').val(this.TwilioApiKey);
        $('#textverifiedApiKey').val(this.TextVerifiedApiKey);
        $('#smsactivateApiKey').val(this.SmsActivateApiKey);
        $('#maxRetries').val(this.MaxRetries);
    }

    async Load() {
        await this.LoadFromDb();
        this.ShowSettings();
    }

    async Save() {
        this.ApiKey = $('#apikey').val();
        this.EnableBandwidthSaver = $('#enableBandwidthSaver').prop('checked');
        this.EnableWebRegister = $('#enableWebRegister').prop('checked');
        this.EnableStealth = $('#enableStealth').prop('checked');
        this.ProxyScraping = $('#proxyScraping').prop('checked');
        this.ProxyChecking = $('#proxyChecking').prop('checked');
        this.Timeout = $('#timeout').val();
        this.FiveSimApiKey = $('#fiveSimApiKey').val();
        this.SmsPoolApiKey = $('#smsPoolApiKey').val();
        this.NamsorApiKey = $('#namsorApiKey').val();
        this.KopeechkaApiKey = $('#kopeechkaApiKey').val();
        this.TwilioApiKey = $('#twilioApiKey').val();
        this.TextVerifiedApiKey = $('#textverifiedApiKey').val();
        this.SmsActivateApiKey = $('#smsactivateApiKey').val();
        this.MaxRetries = $('#maxRetries').val();

        await this.#m_Api.SaveSettings({
            ApiKey: this.ApiKey,
            Timeout: this.Timeout,
            EnableBandwidthSaver: this.EnableBandwidthSaver,
            EnableWebRegister: this.EnableWebRegister,
            EnableStealth: this.EnableStealth,
            FiveSimApiKey: this.FiveSimApiKey,
            SmsPoolApiKey: this.SmsPoolApiKey,
            NamsorApiKey: this.NamsorApiKey,
			KopeechkaApiKey: this.KopeechkaApiKey,
            TwilioApiKey: this.TwilioApiKey,
            TextVerifiedApiKey: this.TextVerifiedApiKey,
            SmsActivateApiKey: this.SmsActivateApiKey,
            ProxyScraping: this.ProxyScraping,
            ProxyChecking: this.ProxyChecking,
            MaxRetries: this.MaxRetries
        });
    }
}

$('#btn_saveSettings').on('click', async (evt) => {
    await BlockingButtonAction(evt.target, async () => {
        try {
            await settings.Save()
            logger.Info(`Settings updated`);
        } catch (e) {
            logger.PrintException(e)
        }
    });
});



let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);
let settings = new Settings(api);

(async () => {
    try {
        await settings.Load();
    } catch (e) {
        if (e.status == 403) {
            LogAccessError();
            return;
        }

        logger.PrintException(e);
    }
})();