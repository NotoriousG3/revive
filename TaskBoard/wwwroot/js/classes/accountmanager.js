class AccountManager {
    #m_Api
    #m_Logger

    constructor(api, logger) {
        this.#m_Api = api;
        this.#m_Logger = logger
    }

    async ChangeUsername(ID, oldName, newName) {
        this.#m_Logger.Debug(`Attempting to change ${oldName} username to ${newName}.`);

        let args = CreateActionArguments({
            AccID: ID,
            OldName: oldName,
            NewName: newName,
            ScheduledTime: new Date($.now())
        });
        
        return await this.#m_Api.ChangeUsername(args);
    }
    
    async CreateMultiple(number, creationArguments) {
        this.#m_Logger.Debug(`Starting account creation process for ${number} accounts`);

        return await this.#m_Api.CreateRandomAccount(number, creationArguments);
    }
	
	async GetTotalAccounts() {
        return await this.#m_Api.TotalAccounts();
    }

    async Delete(id) {
        return await this.#m_Api.DeleteAccount(id);
    }

    async Relog(id) {
        return await this.#m_Api.RelogAccount(id);
    }

    async LoadFriends(id) {
        return await this.#m_Api.LoadFriends(id);
    }

    async AcceptFriends(id) {
        return await this.#m_Api.AcceptFriends(id);
    }

    async Upload(fileSelector, groupName, groupId) {
        const file = $(fileSelector)[0].files[0];
        const formData = new FormData();
        formData.append("inputFile", file);
        formData.append("skipCache", "0");

        // This brings back the id of the upload
        let uploadResponse = await this.#m_Api.UploadFile(formData);
        
        let response = await this.#m_Api.ImportAccounts(uploadResponse.data, groupName, groupId);

        return response.data;
    }

    async Purge() {
        return await this.#m_Api.PurgeAccounts();
    }
    async CleanAll() {
        return await this.#m_Api.CleanAll();
    }
    async RelogAll() {
        return await this.#m_Api.RelogAll();
    }
    async RefreshAll() {
        return await this.#m_Api.RefreshAll();
    }
    async AcceptAll(args) {
        return await this.#m_Api.AcceptAll(args);
    }
}