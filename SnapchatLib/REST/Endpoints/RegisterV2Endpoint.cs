using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Janus.Crypto.Fidelius;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using SnapchatLib.Extras;
using SnapProto.Snapchat.Cdp.Cof;
using SnapProto.Snapchat.Janus.Api;
using SnapProto.Snapchat.Search;
using MailKit.Net.Pop3;
using System.Text.RegularExpressions;
using SnapchatLib.Encryption;

namespace SnapchatLib.REST.Endpoints;

public interface IRegisterV2Endpoint
{
    Task<SCJanusRegisterWithUsernamePasswordResponse> Register(string username, string password, string firstname, string lastname);
    Task<ValidationStatus> ValidateEmail(string emailpop3host, int emailpop3port, bool useSsl, bool useUndetectedBrowser, string email, string password);
    Task<ValidationStatus> ValidateEmail(string link, bool useUndetectedBrowser);
    Task<WebCreationStatus> RegisterWeb(string firstname, string lastname, string username, string password, string email, string emailpassword);
}


internal class RegisterV2Endpoint : EndpointAccessor, IRegisterV2Endpoint
{
    public RegisterV2Endpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    private static string[] _timezones2021 = { "Africa/Abidjan", "Africa/Accra", "Africa/Addis_Ababa", "Africa/Algiers", "Africa/Asmara", "Africa/Asmera", "Africa/Bamako", "Africa/Bangui", "Africa/Banjul", "Africa/Bissau", "Africa/Blantyre", "Africa/Brazzaville", "Africa/Bujumbura", "Africa/Cairo", "Africa/Casablanca", "Africa/Ceuta", "Africa/Conakry", "Africa/Dakar", "Africa/Dar_es_Salaam", "Africa/Djibouti", "Africa/Douala", "Africa/El_Aaiun", "Africa/Freetown", "Africa/Gaborone", "Africa/Harare", "Africa/Johannesburg", "Africa/Juba", "Africa/Kampala", "Africa/Khartoum", "Africa/Kigali", "Africa/Kinshasa", "Africa/Lagos", "Africa/Libreville", "Africa/Lome", "Africa/Luanda", "Africa/Lubumbashi", "Africa/Lusaka", "Africa/Malabo", "Africa/Maputo", "Africa/Maseru", "Africa/Mbabane", "Africa/Mogadishu", "Africa/Monrovia", "Africa/Nairobi", "Africa/Ndjamena", "Africa/Niamey", "Africa/Nouakchott", "Africa/Ouagadougou", "Africa/Porto-Novo", "Africa/Sao_Tome", "Africa/Timbuktu", "Africa/Tripoli", "Africa/Tunis", "Africa/Windhoek", "America/Adak", "America/Anchorage", "America/Anguilla", "America/Antigua", "America/Araguaina", "America/Argentina/Buenos_Aires", "America/Argentina/Catamarca", "America/Argentina/ComodRivadavia", "America/Argentina/Cordoba", "America/Argentina/Jujuy", "America/Argentina/La_Rioja", "America/Argentina/Mendoza", "America/Argentina/Rio_Gallegos", "America/Argentina/Salta", "America/Argentina/San_Juan", "America/Argentina/San_Luis", "America/Argentina/Tucuman", "America/Argentina/Ushuaia", "America/Aruba", "America/Asuncion", "America/Atikokan", "America/Atka", "America/Bahia", "America/Bahia_Banderas", "America/Barbados", "America/Belem", "America/Belize", "America/Blanc-Sablon", "America/Boa_Vista", "America/Bogota", "America/Boise", "America/Buenos_Aires", "America/Cambridge_Bay", "America/Campo_Grande", "America/Cancun", "America/Caracas", "America/Catamarca", "America/Cayenne", "America/Cayman", "America/Chicago", "America/Chihuahua", "America/Coral_Harbour", "America/Cordoba", "America/Costa_Rica", "America/Creston", "America/Cuiaba", "America/Curacao", "America/Danmarkshavn", "America/Dawson", "America/Dawson_Creek", "America/Denver", "America/Detroit", "America/Dominica", "America/Edmonton", "America/Eirunepe", "America/El_Salvador", "America/Ensenada", "America/Fort_Nelson", "America/Fort_Wayne", "America/Fortaleza", "America/Glace_Bay", "America/Godthab", "America/Goose_Bay", "America/Grand_Turk", "America/Grenada", "America/Guadeloupe", "America/Guatemala", "America/Guayaquil", "America/Guyana", "America/Halifax", "America/Havana", "America/Hermosillo", "America/Indiana/Indianapolis", "America/Indiana/Knox", "America/Indiana/Marengo", "America/Indiana/Petersburg", "America/Indiana/Tell_City", "America/Indiana/Vevay", "America/Indiana/Vincennes", "America/Indiana/Winamac", "America/Indianapolis", "America/Inuvik", "America/Iqaluit", "America/Jamaica", "America/Jujuy", "America/Juneau", "America/Kentucky/Louisville", "America/Kentucky/Monticello", "America/Knox_IN", "America/Kralendijk", "America/La_Paz", "America/Lima", "America/Los_Angeles", "America/Louisville", "America/Lower_Princes", "America/Maceio", "America/Managua", "America/Manaus", "America/Marigot", "America/Martinique", "America/Matamoros", "America/Mazatlan", "America/Mendoza", "America/Menominee", "America/Merida", "America/Metlakatla", "America/Mexico_City", "America/Miquelon", "America/Moncton", "America/Monterrey", "America/Montevideo", "America/Montreal", "America/Montserrat", "America/Nassau", "America/New_York", "America/Nipigon", "America/Nome", "America/Noronha", "America/North_Dakota/Beulah", "America/North_Dakota/Center", "America/North_Dakota/New_Salem", "America/Ojinaga", "America/Panama", "America/Pangnirtung", "America/Paramaribo", "America/Phoenix", "America/Port-au-Prince", "America/Port_of_Spain", "America/Porto_Acre", "America/Porto_Velho", "America/Puerto_Rico", "America/Punta_Arenas", "America/Rainy_River", "America/Rankin_Inlet", "America/Recife", "America/Regina", "America/Resolute", "America/Rio_Branco", "America/Rosario", "America/Santa_Isabel", "America/Santarem", "America/Santiago", "America/Santo_Domingo", "America/Sao_Paulo", "America/Scoresbysund", "America/Shiprock", "America/Sitka", "America/St_Barthelemy", "America/St_Johns", "America/St_Kitts", "America/St_Lucia", "America/St_Thomas", "America/St_Vincent", "America/Swift_Current", "America/Tegucigalpa", "America/Thule", "America/Thunder_Bay", "America/Tijuana", "America/Toronto", "America/Tortola", "America/Vancouver", "America/Virgin", "America/Whitehorse", "America/Winnipeg", "America/Yakutat", "America/Yellowknife", "Antarctica/Casey", "Antarctica/Davis", "Antarctica/DumontDUrville", "Antarctica/Macquarie", "Antarctica/Mawson", "Antarctica/McMurdo", "Antarctica/Palmer", "Antarctica/Rothera", "Antarctica/South_Pole", "Antarctica/Syowa", "Antarctica/Troll", "Antarctica/Vostok", "Arctic/Longyearbyen", "Asia/Aden", "Asia/Almaty", "Asia/Amman", "Asia/Anadyr", "Asia/Aqtau", "Asia/Aqtobe", "Asia/Ashgabat", "Asia/Ashkhabad", "Asia/Atyrau", "Asia/Baghdad", "Asia/Bahrain", "Asia/Baku", "Asia/Bangkok", "Asia/Barnaul", "Asia/Beirut", "Asia/Bishkek", "Asia/Brunei", "Asia/Calcutta", "Asia/Chita", "Asia/Choibalsan", "Asia/Chongqing", "Asia/Chungking", "Asia/Colombo", "Asia/Dacca", "Asia/Damascus", "Asia/Dhaka", "Asia/Dili", "Asia/Dubai", "Asia/Dushanbe", "Asia/Famagusta", "Asia/Gaza", "Asia/Harbin", "Asia/Hebron", "Asia/Ho_Chi_Minh", "Asia/Hong_Kong", "Asia/Hovd", "Asia/Irkutsk", "Asia/Istanbul", "Asia/Jakarta", "Asia/Jayapura", "Asia/Jerusalem", "Asia/Kabul", "Asia/Kamchatka", "Asia/Karachi", "Asia/Kashgar", "Asia/Kathmandu", "Asia/Katmandu", "Asia/Khandyga", "Asia/Kolkata", "Asia/Krasnoyarsk", "Asia/Kuala_Lumpur", "Asia/Kuching", "Asia/Kuwait", "Asia/Macao", "Asia/Macau", "Asia/Magadan", "Asia/Makassar", "Asia/Manila", "Asia/Muscat", "Asia/Nicosia", "Asia/Novokuznetsk", "Asia/Novosibirsk", "Asia/Omsk", "Asia/Oral", "Asia/Phnom_Penh", "Asia/Pontianak", "Asia/Pyongyang", "Asia/Qatar", "Asia/Qyzylorda", "Asia/Rangoon", "Asia/Riyadh", "Asia/Saigon", "Asia/Sakhalin", "Asia/Samarkand", "Asia/Seoul", "Asia/Shanghai", "Asia/Singapore", "Asia/Srednekolymsk", "Asia/Taipei", "Asia/Tashkent", "Asia/Tbilisi", "Asia/Tehran", "Asia/Tel_Aviv", "Asia/Thimbu", "Asia/Thimphu", "Asia/Tokyo", "Asia/Tomsk", "Asia/Ujung_Pandang", "Asia/Ulaanbaatar", "Asia/Ulan_Bator", "Asia/Urumqi", "Asia/Ust-Nera", "Asia/Vientiane", "Asia/Vladivostok", "Asia/Yakutsk", "Asia/Yangon", "Asia/Yekaterinburg", "Asia/Yerevan", "Atlantic/Azores", "Atlantic/Bermuda", "Atlantic/Canary", "Atlantic/Cape_Verde", "Atlantic/Faeroe", "Atlantic/Faroe", "Atlantic/Jan_Mayen", "Atlantic/Madeira", "Atlantic/Reykjavik", "Atlantic/South_Georgia", "Atlantic/St_Helena", "Atlantic/Stanley", "Australia/ACT", "Australia/Adelaide", "Australia/Brisbane", "Australia/Broken_Hill", "Australia/Canberra", "Australia/Currie", "Australia/Darwin", "Australia/Eucla", "Australia/Hobart", "Australia/LHI", "Australia/Lindeman", "Australia/Lord_Howe", "Australia/Melbourne", "Australia/NSW", "Australia/North", "Australia/Perth", "Australia/Queensland", "Australia/South", "Australia/Sydney", "Australia/Tasmania", "Australia/Victoria", "Australia/West", "Australia/Yancowinna", "Brazil/Acre", "Brazil/DeNoronha", "Brazil/East", "Brazil/West", "CET", "CST6CDT", "Canada/Atlantic", "Canada/Central", "Canada/Eastern", "Canada/Mountain", "Canada/Newfoundland", "Canada/Pacific", "Canada/Saskatchewan", "Canada/Yukon", "Chile/Continental", "Chile/EasterIsland", "Cuba", "EET", "EST", "EST5EDT", "Egypt", "Eire", "Etc/GMT", "Etc/GMT+0", "Etc/GMT+1", "Etc/GMT+10", "Etc/GMT+11", "Etc/GMT+12", "Etc/GMT+2", "Etc/GMT+3", "Etc/GMT+4", "Etc/GMT+5", "Etc/GMT+6", "Etc/GMT+7", "Etc/GMT+8", "Etc/GMT+9", "Etc/GMT-0", "Etc/GMT-1", "Etc/GMT-10", "Etc/GMT-11", "Etc/GMT-12", "Etc/GMT-13", "Etc/GMT-14", "Etc/GMT-2", "Etc/GMT-3", "Etc/GMT-4", "Etc/GMT-5", "Etc/GMT-6", "Etc/GMT-7", "Etc/GMT-8", "Etc/GMT-9", "Etc/GMT0", "Etc/Greenwich", "Etc/UCT", "Etc/UTC", "Etc/Universal", "Etc/Zulu", "Europe/Amsterdam", "Europe/Andorra", "Europe/Astrakhan", "Europe/Athens", "Europe/Belfast", "Europe/Belgrade", "Europe/Berlin", "Europe/Bratislava", "Europe/Brussels", "Europe/Bucharest", "Europe/Budapest", "Europe/Busingen", "Europe/Chisinau", "Europe/Copenhagen", "Europe/Dublin", "Europe/Gibraltar", "Europe/Guernsey", "Europe/Helsinki", "Europe/Isle_of_Man", "Europe/Istanbul", "Europe/Jersey", "Europe/Kaliningrad", "Europe/Kiev", "Europe/Kirov", "Europe/Lisbon", "Europe/Ljubljana", "Europe/London", "Europe/Luxembourg", "Europe/Madrid", "Europe/Malta", "Europe/Mariehamn", "Europe/Minsk", "Europe/Monaco", "Europe/Moscow", "Europe/Nicosia", "Europe/Oslo", "Europe/Paris", "Europe/Podgorica", "Europe/Prague", "Europe/Riga", "Europe/Rome", "Europe/Samara", "Europe/San_Marino", "Europe/Sarajevo", "Europe/Saratov", "Europe/Simferopol", "Europe/Skopje", "Europe/Sofia", "Europe/Stockholm", "Europe/Tallinn", "Europe/Tirane", "Europe/Tiraspol", "Europe/Ulyanovsk", "Europe/Uzhgorod", "Europe/Vaduz", "Europe/Vatican", "Europe/Vienna", "Europe/Vilnius", "Europe/Volgograd", "Europe/Warsaw", "Europe/Zagreb", "Europe/Zaporozhye", "Europe/Zurich", "GB", "GB-Eire", "GMT", "GMT+0", "GMT-0", "GMT0", "Greenwich", "HST", "Hongkong", "Iceland", "Indian/Antananarivo", "Indian/Chagos", "Indian/Christmas", "Indian/Cocos", "Indian/Comoro", "Indian/Kerguelen", "Indian/Mahe", "Indian/Maldives", "Indian/Mauritius", "Indian/Mayotte", "Indian/Reunion", "Iran", "Israel", "Jamaica", "Japan", "Kwajalein", "Libya", "MET", "MST", "MST7MDT", "Mexico/BajaNorte", "Mexico/BajaSur", "Mexico/General", "NZ", "NZ-CHAT", "Navajo", "PRC", "PST8PDT", "Pacific/Apia", "Pacific/Auckland", "Pacific/Bougainville", "Pacific/Chatham", "Pacific/Chuuk", "Pacific/Easter", "Pacific/Efate", "Pacific/Enderbury", "Pacific/Fakaofo", "Pacific/Fiji", "Pacific/Funafuti", "Pacific/Galapagos", "Pacific/Gambier", "Pacific/Guadalcanal", "Pacific/Guam", "Pacific/Honolulu", "Pacific/Johnston", "Pacific/Kiritimati", "Pacific/Kosrae", "Pacific/Kwajalein", "Pacific/Majuro", "Pacific/Marquesas", "Pacific/Midway", "Pacific/Nauru", "Pacific/Niue", "Pacific/Norfolk", "Pacific/Noumea", "Pacific/Pago_Pago", "Pacific/Palau", "Pacific/Pitcairn", "Pacific/Pohnpei", "Pacific/Ponape", "Pacific/Port_Moresby", "Pacific/Rarotonga", "Pacific/Saipan", "Pacific/Samoa", "Pacific/Tahiti", "Pacific/Tarawa", "Pacific/Tongatapu", "Pacific/Truk", "Pacific/Wake", "Pacific/Wallis", "Pacific/Yap", "Poland", "Portugal", "ROC", "ROK", "Singapore", "Turkey", "UCT", "US/Alaska", "US/Aleutian", "US/Arizona", "US/Central", "US/East-Indiana", "US/Eastern", "US/Hawaii", "US/Indiana-Starke", "US/Michigan", "US/Mountain", "US/Pacific", "US/Samoa", "UTC" };
    SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign zodiac_sign(int day, int month)
    {
        SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Unset;

        // checks month and date within the
        // valid range of a specified zodiac
        if (month == 12)
        {

            if (day < 22)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Sagittarius;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Capricorn;
        }

        else if (month == 1)
        {
            if (day < 20)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Capricorn;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Aquarius;
        }

        else if (month == 2)
        {
            if (day < 19)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Aquarius;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
        }

        else if (month == 3)
        {
            if (day < 21)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
        }
        else if (month == 4)
        {
            if (day < 20)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Pisces;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Taurus;
        }

        else if (month == 5)
        {
            if (day < 21)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Taurus;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Gemini;
        }

        else if (month == 6)
        {
            if (day < 21)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Gemini;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Cancer;
        }

        else if (month == 7)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Cancer;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Leo;
        }

        else if (month == 8)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Leo;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Virgo;
        }

        else if (month == 9)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Virgo;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Libra;
        }

        else if (month == 10)
        {
            if (day < 23)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Libra;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Scorpio;
        }

        else if (month == 11)
        {
            if (day < 22)
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Scorpio;
            else
                astro_sign = SCS2UserInfo.Types.HappeningNowHoroscope_AstrologicalSign.Sagittarius;
        }

        return astro_sign;
    }

    private string GetSnapchatConfirmationLink(string body)
    {
        var regex = new Regex(@"https:\/\/accounts\.snapchat\.com\/accounts\/confirm_email\?n=[\w]*");
        return regex.Match(body).Value;
    }
    public async Task<ValidationStatus> ValidateEmail(string link, bool useUndetectedBrowser)
    {
        // we want to do this dance for 10 minutes tops
        //|-----------------------------------------------------------------------|
        //|    o   \ o /  _ o         __|    \ /     |__        o _  \ o /   o    |
        //|   /|\    |     /\   ___\o   \o    |    o/    o/__   /\     |    /|\   |
        //|   / \   / \   | \  /)  |    ( \  /o\  / )    |  (\  / |   / \   / \   |
        //|-----------------------------------------------------------------------|
        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attempts);
        var maxWaitTime = TimeSpan.FromMinutes(10);

        while (true)
        {

            // Now use puppeteer to open the link
            var webproxy = Config.Proxy;

            if (useUndetectedBrowser)
            {

                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments($"--proxy-server=http://{Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "")}");
                using (var driver = UndetectedChromeDriver.Create(chromeOptions, null,
        driverExecutablePath:
            await new ChromeDriverInstaller().Auto(), null, 0, 0, true))
                {
                    driver.GoToUrl(link);
                    var responseText = driver.PageSource;

                    if (responseText.Contains("invalid") || responseText.Contains("sorry") ||
                        responseText.Contains("expired")) // check for all possible error messages from snapchat
                    {
                        return ValidationStatus.FailedValidation;
                    }

                    if (responseText.Contains("Email Address Verified") || responseText.Contains("verified") | responseText.Contains("Thanks!"))
                        return ValidationStatus.Validated;
                    driver.Quit();
                }
            }
            else
            {

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = Config.Proxy,
                };

                var deviceInfo = Base64.Base64Decode(Config.DeviceProfile).Split(':');
                using var _client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
                _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"Mozilla/5.0 (Linux; Android {deviceInfo[2]}; {deviceInfo[1]}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Mobile Safari/537.36");
                var httpResponse = await _client.GetAsync(link);
                var responseText = await httpResponse.Content.ReadAsStringAsync();

                if (responseText.Contains("invalid") || responseText.Contains("sorry") ||
                    responseText
                        .Contains(
                            "expired")) // check for all possible error messages from snapchat
                {
                    return ValidationStatus.FailedValidation;
                }


                if (responseText.Contains("Email Address Verified") || responseText.Contains("verified") | responseText.Contains("Thanks!"))
                    return ValidationStatus.Validated;
            }

            return ValidationStatus.FailedValidation; // we return this if all the errors above return false this means we validated!
        }
    }
    public async Task<ValidationStatus> ValidateEmail(string emailpop3host, int emailpop3port, bool useSsl, bool useUndetectedBrowser, string email, string password)
    {
        // we want to do this dance for 10 minutes tops
        //|-----------------------------------------------------------------------|
        //|    o   \ o /  _ o         __|    \ /     |__        o _  \ o /   o    |
        //|   /|\    |     /\   ___\o   \o    |    o/    o/__   /\     |    /|\   |
        //|   / \   / \   | \  /)  |    ( \  /o\  / )    |  (\  / |   / \   / \   |
        //|-----------------------------------------------------------------------|
        var attempts = 1;
        var waitTime = TimeSpan.FromSeconds(2 ^ attempts);
        var maxWaitTime = TimeSpan.FromMinutes(10);

        while (true)
        {
            using var client = new Pop3Client();
            await client.ConnectAsync(emailpop3host, emailpop3port, useSsl);

            client.AuthenticationMechanisms.Remove("XOAUTH2");

            try
            {
                await client.AuthenticateAsync(email, password);
            }
            catch (Exception ex)
            {
                m_Logger.Debug($"{email} - {ex.Message}");
                return ValidationStatus.FailedValidation;
            }

            // Get all messages
            var countAttempts = 0;
            var count = 0;
            bool isFailed = false;

            while (true)
            {
                count = await client.GetMessageCountAsync();

                if (count != 0)
                {
                    break;
                }

                if (countAttempts++ >= 5)
                {
                    isFailed = true;
                    break;
                }

                await Task.Delay(5000);
            }

            if (isFailed)
            {
                return ValidationStatus.FailedValidation;
            }

            var allMessages = await client.GetMessagesAsync(0, count);

            // Order the messages by date, then filter those that do not have a snapchat link
            var filtered = allMessages.OrderByDescending(m => m.Date).Where(m => m.HtmlBody != null && !string.IsNullOrWhiteSpace(GetSnapchatConfirmationLink(m.HtmlBody))).ToList();

            if (filtered.Count == 0)
            {
                if (waitTime >= maxWaitTime) return ValidationStatus.FailedValidation;

                await Task.Delay(waitTime);
                waitTime = TimeSpan.FromSeconds(2 ^ ++attempts);
                await client.DisconnectAsync(true);
                continue;
            }

            // We use the first, this is already ordered to be latest first
            var first = filtered.FirstOrDefault();

            if (first != null)
            {
                var link = GetSnapchatConfirmationLink(first.HtmlBody);

                // Now use puppeteer to open the link
                var webproxy = Config.Proxy;

                if (useUndetectedBrowser)
                {

                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArguments($"--proxy-server=http://{Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "")}");
                    using (var driver = UndetectedChromeDriver.Create(chromeOptions, null,
            driverExecutablePath:
                await new ChromeDriverInstaller().Auto(), null, 0, 0, true))
                    {
                        driver.GoToUrl(link);
                        var responseText = driver.PageSource;

                        if (responseText.Contains("invalid") || responseText.Contains("sorry") ||
                            responseText.Contains("expired")) // check for all possible error messages from snapchat
                        {
                            return ValidationStatus.FailedValidation;
                        }

                        if (responseText.Contains("Email Address Verified") || responseText.Contains("verified") | responseText.Contains("Thanks!"))
                            return ValidationStatus.Validated;
                        driver.Quit();
                    }
                }
                else
                {

                    var httpClientHandler = new HttpClientHandler
                    {
                        Proxy = Config.Proxy,
                    };

                    using var _client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
                    var deviceInfo = Base64.Base64Decode(Config.DeviceProfile).Split(':');
                    _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"Mozilla/5.0 (Linux; Android {deviceInfo[2]}; {deviceInfo[1]}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Mobile Safari/537.36");
                    var httpResponse = await _client.GetAsync(link);
                    var responseText = await httpResponse.Content.ReadAsStringAsync();

                    if (responseText.Contains("invalid") || responseText.Contains("sorry") ||
                        responseText
                            .Contains(
                                "expired")) // check for all possible error messages from snapchat
                    {
                        return ValidationStatus.FailedValidation;
                    }


                    if (responseText.Contains("Email Address Verified") || responseText.Contains("verified") | responseText.Contains("Thanks!"))
                        return ValidationStatus.Validated;
                }

                return ValidationStatus.FailedValidation; // we return this if all the errors above return false this means we validated!
            }
        }
    }
    public virtual async Task<WebCreationStatus> RegisterWeb(string firstname, string lastname, string username, string password, string email, string emailpassword)
    {
        try
        {
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(emailpassword))
            {
                throw new Exception("Missing required fields");
            }


            // Get device information
            m_Logger.Debug("Getting Device Info");
            // Set up device information
            // Set up user information
            Config.Username = username;
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;
            int rndYear = rnd.Next(1965, 2004);
            int rndMonth = rnd.Next(1, 12);
            int rndDay = rnd.Next(1, 28);
            Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));
            Config.TimeZone = _timezones2021.PickRandom();
            Config.Horoscope = zodiac_sign(rndDay, rndMonth);
            // Check if the proxy has credentials
            if (Config.Proxy.Credentials != null)
            {
                var proxyExtension = new ProxyExtension(Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "").Split(":")[0], Convert.ToInt32(Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "").Split(":")[1]), Config.Proxy.Credentials.GetCredential(Config.Proxy.Address, "").UserName, Config.Proxy.Credentials.GetCredential(Config.Proxy.Address, "").Password);
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument($"--load-extension={proxyExtension.Directory}");
                chromeOptions.AddArgument("--mute-audio");
                chromeOptions.AddArguments("--window-size=1,1");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--disable-software-rasterizer");
                chromeOptions.AddArguments($"--proxy-server=http://{Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "")}");
                using (var driver = UndetectedChromeDriver.Create(chromeOptions, null,
        driverExecutablePath:
            await new ChromeDriverInstaller().Auto(), null, 0, 0, true))
                {
                    driver.GoToUrl("https://accounts.snapchat.com/accounts/signup?client_id=ads-api&referrer=https%253A%252F%252Fbusiness.snapchat.com%252F");
                    var firstName = driver.FindElement(By.XPath("//input[@name='firstName']"));
                    firstName.Click();
                    firstName.SendKeys(firstname);
                    var lastName = driver.FindElement(By.XPath("//input[@name='lastName']"));
                    lastName.Click();
                    lastName.SendKeys(lastname);
                    var inputusername = driver.FindElement(By.XPath("//input[@name='username']"));
                    inputusername.Click();
                    inputusername.SendKeys(username);
                    var inputpassword = driver.FindElement(By.XPath("//input[@name='password']"));
                    inputpassword.Click();
                    inputpassword.SendKeys(password);
                    var inputemail = driver.FindElement(By.XPath("//input[@name='email']"));
                    inputemail.Click();
                    inputemail.SendKeys(email);
                    var birthdayMonth = new SelectElement(driver.FindElement(By.XPath("//select[@name='birthdayMonth']")));
                    birthdayMonth.SelectByValue(rndMonth.ToString());
                    var birthdayDay = driver.FindElement(By.XPath("//input[@name='birthdayDay']"));
                    birthdayDay.Click();
                    birthdayDay.SendKeys(rndMonth.ToString());
                    var birthdayYear = driver.FindElement(By.XPath("//input[@name='birthdayYear']"));
                    birthdayYear.Click();
                    birthdayYear.SendKeys(rndYear.ToString());
                    await Task.Delay(5000);
                    var submit = driver.FindElement(By.XPath("//button[normalize-space()='Sign Up & Accept']"));
                    submit.Click();
                    driver.Quit();
                }
                // Register the user
                return WebCreationStatus.Created;
            }
            else
            {
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--mute-audio");
                chromeOptions.AddArguments("--window-size=1,1");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--disable-software-rasterizer");
                chromeOptions.AddArguments($"--proxy-server=http://{Config.Proxy.Address.ToString().Replace("/", "").Replace("http:", "")}");
                using (var driver = UndetectedChromeDriver.Create(chromeOptions, null,
        driverExecutablePath:
            await new ChromeDriverInstaller().Auto(), null, 0, 0, true))
                {
                    driver.GoToUrl("https://accounts.snapchat.com/accounts/signup?client_id=ads-api&referrer=https%253A%252F%252Fbusiness.snapchat.com%252F");
                    var firstName = driver.FindElement(By.XPath("//input[@name='firstName']"));
                    firstName.Click();
                    firstName.SendKeys(firstname);
                    var lastName = driver.FindElement(By.XPath("//input[@name='lastName']"));
                    lastName.Click();
                    lastName.SendKeys(lastname);
                    var inputusername = driver.FindElement(By.XPath("//input[@name='username']"));
                    inputusername.Click();
                    inputusername.SendKeys(username);
                    var inputpassword = driver.FindElement(By.XPath("//input[@name='password']"));
                    inputpassword.Click();
                    inputpassword.SendKeys(password);
                    var inputemail = driver.FindElement(By.XPath("//input[@name='email']"));
                    inputemail.Click();
                    inputemail.SendKeys(email);
                    var birthdayMonth = new SelectElement(driver.FindElement(By.XPath("//select[@name='birthdayMonth']")));
                    birthdayMonth.SelectByValue(rndMonth.ToString());
                    var birthdayDay = driver.FindElement(By.XPath("//input[@name='birthdayDay']"));
                    birthdayDay.Click();
                    birthdayDay.SendKeys(rndMonth.ToString());
                    var birthdayYear = driver.FindElement(By.XPath("//input[@name='birthdayYear']"));
                    birthdayYear.Click();
                    birthdayYear.SendKeys(rndYear.ToString());
                    await Task.Delay(5000);
                    var submit = driver.FindElement(By.XPath("//button[normalize-space()='Sign Up & Accept']"));
                    submit.Click();
                    driver.Quit();
                }
                // Register the user
                return WebCreationStatus.Created;
            }

        }
        catch (Exception ex)
        {
            /*
            if (Config.Debug)
            {
                throw new Exception(ex.ToString());
            }
            */
            return WebCreationStatus.Failed;
        }
    }
    public virtual async Task<SCJanusRegisterWithUsernamePasswordResponse> Register(string username, string password, string firstname, string lastname)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname))
            {
                throw new Exception("Missing required fields");
            }
            // Get device information
            m_Logger.Debug("Getting Device Info");
            // Set up device information
            await SnapchatClient.GetDevice();
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long oneHourAgoTimestamp = currentTimestamp - (60 * 60 * 1000);
            Config.install_time = new Random().NextInt64(1680660161000, oneHourAgoTimestamp);
            Config.Device = Guid.NewGuid().ToString();
            Config.Install = Guid.NewGuid().ToString();
            await SnapchatClient.SetDeviceInfo();
            SnapchatGrpcClient.SetupServiceClients();

            // Set up user information
            Config.Username = username;
            Random rnd = new Random();
            DateTime datetoday = DateTime.Now;
            int rndYear = rnd.Next(1965, 2004);
            int rndMonth = rnd.Next(1, 12);
            int rndDay = rnd.Next(1, 28);
            Config.Age = m_Utilities.GetAge(new DateTime(rndYear, rndMonth, rndDay));
            Config.TimeZone = _timezones2021.PickRandom();
            Config.Horoscope = zodiac_sign(rndDay, rndMonth);
            Config.ClientID = Guid.NewGuid().ToString();

            // Set up targeting information
            SCCofConfigTargetingResponse cof = null;
            var BlizzardSessionId = "f2." + m_Utilities.RandomString(16);
            var _SCCofConfigTargetingRequest = new SCCofConfigTargetingRequest
            {
                ScreenHeight = 1080,
                ScreenWidth = 1080,
                Connectivity = new SCCofConnectivity { NetworkType = SCCofConnectivity.Types.SCCofConnectivity_NetworkType.Wifi },
                MaxVideoHeightPx = 1080,
                MaxVideoWidthPx = 2135,
                DeltaSync = true,
                TriggerEventType = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingTriggerEventType.ForegroundTrigger,
                AppState = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingAppState.Foreground,
                DeviceId = Config.Device,
                IsUnAuthorized = true,
                AppLocale = "en",
                Instrumentation = SCCofConfigTargetingRequest.Types.SCCofConfigTargetingInstrumentation.UserAuthentication,
                SyncTriggerBlizzardSessionId = BlizzardSessionId,
                SyncExecutionBlizzardSessionId = BlizzardSessionId,
                CofSyncExecutionDelayFromStartupMs = 92,
                CofSyncTriggerDelayFromStartupMs = 130,
                SyncTriggerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                LenscoreVersion = 201,
                ClientId = Config.ClientID,
            };
            cof = await SnapchatGrpcClient.CofAsync(_SCCofConfigTargetingRequest);
            Config.AccountCountryCode = cof.Iso3166Alpha2CountryCodeFromRequestIp;
            // Set up configuration data for Android
            List<int> cofConfigData_Android = new List<int>();
            foreach (var x in cof.ConfigResultsArray)
            {
                if (x.NamespaceP == ConfigResult.Types.Namespace.ActivationCore && x.Priority != -1)
                {
                    cofConfigData_Android.Add(x.SequenceId);
                }
            }
            foreach (var x in cof.ConfigResultsArray)
            {
                if (x.NamespaceP == ConfigResult.Types.Namespace.Security && x.Priority != -1)
                {
                    cofConfigData_Android.Add(x.SequenceId);
                }
            }

            foreach (var x in cof.ConfigResultsArray)
            {
                if (x.ConfigId == "EEL_RECEIVE_CONFIG")
                {
                    SnapchatClient.mcs_cof_ids_bin.Add(x.SequenceId);
                }
                if (x.ConfigId == "mcs_regionalization_treatment")
                {
                    SnapchatClient.mcs_cof_ids_bin.Add(x.SequenceId);
                }
            }

            var fidelius = FideliusDevice.Create();
            // Register the user
            var signregister = await SnapchatGrpcClient.Sign("/snapchat.janus.api.RegistrationService/RegisterWithUsernamePassword");
            var _SCJanusRegisterWithUsernamePasswordRequest = new SCJanusRegisterWithUsernamePasswordRequest
            {
                FirstName = firstname,
                LastName = lastname,
                Username = username,
                Password = password,
                Birthdate = new SnapProto.Google.Type.GTPDate { Day = rndDay, Month = rndMonth, Year = rndYear },
                PredictedPhoneNumberCountryCode = Config.AccountCountryCode,
                TimeZone = Config.TimeZone,
                AutofillSource = SCJanusRegisterWithUsernamePasswordRequest.Types.SCJanusPhoneAutofillSource.Empty,
                RegistrationHeader = new SCJanusRegistrationHeader
                {
                    AuthenticationSessionId = Guid.NewGuid().ToString(),
                    BlizzardClientId = Config.ClientID,
                    NetworkRequestId = Guid.NewGuid().ToString(),
                    RegistrationFlowSessionId = Guid.NewGuid().ToString(),
                    ClientAttestationPayload = ByteString.CopyFrom(Convert.FromBase64String(signregister.Attestation.Replace('-', '+').Replace('_', '/'))),
                    CofTags = new SCJanusCofTags { ETag = cof.ConfigResultsEtag },
                    CofDeviceId = Config.Device,
                    CofConfigData = new PartialToken { SequenceIdsArray = { cofConfigData_Android } },
                    DeviceToken = new SCJanusDeviceToken { IdP = Config.dtoken1i },
                    FideliusClientInit = new SCJanusFideliusClientInit
                    {
                        TentativeDeviceKey = new SCJanusFideliusTentativeDeviceKey
                        {
                            PublicKey = ByteString.CopyFrom(fidelius.PublicUnwrapped),
                            HashedPublicKey = ByteString.CopyFrom(fidelius.PublicHash),
                            Iwek = ByteString.CopyFrom(fidelius.Iwek),
                            Version = 9
                        }
                    },
                    VendorAttestationPayload = ByteString.FromBase64("CAESAmllGAAiAzc6ICoUY29tLnNuYXBjaGF0LmFuZHJvaWQwBw=="),
                    AttemptNumber = 1,
                }
            };
            var resp = await SnapchatGrpcClient.RegisterAsync(_SCJanusRegisterWithUsernamePasswordRequest);
            await SnapchatClient.SendMetrics("registration_code", ((int)resp.StatusCode).ToString());
            // Check for errors
            if (resp.ErrorData != null && !string.IsNullOrEmpty(resp.ErrorData.HumanReadableErrorMessage))
            {
                throw new Exception(resp.ErrorData.HumanReadableErrorMessage);
            }

            // Set up user authentication tokens and access tokens
            if (resp.BootstrapData.UserSession.AuthToken != null)
            {
                Config.refreshToken = resp.BootstrapData.UserSession.SnapSessionResponse.RefreshToken;
                Config.AuthToken = resp.BootstrapData.UserSession.AuthToken;
                Config.user_id = resp.BootstrapData.UserSession.UserId;
                m_Logger.Debug($"User_id: {Config.user_id}");
                m_Logger.Debug($"AuthToken: {Config.AuthToken}");
                m_Logger.Debug("Registration complete");

                var cof2result = SCCofConfigTargetingResponse.Parser.ParseFrom(resp.BootstrapData.Cof);

                foreach (var cof2 in cof2result.ConfigResultsArray)
                {
                    if (cof2.SequenceId != 0)
                    {
                        if (cof2.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_CREATE_STREAK_END_STATUS" || cof2.ConfigId == "MAX_HOURS_AFTER_STREAK_EXPIRE_TO_ENABLE_RESTORE" || cof2.ConfigId == "MIN_STREAK_COUNT_TO_ENABLE_RESTORE" || cof2.ConfigId == "STREAKS_EDUCATION_ENABLED")
                        {
                            SnapchatClient.mcs_cof_ids_bin.Add(cof2.SequenceId);
                        }
                    }
                }

                foreach (var snapAccessTokensArray in resp.BootstrapData.UserSession.SnapSessionResponse.SnapAccessTokensArray)
                {

                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/api-gateway")
                    {
                        Config.Access_Token = snapAccessTokensArray.AccessToken;
                    }
                    if (snapAccessTokensArray.Scope == "https://auth.snapchat.com/snap_token/api/business-accounts")
                    {
                        Config.BusinessAccessToken = snapAccessTokensArray.AccessToken;
                    }
                }
            }
            return resp;
        }
        catch (Exception ex)
        {
            if (Config.Debug)
            {
                throw new Exception(ex.ToString());
            }

            throw new Exception("Failed to register");
        }
    }
}