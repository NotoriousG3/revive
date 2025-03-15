class Level {
    static DEBUG = 0;
    static INFO = 1;
    static ERROR = 2;
    static WARNING = 3;

    static ToText(level) {
        switch (level) {
            case this.DEBUG:
                return "DEBUG";
            case this.INFO:
                return "INFO";
            case this.WARNING:
                return "WARNING";
            case this.ERROR:
                return "ERROR";
        }
    }

    static Color(level) {
        switch (level) {
            case this.DEBUG:
                return "text-white-50";
            case this.INFO:
                return "text-white";
            case this.WARNING:
                return "text-warning";
            case this.ERROR:
                return "text-danger";
        }
    }
}

class Line {
    constructor(level, text) {
        this.level = level;
        this.text = text;
    }

    Print() {
        let txt = Level.ToText(this.level);
        return `[${txt}] ${this.text}`;
    }
}

class Logger {
    constructor(containerSelector, alertManager, printToConsole = true) {
        this.container = $(containerSelector);
        this.printToConsole = printToConsole;
        this.alertManager = alertManager;
    }

    CreateLineElement(line) {
        var color = Level.Color(line.level);
        return $(`<div class='console-text ${color}'>${line.Print()}</div>`);
    }

    AppendLine(line) {
        if (this.container[0] !== undefined) {
            this.container.append(this.CreateLineElement(line));
            this.container[0].scrollTop = this.container[0].scrollHeight;
        }

        if (this.printToConsole)
            console.log(line);
    }

    Info(msg) {
        let line = new Line(Level.INFO, msg);
        if (this.alertManager)
            this.alertManager.CreateInfo(msg);
        this.AppendLine(line);
    }

    Debug(msg, alertTimeoutMs = 7000) {
        let line = new Line(Level.DEBUG, msg, alertTimeoutMs);
        this.AppendLine(line);
    }

    Error(msg, alertTimeoutMs = 7000) {
        let line = new Line(Level.ERROR, msg);
        if (this.alertManager)
            this.alertManager.CreateError(msg, alertTimeoutMs);

        this.AppendLine(line);
    }
    
    Warning(msg, alertTimeoutMs = 7000) {
        let line = new Line(Level.WARNING, msg);
        if (this.alertManager)
            this.alertManager.CreateWarning(msg, alertTimeoutMs);

        this.AppendLine(line);
    }

    PrintWorkScheduled(response) {
        let msg = `${response.message}. Click <a href="/workstatus/${response.data}" class="alert-link">here</a> to track its progress`;
        this.Info(msg);
    }

    PrintException(e) {
        if (e.responseJSON != undefined) {
            if (e.responseJSON.errors) {
                let {errors} = e.responseJSON;
                let lines = Object.values(errors).map((field) => field.map(l => l).join("<br />"));

                this.Error(lines.join("<br />"));
                return;
            } else if (e.responseJSON.message) {
                let msg = e.responseJSON.message;

                if (e.responseJSON.data)
                    msg += ` ${e.responseJSON.data.Message}`;

                this.Error(msg);
                return;
            }

            this.Error(e);
        }

        // When we don't have the responseJSON, try to fetch the first line of the text.
        //this.Error(e.responseText.split('\r\n')[0]);
        this.Error(e.message);
    }
}