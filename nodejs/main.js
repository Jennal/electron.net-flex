const { app, BrowserWindow, protocol } = require('electron');
const { spawn } = require('child_process');
const portscanner = require('portscanner');
const path = require('path');

app.on('ready', () => {
    startServer();
    let win = new BrowserWindow({
        width: 640,
        height: 320,
        center: true,
        show: false
    });
    win.loadURL('http://localhost:8000/wwwroot/index.html');
    win.once('ready-to-show', () => {
        win.show();
    });
});

function startServer() {
    var srvPath = path.join(__dirname, "../ElectronFlex/bin/Debug/net5.0/electron.net-flex.exe");
    if (path.basename(app.getAppPath()) == 'app.asar') {
        srvPath = path.join(path.dirname(app.getPath('exe')), "bin/electron.net-flex.exe");
    }

    let child = spawn(srvPath, [
        "--webport=8000",
        "--wsport=8001"
    ]);
    child.stdout.setEncoding('utf8');
    child.stdout.on('data', data => {
        let lines = data.split("\n");
        lines.forEach(line => {
            if (line == '') return;

            if (line.startsWith("$$$")) {
                line = line.substring(3);
                console.log("cs eval: " + line);
                eval(line);
            }
            else
            {
                console.log("CS-OUT: " + line);
            } 
        });
    });

    child.stdin.setEncoding('utf8');
    child.stdin.cork();
    child.stdin.write("Hello cs!\n");
    child.stdin.uncork();
}