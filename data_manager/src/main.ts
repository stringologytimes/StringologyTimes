import { app, BrowserWindow, ipcMain } from 'electron';
import { join } from 'path';
import { exec } from 'child_process';
import { writeFile } from 'fs/promises';
import { countTitleTagsWithProgress, EntryInfo, saveBigintsAsUint64BinaryAsync } from "./dblp_preprocessor";

let win: BrowserWindow;
function createWindow(): void {
  win = new BrowserWindow({
    width: 900,
    height: 640,
    webPreferences: {
      preload: join(__dirname, 'preload.js'), // 生成後のJSを読む（tscビルド前提）
      nodeIntegration: false,
      contextIsolation: true,
    },
  });

  win.loadFile(join(__dirname, 'index.html'));
}

// 例1: Nodeのバージョンを取得
ipcMain.handle("dblp:start", async (event, { filePath, recordOnly }: { filePath: string; recordOnly: boolean }) => {

    try {
      console.log("start");
      console.log(filePath);

      const records : EntryInfo[] = await countTitleTagsWithProgress(filePath, recordOnly, (p : any) => win.webContents.send("dblp:progress", p));
      await saveBigintsAsUint64BinaryAsync(records, "./dblp.idx");


      win.webContents.send("dblp:done", { total: records.length });
    } catch (err: any) {
      win.webContents.send("dblp:error", { message: String(err?.message ?? err) });
    }
});

ipcMain.handle("dblp:search", async (event, { url, indexFilePath, dblpFilePath }: { url: string; indexFilePath: string; dblpFilePath: string }) => {
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
ipcMain.handle(
  'run:write-file',
  async (_event, args: { filepath: string; text: string }) => {
    const { filepath, text } = args;
    // 必要ならパス検証・拡張子制限などを追加
    await writeFile(filepath, text, { encoding: 'utf-8' });
    return `Wrote ${text.length} bytes to ${filepath}`;
  }
);

app.whenReady().then(createWindow);
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) createWindow();
});