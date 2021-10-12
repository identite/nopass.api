const URLsignalR = "/signalR";
const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(URLsignalR)
    .configureLogging(signalR.LogLevel.Information)
    .build();
hubConnection.serverTimeoutInMilliseconds = 60000;

// This method is called server side in the "AuthorizedUser" method of the controller.
// The received link and token will be used to authenticate the user and login.
hubConnection.on('Redirect', function (urlRedirect, token) {
    window.location.replace(urlRedirect + token);
});

// This method is called server side in the "UpdatePicture" method of the controller.
// The image and timer will be updated.
hubConnection.on('ChangeImage', function (newImageCaptcha, newNextChange) {
    var imageCaptcha = document.getElementsByClassName("imageNoPass");
    imageCaptcha[0].src = 'data:image/png;base64,' + newImageCaptcha;
    nextChange = parseInt(newNextChange);
});

// This method is called server side in the “AuthorizedUser” method of the controller, 
// and the service “LoginNoPassService” to notify the user about login errors.
hubConnection.on('ShowError', function (message) {
    var imageCaptcha = document.getElementsByClassName("imageNoPass");
    var textTimer = document.getElementsByClassName("nextChange");
    imageCaptcha[0].parentNode.removeChild(imageCaptcha[0])
    textTimer[0].parentNode.removeChild(textTimer[0])

    var showError = document.getElementsByClassName("showError");
    showError[0].innerHTML = message;
});

startSignalR();
startTimer();

// In this method, the hubConnection object of the SignalR
// library is initialized by the frontend to notify the backend.
async function startSignalR() {
    try {
        await hubConnection.start();
        console.log(`SignalR Connected to ${URLsignalR}`);
        hubConnection.invoke('SetConnectionID', authId)
    } catch (err) {
        console.log(err);
        setTimeout(startSignalR, 5000);
    }
};

async function startTimer() {
    var textTimer = document.getElementsByClassName("nextChange");
    nextChange -= 1000;
    textTimer[0].innerHTML = Math.round(nextChange / 1000);
    setInterval(subtractOneSecond, 1000, textTimer[0]);
};

function subtractOneSecond(timer) {
    if (nextChange > 1000) {
        timer.innerHTML = Math.round(nextChange / 1000);
        nextChange -= 1000;
    }
    else {
        timer.innerHTML = 1;
        nextChange -= 1000;
    }
};