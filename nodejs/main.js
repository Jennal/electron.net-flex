const { app, BrowserWindow, protocol } = require('electron');
const { spawn } = require('child_process');
const portscanner = require('portscanner');
const path = require('path');
const { dosDateTimeToDate } = require('yauzl');
require('./bufferExtension')(Buffer)
const { NodePack, NodePackType } = require("./nodePack")

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
    var srvPath = path.join(__dirname, "../ElectronFlex/bin/Debug/net5.0/ElectronFlex.exe");
    if (path.basename(app.getAppPath()) == 'app.asar') {
        srvPath = path.join(path.dirname(app.getPath('exe')), "bin/ElectronFlex.exe");
    }

    let child = spawn(srvPath, [
        "--electron",
        "--webport=8000",
        "--wsport=8001"
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

        // let lines = data.split("\n");
        // lines.forEach(line => {
        //     if (line == '') return;

        //     if (line.startsWith("$$$")) {
        //         line = line.substring(3);
        //         console.log("cs eval: " + line);
        //         eval(line);
        //     }
        //     else
        //     {
        //         console.log("CS-OUT: " + line);
        //     } 
        // });
    });

    // child.stdin.setEncoding('utf8');
    // child.stdin.cork();
    // child.stdin.write("Hello cs!\n");
    // child.stdin.uncork();
}