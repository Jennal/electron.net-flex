const { app, BrowserWindow, protocol } = require('electron');
const { spawn } = require('child_process');
const portscanner = require('portscanner');
const path = require('path');
const { dosDateTimeToDate } = require('yauzl');
require('./bufferExtension')(Buffer)
const { NodePack, NodePackType } = require("./nodePack");
const { resolve } = require('path');

//TOOD: find free port, and set to c# server/browser

app.on('ready', async () => {
    let {webPort, wsPort} = await startServer();
    let win = new BrowserWindow({
        width: 640,
        height: 320,
        center: true,
        show: false
    });
    win.loadURL('http://localhost:' + webPort + '/wwwroot/index.html');

    win.webContents.on('did-finish-load', function() {
        win.webContents.executeJavaScript("client.connect('localhost', " + wsPort + ");");
    });

    win.once('ready-to-show', () => {
        win.show();
    });
});

async function startServer() {
        var srvPath = path.join(__dirname, "../ElectronFlex/bin/Debug/net5.0/ElectronFlex.exe");
        if (path.basename(app.getAppPath()) == 'app.asar') {
            srvPath = path.join(path.dirname(app.getPath('exe')), "bin/ElectronFlex.exe");
        }
    
        // Added default port as configurable for port restricted environments.
        let defaultElectronPort = 8000;
        // hostname needs to be localhost, otherwise Windows Firewall will be triggered.
        let webPort = await portscanner.findAPortNotInUse(defaultElectronPort, 65535, 'localhost');
        console.log('Web Port: ' + webPort);
        let wsPort = await portscanner.findAPortNotInUse(webPort+1, 65535, 'localhost');
        console.log('WS Port: ' + wsPort);
                
        let child = spawn(srvPath, [
            "--electron",
            "--webport=" + webPort,
            "--wsport=" + wsPort
        ]);
    
        let buff = Buffer.alloc(128*1024); //128k
    
        // child.stdout.setEncoding('utf8');
        child.stdout.on('data', data => {
            // console.log('data', data, data.toString('utf8'));
            buff = buff.writeBytes(data);
            // console.log('buff', buff);
    
            var pack = NodePack.Decode(buff);
            // console.log("pack", pack);
            while (pack) {
                switch (pack.Type) {
                    case NodePackType.ConsoleOutput:
                        console.log("CS WriteLine: " + pack.Content);
                        break;
                    case NodePackType.InvokeCode:
                        console.log("CS Invoke: " + pack.Content);
                        var result = eval(pack.Content);
                        var json = JSON.stringify(result);
                        json = json === undefined ? 'null' : json;
                        console.log("Invoke Result: " + json);
                        var retPack = new NodePack(pack.Id, NodePackType.InvokeResult, json);
            
                        child.stdin.cork();
                        child.stdin.write(retPack.Encode());
                        child.stdin.uncork();
                        break;
                }
    
                pack = NodePack.Decode(buff);
                // console.log("pack", pack);
            }
        });

        return {
            "webPort": webPort,
            "wsPort": wsPort
        };
}