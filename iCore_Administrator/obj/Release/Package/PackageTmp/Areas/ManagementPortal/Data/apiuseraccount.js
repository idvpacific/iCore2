//Acount functions
var T3LD = 0;
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
//Reload dynamic tabs information
function ReloadTab(TInd) {
    switch (TInd) {
        case '3': { if (T3LD == 0) { T_AccessPolicy_Reload(); } }
    }
}
//Access policy functions
function T_AccessPolicy_Clear() { var clist = document.getElementsByClassName("AccPCB"); for (var i = 0; i < clist.length; ++i) { clist[i].checked = false; }; document.getElementById('Txt_ADR').value = "3"; $('#Txt_ADR_Type').val("3"); $('#Txt_ADR_Type').select2({ dropdownAutoWidth: true, width: '100%' }).trigger('change'); }
function T_AccessPolicy_Reload() {
    try {
        if (T3LD == 0) { T_AccessPolicy_Clear(); }
        T3LD = 1;
        toastr.clear();
        document.getElementById('Tab_Wait_3').style.display = "block";
        document.getElementById('Tab_3').style.display = "none";
        toastr.info('The system is reviewing your request', 'Please wait ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        $.ajax({
            url: app.Urls.U7, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById('Tab_Wait_3').style.display = "none";
                document.getElementById('Tab_3').style.display = "block";
                toastr.clear();
                toastr.error('An error occurred while processing your request to reload access policy information', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    toastr.clear();
                    T_AccessPolicy_Clear();
                    var res = ErrorResult.split("-");
                    if (res[0].trim() == "1") { document.getElementById('C_Portal_UPA').checked = true; }
                    document.getElementById('Txt_ADR').value = res[1].trim();
                    $('#Txt_ADR_Type').val(res[2].trim());
                    $('#Txt_ADR_Type').select2({ dropdownAutoWidth: true, width: '100%' }).trigger('change');
                    if (res[3].trim() == "1") { document.getElementById('C_API_IDV_OCR').checked = true; }
                    if (res[4].trim() == "1") { document.getElementById('C_API_IDV_DrivingLic').checked = true; }
                    if (res[5].trim() == "1") { document.getElementById('C_API_IDV_DrivingLic_Valication').checked = true; }
                    if (res[6].trim() == "1") { document.getElementById('C_API_IDV_Passport_OCR').checked = true; }
                    if (res[7].trim() == "1") { document.getElementById('C_API_IDV_Passport_validation').checked = true; }
                    if (res[8].trim() == "1") { document.getElementById('C_API_Acuant_OCR_IDCard').checked = true; }
                    if (res[9].trim() == "1") { document.getElementById('C_API_Acuant_Validation_IDCard').checked = true; }
                    if (res[10].trim() == "1") { document.getElementById('C_API_Acuant_OCR_Passport').checked = true; }
                    if (res[11].trim() == "1") { document.getElementById('C_API_Acuant_Validation_Passport').checked = true; }
                    if (res[12].trim() == "1") { document.getElementById('C_API_TVS_Passport').checked = true; }
                    if (res[13].trim() == "1") { document.getElementById('C_API_Vevo_Check').checked = true; }
                    if (res[14].trim() == "1") { document.getElementById('C_API_GIC').checked = true; }
                    if (res[15].trim() == "1") { document.getElementById('C_API_LNT').checked = true; }
                    if (res[16].trim() == "1") { document.getElementById('C_API_FRM').checked = true; }
                    if (res[17].trim() == "1") { document.getElementById('C_API_RetriveAllTrans_Data').checked = true; }
                    if (res[18].trim() == "1") { document.getElementById('C_API_RetriveAllTrans_File').checked = true; }
                    document.getElementById('Tab_Wait_3').style.display = "none";
                    document.getElementById('Tab_3').style.display = "block";
                    toastr.success("API user access policy successfully reloaded", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                }
                else {
                    if (ErrorCode != "2") {
                        document.getElementById('Tab_Wait_3').style.display = "none";
                        document.getElementById('Tab_3').style.display = "block";
                        toastr.clear();
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        toastr.clear();
        document.getElementById('Tab_Wait_3').style.display = "none";
        document.getElementById('Tab_3').style.display = "block";
        toastr.error('An error occurred while processing your request to reload access policy information', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function T_AccessPolicy_Save() {
    try {
        toastr.clear();
        document.getElementById('Tab_Wait_3').style.display = "block";
        document.getElementById('Tab_3').style.opacity = "0.7";
        document.getElementById('Tab_3').style.pointerEvents = "none";
        toastr.info('The system is reviewing your request', 'Please wait ...', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
        var formData = new FormData();
        formData.append("UI", app.Urls.UID);
        if (document.getElementById('C_Portal_UPA').checked == true) { formData.append("C1", "1"); } else { formData.append("C1", "0"); }
        formData.append("C2", document.getElementById('Txt_ADR').value.trim());
        formData.append("C3", $("#Txt_ADR_Type :selected").val().trim());
        formData.append("C4", $("#Txt_ADR_Type :selected").text().trim());
        if (document.getElementById('C_API_IDV_OCR').checked == true) { formData.append("C5", "1"); } else { formData.append("C5", "0"); }
        if (document.getElementById('C_API_IDV_DrivingLic').checked == true) { formData.append("C6", "1"); } else { formData.append("C6", "0"); }
        if (document.getElementById('C_API_IDV_DrivingLic_Valication').checked == true) { formData.append("C7", "1"); } else { formData.append("C7", "0"); }
        if (document.getElementById('C_API_IDV_Passport_OCR').checked == true) { formData.append("C8", "1"); } else { formData.append("C8", "0"); }
        if (document.getElementById('C_API_IDV_Passport_validation').checked == true) { formData.append("C9", "1"); } else { formData.append("C9", "0"); }
        if (document.getElementById('C_API_Acuant_OCR_IDCard').checked == true) { formData.append("C10", "1"); } else { formData.append("C10", "0"); }
        if (document.getElementById('C_API_Acuant_Validation_IDCard').checked == true) { formData.append("C11", "1"); } else { formData.append("C11", "0"); }
        if (document.getElementById('C_API_Acuant_OCR_Passport').checked == true) { formData.append("C12", "1"); } else { formData.append("C12", "0"); }
        if (document.getElementById('C_API_Acuant_Validation_Passport').checked == true) { formData.append("C13", "1"); } else { formData.append("C13", "0"); }
        if (document.getElementById('C_API_TVS_Passport').checked == true) { formData.append("C14", "1"); } else { formData.append("C14", "0"); }
        if (document.getElementById('C_API_Vevo_Check').checked == true) { formData.append("C15", "1"); } else { formData.append("C15", "0"); }
        if (document.getElementById('C_API_GIC').checked == true) { formData.append("C16", "1"); } else { formData.append("C16", "0"); }
        if (document.getElementById('C_API_LNT').checked == true) { formData.append("C17", "1"); } else { formData.append("C17", "0"); }
        if (document.getElementById('C_API_FRM').checked == true) { formData.append("C18", "1"); } else { formData.append("C18", "0"); }
        if (document.getElementById('C_API_RetriveAllTrans_Data').checked == true) { formData.append("C19", "1"); } else { formData.append("C19", "0"); }
        if (document.getElementById('C_API_RetriveAllTrans_File').checked == true) { formData.append("C20", "1"); } else { formData.append("C20", "0"); }
        $.ajax({
            url: app.Urls.U8, type: "POST", data: formData, dataType: 'json', contentType: false, processData: false, async: true,
            error: function () {
                document.getElementById('Tab_Wait_3').style.display = "none";
                document.getElementById('Tab_3').style.opacity = "1";
                document.getElementById('Tab_3').style.pointerEvents = "auto";
                toastr.clear();
                toastr.error('An error occurred while processing your request to save access policy changes', 'An error has occurred! [C02]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
            },
            success: function (data) {
                var ErrorCode = ""; var ErrorResult = "";
                $.each(data, function (i, item) { ErrorResult = item.Text; ErrorCode = item.Value; });
                if (ErrorCode == "0") {
                    toastr.clear();
                    document.getElementById('Tab_Wait_3').style.display = "none";
                    document.getElementById('Tab_3').style.opacity = "1";
                    document.getElementById('Tab_3').style.pointerEvents = "auto";
                    toastr.success("API user access policy successfully saved changes", 'Congratulations', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                }
                else {
                    if (ErrorCode != "2") {
                        toastr.clear();
                        document.getElementById('Tab_Wait_3').style.display = "none";
                        document.getElementById('Tab_3').style.opacity = "1";
                        document.getElementById('Tab_3').style.pointerEvents = "auto";
                        toastr.error(ErrorResult.trim(), 'An error has occurred! [C03]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
                    }
                    else { window.location.href = app.Urls.U2; }
                }
            }
        });
    } catch (e) {
        toastr.clear();
        document.getElementById('Tab_Wait_3').style.display = "none";
        document.getElementById('Tab_3').style.opacity = "1";
        document.getElementById('Tab_3').style.pointerEvents = "auto";
        toastr.error('An error occurred while processing your request to save access policy changes', 'An error has occurred! [C01]', { "showMethod": "slideDown", "hideMethod": "slideUp", positionClass: 'toast-bottom-right', "progressBar": true, "closeButton": true, "timeOut": 8000 });
    }
}
function T_AccessPolicy_CheckAll() { var clist = document.getElementsByClassName("AccPCB"); for (var i = 0; i < clist.length; ++i) { clist[i].checked = true; }; }
function T_AccessPolicy_UncheckAll() { var clist = document.getElementsByClassName("AccPCB"); for (var i = 0; i < clist.length; ++i) { clist[i].checked = false; }; }