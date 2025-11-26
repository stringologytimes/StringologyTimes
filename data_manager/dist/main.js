"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const electron_1 = require("electron");
const path_1 = require("path");
const promises_1 = require("fs/promises");
const dblp_preprocessor_1 = require("./dblp_preprocessor");
let win;
function createWindow() {
    win = new electron_1.BrowserWindow({
        width: 900,
        height: 640,
        webPreferences: {
            preload: (0, path_1.join)(__dirname, 'preload.js'), // 生成後のJSを読む（tscビルド前提）
            nodeIntegration: false,
            contextIsolation: true,
        },
    });
    win.loadFile((0, path_1.join)(__dirname, 'index.html'));
}
// 例1: Nodeのバージョンを取得
electron_1.ipcMain.handle("dblp:start", async (event, { filePath, recordOnly }) => {
    try {
        console.log("start");
        console.log(filePath);
        const records = await (0, dblp_preprocessor_1.countTitleTagsWithProgress)(filePath, recordOnly, (p) => win.webContents.send("dblp:progress", p));
        await (0, dblp_preprocessor_1.saveBigintsAsUint64BinaryAsync)(records, "./dblp.idx");
        win.webContents.send("dblp:done", { total: records.length });
    }
    catch (err) {
        win.webContents.send("dblp:error", { message: String(err?.message ?? err) });
    }
});
electron_1.ipcMain.handle("dblp:search", async (event, { url, indexFilePath, dblpFilePath }) => {
    console.log("search");
    /*
    try {
      console.log("start");
  
  
    } catch (err: any) {
      win.webContents.send("dblp:error", { message: String(err?.message ?? err) });
    }
    */
});
// 例2: ファイル書き込み
electron_1.ipcMain.handle('run:write-file', async (_event, args) => {
    const { filepath, text } = args;
    // 必要ならパス検証・拡張子制限などを追加
    await (0, promises_1.writeFile)(filepath, text, { encoding: 'utf-8' });
    return `Wrote ${text.length} bytes to ${filepath}`;
});
electron_1.app.whenReady().then(createWindow);
electron_1.app.on('window-all-closed', () => {
    if (process.platform !== 'darwin')
        electron_1.app.quit();
});
electron_1.app.on('activate', () => {
    if (electron_1.BrowserWindow.getAllWindows().length === 0)
        createWindow();
});
