class UsernameManager {
    #m_Api

    constructor(api) {
        this.#m_Api = api;
    }

    async GetUserNames() {
        let result = await this.#m_Api.GetUserNames();
        this.Names = result.data;
    }
    
    async Delete(id) {
        await this.#m_Api.DeleteUserName(id);
    }

    async Upload(fileSelector, callback) {
        let file = $(fileSelector)[0].files[0];
        let formData = new FormData();
        formData.append("inputFile", file);
        formData.append("skipCache", "0");

        let uploadResponse = await this.#m_Api.UploadFile(formData);
        let result = await this.#m_Api.ImportUserNames(uploadResponse.data);

        if (callback !== undefined) callback();

        return result;
    }
    
    async Purge() {
        return await this.#m_Api.PurgeUserNames();
    }
}