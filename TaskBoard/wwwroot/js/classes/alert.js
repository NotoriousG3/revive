class AlertManager {
    CreateInfo(message) {
        this.CreateAlert(message, "alert-success", "fa fa-check-circle");
    }

    CreateError(message) {
        let msg = `<strong>Oops!</strong> ${message}`;
        this.CreateAlert(msg, "alert-danger", "fa fa-octagon-exclamation");
    }
    
    CreateWarning(message) {
        this.CreateAlert(message, "alert-warning", "fa fa-exclamation-triangle");
    }

    CreateAlert(message, colorClass, iconClass) {
        let alert = $(`<div id="alert" class="d-flex alert ${colorClass} alert-dismissible fade show" role="alert">
           <i class="fa ${iconClass} fa-2x me-2"></i>
            <div>${message}</div>
           <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`);
        let bsAlert = new bootstrap.Alert(alert[0]);

        $('#alertContainer').append(alert);

        setTimeout(() => {
            if (bsAlert) bsAlert.close()
        }, 7000);
    }
}