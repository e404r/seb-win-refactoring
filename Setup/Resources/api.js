


const {app, session, shell, BrowserWindow} = require('electron');
const path = require('path');
const url = require('url');
require('dotenv').config();
let mainWindow;
function createWindow () {
	


  mainWindow = new BrowserWindow({
    transparent: true, 

    
    skipTaskbar: true,
    titleBarStyle: 'hidden',
    width: 1366,
    height: 768,
	  resizable: false,
    fullscreen: false,
	  frame: false,
    backgroundColor: '#840000',
    show: false,
	  icon: __dirname + 'atsu.icns',
    webPreferences: {
    nodeIntegration: true,
    nodeIntegrationInWorker: false,
	  blinkFeatures: 'OverlayScrollbars',
    webviewTag: true
    }
  });




  mainWindow.setMenu(null);
  



mainWindow.loadURL(`file://${__dirname}/index.html`);
  session.defaultSession.webRequest.onBeforeSendHeaders((details, callback) => {
    details.requestHeaders['User-Agent'] = "e404r/local";
    callback({ cancel: false, requestHeaders: details.requestHeaders });
  });



  
mainWindow.webContents.on('new-window', (event, url, frameName, disposition, options, additionalFeatures) => {

     Object.assign(options, {
      parent: mainWindow,
      width: 100,
      height: 100
    })


})


  mainWindow.on('closed', function () {

    mainWindow = null
  })
  
  
  
   mainWindow.once('ready-to-show', () => {
     mainWindow.show()
 })
  
  
  
  
  
  
  
  
  
}





app.on('ready', createWindow);

app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit()
});

app.on('activate', function () {
  if (mainWindow === null) {
  
  }
});



app.on('browser-window-created',function(e,window) {

      window.setMenu(null);

  });

app.allowRendererProcessReuse = true;





if (require('electron-squirrel-startup')) app.quit()

const setupEvents = require('./installers/setup-events')
if (setupEvents.handleSquirrelEvent()) {
  process.exit()
}




