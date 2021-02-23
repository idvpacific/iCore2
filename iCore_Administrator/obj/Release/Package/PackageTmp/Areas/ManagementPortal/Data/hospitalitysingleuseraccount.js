function T_Account_Login_Clear() {
    document.getElementById("TXT_ACC_Login_Email").value = "";
    document.getElementById("TXT_ACC_Login_Username").value = "";
    document.getElementById("TXT_ACC_Login_Password").value = "";
    document.getElementById("TXT_ACC_Login_Email").focus();
}
function T_Account_Login_Reload() {
    try {
        document.getElementById("ACC_Login").style.opacity = "0.7";
        document.getElementById("ACC_Login").style.pointerEvents = "none";
        document.getElementById("ACC_Login_Wait").style.display = "block";
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        $.ajax({
            url: app.Urls.U3, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("ACC_Login_Wait").style.display = "none";
                document.getElementById("ACC_Login").style.opacity = "1";
                document.getElementById("ACC_Login").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to reload user information', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    T_Account_Login_Clear();
                    toastr.success("User information successfully reloaded", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    document.getElementById("ACC_Login_Wait").style.display = "none";
                    document.getElementById("ACC_Login").style.opacity = "1";
                    document.getElementById("ACC_Login").style.pointerEvents = "auto";
                    var Sepval = ErrorResult.split("#");
                    document.getElementById("TXT_ACC_Login_Email").value = Sepval[0].trim();
                    document.getElementById("TXT_ACC_Login_Username").value = Sepval[1].trim();
                    document.getElementById("TXT_ACC_Login_Password").value = Sepval[2].trim();
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("ACC_Login_Wait").style.display = "none";
                        document.getElementById("ACC_Login").style.opacity = "1";
                        document.getElementById("ACC_Login").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("ACC_Login_Wait").style.display = "none";
        document.getElementById("ACC_Login").style.opacity = "1";
        document.getElementById("ACC_Login").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to reload user information', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function T_Account_Login_Save() {
    try {
        document.getElementById("ACC_Login").style.opacity = "0.7";
        document.getElementById("ACC_Login").style.pointerEvents = "none";
        document.getElementById("ACC_Login_Wait").style.display = "block";
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        formData.append("R1", document.getElementById("TXT_ACC_Login_Email").value);
        formData.append("R2", document.getElementById("TXT_ACC_Login_Username").value);
        formData.append("R3", document.getElementById("TXT_ACC_Login_Password").value);
        $.ajax({
            url: app.Urls.U4, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("ACC_Login_Wait").style.display = "none";
                document.getElementById("ACC_Login").style.opacity = "1";
                document.getElementById("ACC_Login").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to save changes', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    document.getElementById("ACC_Login_Wait").style.display = "none";
                    document.getElementById("ACC_Login").style.opacity = "1";
                    document.getElementById("ACC_Login").style.pointerEvents = "auto";
                    toastr.success("User account login information successfully saved", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("ACC_Login_Wait").style.display = "none";
                        document.getElementById("ACC_Login").style.opacity = "1";
                        document.getElementById("ACC_Login").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("ACC_Login_Wait").style.display = "none";
        document.getElementById("ACC_Login").style.opacity = "1";
        document.getElementById("ACC_Login").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to save changes', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function T_Account_API_Clear() {
    document.getElementById("TXT_API_Auth_Username").value = "";
    document.getElementById("TXT_API_Auth_Key").value = "";
    document.getElementById("TXT_API_Get_Username").value = "";
    document.getElementById("TXT_API_Get_Key").value = "";
    document.getElementById("TXT_API_Post_Username").value = "";
    document.getElementById("TXT_API_Post_Key").value = "";
    document.getElementById("TXT_API_Auth_Username").focus();
}
function T_Account_API_Reload() {
    try {
        document.getElementById("ACC_API").style.opacity = "0.7";
        document.getElementById("ACC_API").style.pointerEvents = "none";
        document.getElementById("ACC_API_Wait").style.display = "block";
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        $.ajax({
            url: app.Urls.U5, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("ACC_API_Wait").style.display = "none";
                document.getElementById("ACC_API").style.opacity = "1";
                document.getElementById("ACC_API").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to reload user information', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    T_Account_API_Clear();
                    toastr.success("User information successfully reloaded", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    document.getElementById("ACC_API_Wait").style.display = "none";
                    document.getElementById("ACC_API").style.opacity = "1";
                    document.getElementById("ACC_API").style.pointerEvents = "auto";
                    var Sepval = ErrorResult.split("#");
                    document.getElementById("TXT_API_Auth_Username").value = Sepval[0].trim();
                    document.getElementById("TXT_API_Auth_Key").value = Sepval[1].trim();
                    document.getElementById("TXT_API_Get_Username").value = Sepval[2].trim();
                    document.getElementById("TXT_API_Get_Key").value = Sepval[3].trim();
                    document.getElementById("TXT_API_Post_Username").value = Sepval[4].trim();
                    document.getElementById("TXT_API_Post_Key").value = Sepval[5].trim();
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("ACC_API_Wait").style.display = "none";
                        document.getElementById("ACC_API").style.opacity = "1";
                        document.getElementById("ACC_API").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("ACC_API_Wait").style.display = "none";
        document.getElementById("ACC_API").style.opacity = "1";
        document.getElementById("ACC_API").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to reload user information', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function T_Account_API_Save() {
    try {
        document.getElementById("ACC_API").style.opacity = "0.7";
        document.getElementById("ACC_API").style.pointerEvents = "none";
        document.getElementById("ACC_API_Wait").style.display = "block";
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        formData.append("R1", document.getElementById("TXT_API_Auth_Username").value);
        formData.append("R2", document.getElementById("TXT_API_Auth_Key").value);
        formData.append("R3", document.getElementById("TXT_API_Get_Username").value);
        formData.append("R4", document.getElementById("TXT_API_Get_Key").value);
        formData.append("R5", document.getElementById("TXT_API_Post_Username").value);
        formData.append("R6", document.getElementById("TXT_API_Post_Key").value);
        $.ajax({
            url: app.Urls.U6, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("ACC_API_Wait").style.display = "none";
                document.getElementById("ACC_API").style.opacity = "1";
                document.getElementById("ACC_API").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to save changes', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    document.getElementById("ACC_API_Wait").style.display = "none";
                    document.getElementById("ACC_API").style.opacity = "1";
                    document.getElementById("ACC_API").style.pointerEvents = "auto";
                    toastr.success("User account API information successfully saved", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("ACC_API_Wait").style.display = "none";
                        document.getElementById("ACC_API").style.opacity = "1";
                        document.getElementById("ACC_API").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("ACC_API_Wait").style.display = "none";
        document.getElementById("ACC_API").style.opacity = "1";
        document.getElementById("ACC_API").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to save changes', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function ReloadKey(KID) {
    try {
        switch (KID) {
            case "1": {
                document.getElementById("ACC_Login").style.opacity = "0.7";
                document.getElementById("ACC_Login").style.pointerEvents = "none";
                document.getElementById("ACC_Login_Wait").style.display = "block";
                break;
            }
            case "2": {
                document.getElementById("ACC_Login").style.opacity = "0.7";
                document.getElementById("ACC_Login").style.pointerEvents = "none";
                document.getElementById("ACC_Login_Wait").style.display = "block";
                break;
            }

            case "3": {
                document.getElementById("ACC_API").style.opacity = "0.7";
                document.getElementById("ACC_API").style.pointerEvents = "none";
                document.getElementById("ACC_API_Wait").style.display = "block";
                break;
            }
            case "4": {
                document.getElementById("ACC_API").style.opacity = "0.7";
                document.getElementById("ACC_API").style.pointerEvents = "none";
                document.getElementById("ACC_API_Wait").style.display = "block";
                break;
            }
            case "5": {
                document.getElementById("ACC_API").style.opacity = "0.7";
                document.getElementById("ACC_API").style.pointerEvents = "none";
                document.getElementById("ACC_API_Wait").style.display = "block";
                break;
            }
            case "6": {
                document.getElementById("ACC_API").style.opacity = "0.7";
                document.getElementById("ACC_API").style.pointerEvents = "none";
                document.getElementById("ACC_API_Wait").style.display = "block";
                break;
            }
            case "7": {
                document.getElementById("ACC_API").style.opacity = "0.7";
                document.getElementById("ACC_API").style.pointerEvents = "none";
                document.getElementById("ACC_API_Wait").style.display = "block";
                break;
            }
            case "8": {
                document.getElementById("ACC_API").style.opacity = "0.7";
                document.getElementById("ACC_API").style.pointerEvents = "none";
                document.getElementById("ACC_API_Wait").style.display = "block";
                break;
            }
        }
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        formData.append("KID", KID);
        $.ajax({
            url: app.Urls.U1, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById("ACC_API_Wait").style.display = "none";
                document.getElementById("ACC_API").style.opacity = "1";
                document.getElementById("ACC_API").style.pointerEvents = "auto";
                document.getElementById("ACC_Login_Wait").style.display = "none";
                document.getElementById("ACC_Login").style.opacity = "1";
                document.getElementById("ACC_Login").style.pointerEvents = "auto";
                toastr.error('An error occurred while processing your request to get new key', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    switch (KID) {
                        case "1": { document.getElementById("TXT_ACC_Login_Username").value = ErrorResult.trim(); break; }
                        case "2": { document.getElementById("TXT_ACC_Login_Password").value = ErrorResult.trim(); break; }
                        case "3": { document.getElementById("TXT_API_Auth_Username").value = ErrorResult.trim(); break; }
                        case "4": { document.getElementById("TXT_API_Auth_Key").value = ErrorResult.trim(); break; }
                        case "5": { document.getElementById("TXT_API_Get_Username").value = ErrorResult.trim(); break; }
                        case "6": { document.getElementById("TXT_API_Get_Key").value = ErrorResult.trim(); break; }
                        case "7": { document.getElementById("TXT_API_Post_Username").value = ErrorResult.trim(); break; }
                        case "8": { document.getElementById("TXT_API_Post_Key").value = ErrorResult.trim(); break; }
                    }
                    document.getElementById("ACC_API_Wait").style.display = "none";
                    document.getElementById("ACC_API").style.opacity = "1";
                    document.getElementById("ACC_API").style.pointerEvents = "auto";
                    document.getElementById("ACC_Login_Wait").style.display = "none";
                    document.getElementById("ACC_Login").style.opacity = "1";
                    document.getElementById("ACC_Login").style.pointerEvents = "auto";
                    toastr.success("New key successfully generated", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById("ACC_API_Wait").style.display = "none";
                        document.getElementById("ACC_API").style.opacity = "1";
                        document.getElementById("ACC_API").style.pointerEvents = "auto";
                        document.getElementById("ACC_Login_Wait").style.display = "none";
                        document.getElementById("ACC_Login").style.opacity = "1";
                        document.getElementById("ACC_Login").style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        document.getElementById("ACC_API_Wait").style.display = "none";
        document.getElementById("ACC_API").style.opacity = "1";
        document.getElementById("ACC_API").style.pointerEvents = "auto";
        document.getElementById("ACC_Login_Wait").style.display = "none";
        document.getElementById("ACC_Login").style.opacity = "1";
        document.getElementById("ACC_Login").style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to get new key', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}