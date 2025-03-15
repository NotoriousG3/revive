class EmailManager {
    #m_Api

    constructor(api) {
        this.#m_Api = api;
    }

    async GetEmails() {
        let result = await this.#m_Api.GetEmails();
        this.Emails = result.data;
    }

    async Delete(id) {
        await this.#m_Api.DeleteEmail(id);
    }

    async Upload(fileSelector, callback) {
        let file = $(fileSelector)[0].files[0];
        let formData = new FormData();
        formData.append("inputFile", file);
        formData.append("skipCache", "0");

        let uploadResponse = await this.#m_Api.UploadFile(formData);
        let result = await this.#m_Api.ImportEmails(uploadResponse.data);

        if (callback !== undefined) callback();

        return result;
    }
    
    async Purge() {
        return await this.#m_Api.PurgeEmails();
    }
}