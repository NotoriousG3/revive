function statusConvert(original) {
    switch(original){
        case "OKAY":
            return 0;
        case "BAD_PROXY":
            return 1;
        case "NEEDS_RELOG":
            return 2;
        case "RATE_LIMITED":
            return 3;
        case "LOCKED":
            return 4;
        case "BANNED":
            return 5;
        default:
            return "o";
    }
}

function validationConvert(original){
    switch(original){
        case "NotValidated":
            return 0;
        case "Validated":
            return 1;
        case "FailedValidation":
            return 2;
        case "PartiallyValidated":
            return 3;
        default:
            return "o";
    }
}

function osConvert(original) {
    switch(original){
        case "ios":
            return 0;
        case "android":
            return 1;
        default:
            return "o";
    }
}

class Api {
    static ResponseCode = {
        Ok: 0,
        Error: 1,
        MaximumAccounts: 2,
        ArgumentsValidationFailed: 3,
        NoPhoneVerificationApiSet: 4,
        Unauthorized: 5
    }

    #m_Logger

    constructor(logger) {
        this.#m_Logger = logger
    }
    
    GetToken() {
        return document.getElementById("RequestVerificationToken").value
    }

    async CreateRandomAccount(accounts, creationArguments) {
        let finalArgs = Object.assign({}, { AccountsToUse: accounts, ProxyGroup: GetProxyGroupToUse() }, creationArguments);
        return await this.PostJson('/api/account', finalArgs);
    }

    GetAllAccounts() {
        return new Promise((resolve, reject) => {
            $.get('/api/account', null, (data) => {
                resolve(data);
            }).fail((err) => reject(err));
        })
    }

    GetEmails() {
        return new Promise((resolve, reject) => {
            $.get('/api/email', null, (data) => {
                resolve(data);
            }).fail((err) => reject(err));
        })
    }

    GetSettings() {
        return new Promise((resolve, reject) => {
            $.get('/api/appsettings', {}, (data) => {
                resolve(data);
            }).fail((err) => reject(err));
        });
    }

    GetProxies() {
        return new Promise((resolve, reject) => {
            $.get('/api/proxy', {}, data => {
                resolve(data);
            }).fail((err) => reject(err));
        });
    }

    PostJson(url, object) {
        return new Promise((resolve, reject) => {
            $.ajax(url, {
                method: "POST",
                data: JSON.stringify(object),
                headers: {
                    "X-XSRF-TOKEN":
                    this.GetToken()
                },
                content: 'json',
                contentType: "application/json; charset=utf-8",
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        });
    }

    SaveSettings(settings) {
        return this.PostJson('/api/appsettings', settings);
    }

    DeleteAccount(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/account/${id}`, {
                method: "DELETE",
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        })
    }
	
    RelogAccount(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/account/relog/${id}`, {
                method: "POST",
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        })
    }
	
	LoadFriends(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/account/loadfriends/${id}`, {
                method: "POST",
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        })
    }
	
	AcceptFriends(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/account/acceptfriends/${id}`, {
                method: "POST",
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        })
    }

    DeleteEmail(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/email/${id}`, {
                method: "DELETE",
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        })
    }

    // Action template
    async Subscribe(args) {
        return await this.PostJson('/api/snapchataction/subscribe', args);
    }
    
    async ChangeUsername(args) {
        return await this.PostJson(`/api/account/changeusername`, args);
    }
    
	async FindUsersViaSearch(args) {
        return await this.PostJson('/api/snapchataction/findusersviasearch', args);
    }
    
	async PhoneToUsername(args) {
        return await this.PostJson('/api/snapchataction/phonetousername', args);
    }
	
	async EmailToUsername(args) {
        return await this.PostJson('/api/snapchataction/emailtousername', args);
    }

    async ReportUserPublicProfileRandom(args) {
        return await this.PostJson('/api/snapchataction/reportuserpublicprofilerandom', args);
    }

    async ReportUserRandom(args) {
        return await this.PostJson('/api/snapchataction/reportuserrandom', args);
    }

    async IsFileInServer(fileName) {
        return new Promise((resolve, reject) => {
            $.get('/api/upload', {filename: fileName}, (response) => {
                resolve(response);
            }).fail((err) => {
                reject(err);
            })
        });
    }

    async UploadFile(formData) {
        return new Promise((resolve, reject) => {
            $.ajax('/api/upload', {
                method: "POST",
                data: formData,
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                cache: false,
                contentType: false,
                processData: false,
                success: (data) => {
                    resolve(data);
                },
                error: (data) => {
                    reject(data);
                }
            });
        });
    }

    async PostDirect(args) {
        return await this.PostJson('/api/snapchataction/postdirect', args);
    }

    async SendMention(args) {
        return await this.PostJson('/api/snapchataction/sendmention', args);
    }
	
    async SendMessage(args) {
        return await this.PostJson('/api/snapchataction/sendmessage', args);
    }

    async ViewBusinessPublicStory(args) {
        return await this.PostJson('/api/snapchataction/viewbusinesspublicstory', args);
    }

    async ReportUserStoryRandom(args) {
        return await this.PostJson('/api/snapchataction/ReportUserStoryRandom', args);
    }

    async ViewPublicStory(args) {
        return await this.PostJson('/api/snapchataction/viewpublicstory', args);
    }

    async AddFriend(args) {
        return await this.PostJson('/api/snapchataction/addfriend', args);
    }

    async Test(args) {
        return await this.PostJson('/api/snapchataction/test', args);
    }

    async PostStory(args) {
        return await this.PostJson('/api/snapchataction/poststory', args);
    }

    async GetJobs() {
        return new Promise((resolve, reject) => {
            $.get('/api/workstatus', (data) => {
                resolve(data);
            }).fail((err) => {
                reject(err);
            });
        });
    }

    async SaveProxy(address, user, pass, groupData) {
        return await this.PostJson('/api/proxy', {Address: address, User: user, Password: pass, GroupData: groupData });
    }

    async DeleteProxy(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/proxy/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async GetWorkLogs(workId, pageNumber) {
        return new Promise((resolve, reject) => {
            $.get(`/api/workstatus/${workId}/logs`, {page: pageNumber, number: 100}, (data) => {
                resolve(data)
            }).fail((data) => {
                reject(data);
            })
        })
    }

    async CancelWork(workId) {
        return await this.PostJson(`/api/workstatus/${workId}/cancel`, null);
    }

    async ImportAccounts(uploadId, groupName, groupId) {
        return await this.PostJson('/api/account/import', { UploadId: uploadId, GroupName: groupName, GroupId: groupId});
    }

    async ImportEmails(uploadId) {
        return await this.PostJson('/api/email/import', uploadId);
    }

    async ImportNames(uploadId) {
        return await this.PostJson('/api/names/import', uploadId);
    }

    async ImportMacros(uploadId) {
        return await this.PostJson('/api/macros/import', uploadId);
    }

    async PurgeNames() {
        return await this.PostJson('/api/names/purge', null);
    }

    async PurgeMacros() {
        return await this.PostJson('/api/macros/purge', null);
    }
    
    async PurgeAccounts() {
        return await this.PostJson('/api/account/purge', null);
    }

    async RelogAll() {
        return await this.PostJson('/api/account/relogall', null);
    }

    async CleanAll() {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/account/cleanall`, {
                method: 'GET',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async RefreshAll(args) {
        return await this.PostJson('/api/account/refreshall', args);
    }
    async ExportFriends(args) {
        return await this.PostJson('/api/account/exportfriends', args);
    }
    async RelogAccounts(args) {
        return await this.PostJson('/api/account/relogall', args);
    }
    async AcceptAll(args) {
        return await this.PostJson('/api/account/acceptall', args);
    }

    async QuickAdd(args) {
        return await this.PostJson('/api/account/quickadd', args);
    }

    async RemoveFriends(args) {
        return await this.PostJson('/api/account/friendcleaner', args);
    }

    async PurgeEmails() {
        return await this.PostJson('/api/email/purge', null);
    }

    async ImportProxies(uploadId, groupData) {
        return await this.PostJson('/api/proxy/import', { UploadId: uploadId, GroupData: groupData });
    }

    async PurgeProxies() {
        return await this.PostJson('/api/proxy/purge', null);
    }
    
    async SaveKeyword(keyword){
        return await this.PostJson('/api/Keyword', {Name: keyword});    
    }

    async DeleteMacro(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/macros/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    GetKeywords() {
        return new Promise((resolve, reject) => {
            $.get('/api/Keyword', {}, data => {
                resolve(data);
            }).fail((err) => reject(err));
        });
    }

    async DeleteKeyword(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/Keyword/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async DeleteName(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/names/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async DeleteUserName(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/usernames/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async ImportUserNames(uploadId) {
        return await this.PostJson('/api/usernames/import', uploadId);
    }
    
    async ImportKeywords(uploadId) {
        return await this.PostJson('/api/Keyword/import', uploadId);
    }

    async PurgeUserNames() {
        return await this.PostJson('/api/usernames/purge', null);
    }
    
/// PhoneScraper Calls
    async PurgePhoneScraper() {
        return await this.PostJson('/api/PhoneScrape/purge', null);
    }
	
    async SavePhoneScraper(phone, countryCode){
        return await this.PostJson('/api/PhoneScrape', {Number: phone, CountryCode: countryCode});    
    }

    GetPhoneScraper() {
        return new Promise((resolve, reject) => {
            $.get('/api/PhoneScrape', {}, data => {
                resolve(data);
            }).fail((err) => reject(err));
        });
    }

    async DeletePhoneScraper(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/PhoneScrape/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async ImportPhoneScraper(uploadId) {
        return await this.PostJson('/api/PhoneScrape/import', uploadId);
    }
/// EmailScraper Calls
    async PurgeEmailScraper() {
        return await this.PostJson('/api/EmailScrape/purge', null);
    }
	
    async SaveEmailScraper(email){
        return await this.PostJson('/api/EmailScrape', {Address: email});    
    }

    GetEmailScraper() {
        return new Promise((resolve, reject) => {
            $.get('/api/EmailScrape', {}, data => {
                resolve(data);
            }).fail((err) => reject(err));
        });
    }

    async DeleteEmailScraper(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/EmailScrape/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async ImportEmailScraper(uploadId) {
        return await this.PostJson('/api/EmailScrape/import', uploadId);
    }

    async PurgeKeywords() {
        return await this.PostJson('/api/Keyword/purge', null);
    }
    
    async SaveTargetUser(username) {
        return await this.PostJson('/api/targetuser', {Username: username});
    }

    async DeleteTargetUser(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/targetuser/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }

    async ImportTargetUsers(uploadId) {
        return await this.PostJson('/api/targetuser/import', uploadId);
    }
	
	async TotalAccounts() {
        return await this.PostJson('/api/account/total');
    }

    async PurgeTargetUsers() {
        return await this.PostJson('/api/targetuser/purge', null);
    }

    async PurgeAddedTargetUsers() {
        return await this.PostJson('/api/targetuser/purge_added', null);
    }

    async PurgeFilteredTargetUsers(args) {
        return await this.PostJson('/api/targetuser/purge_filtered', args);
    }

    async PurgeFilteredAccounts(args) {
        return await this.PostJson('/api/account/purge_filtered', args);
    }

    async CreateBitmoji(args) {
        return await this.PostJson('/api/account/createbitmoji', args);
    }

    async ExportFilteredTargetUsers(CountryCode, Gender, Race, Added, Searched) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/targetuser/export_filtered/${CountryCode}/${Gender}/${Race}/${Added}/${Searched}`, {
                method: 'GET',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
        //return await this.PostJson('/api/targetuser/export_filtered', args);
    }
    
    async ExportFilteredAccount(emailvalidation, phonevalidation, status, hasadded) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/account/export_filtered/${validationConvert(emailvalidation)}/${validationConvert(phonevalidation)}/${statusConvert(status)}/${hasadded}`, {
                method: 'GET',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
        //return await this.PostJson('/api/targetuser/export_filtered', args);
    }
    
    async DeleteFile(id) {
        return new Promise((resolve, reject) => {
            $.ajax(`/api/upload/${id}`, {
                method: 'DELETE',
                headers: {
                    "X-XSRF-TOKEN": this.GetToken()
                },
                success: (data) => resolve(data),
                error: (data) => reject(data)
            });
        });
    }
    
    async PurgeFiles() {
        return await this.PostJson('/api/upload/purge', null);
    }
    
    async GetAccountGroups(accountId) {
        return new Promise((resolve, reject) => {
            $.get(`/api/Account/groups/${accountId}`, {}, data => {
                resolve(data.data.groups);
            }).fail((err) => reject(err));
        });
    }
    
    async AddAccountToGroup(accountId, groupId) {
        return await this.PostJson('/api/account/groups/add', { AccountId: accountId, GroupId: groupId });
    }
    
    async RemoveAccountFromGroup(accountId, groupId) {
        return await this.PostJson('/api/account/groups/remove', { AccountId: accountId, GroupId: groupId });
    }

    async AddProxyToGroup(proxyId, groupId) {
        return await this.PostJson('/api/proxy/groups/add', { ProxyId: proxyId, GroupId: groupId });
    }

    async GetProxyGroups(proxyId) {
        return new Promise((resolve, reject) => {
            $.get(`/api/Proxy/groups/${proxyId}`, {}, data => {
                resolve(data.data.groups);
            }).fail((err) => reject(err));
        });
    }

    async RemoveProxyFromGroup(proxyId, groupId) {
        return await this.PostJson('/api/proxy/groups/remove', { ProxyId: proxyId, GroupId: groupId });
    }
}