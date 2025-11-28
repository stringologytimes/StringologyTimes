import * as fs from 'fs' 
import * as path from "path";
import { URLConverter } from "./basic_functions/url_converter";

const outputFolderPath = "data/auto_generated";
const inputFolderPath = "data/raw/user_files";

/*
const denied_users_raw_text = fs.readFileSync("data/deined_users.csv", 'utf8');
const denied_users = denied_users_raw_text.split("\n");
const denied_user_names : string[] = denied_users.map((user) => user.split(",")[0]);
*/


/**
 * フォルダ X 直下の各サブフォルダ Y にある url.csv を集めて
 * 全 URL の集合を作り、ソートした配列を返す（同期版）。
 */
export function build_unique_url_list(rootDir: string, denied_user_names: Set<string>): string[] {
    const urlSet = new Set<string>();
  
    // X 直下のエントリを取得
    const entries = fs.readdirSync(rootDir, { withFileTypes: true });
  
    for (const entry of entries) {
      if (!entry.isDirectory()) continue; // フォルダ Y だけ対象
  
      const csvPath = path.join(rootDir, entry.name, "url_and_doi.csv");
  
      try {
        const stat = fs.statSync(csvPath);
        if (!stat.isFile()) continue;
  
        // url.csv を同期的に読み込み
        const csvContent = fs.readFileSync(csvPath, "utf8");
  
        // 一列のみの CSV（1 行 1 URL）として処理
        const lines = csvContent.split(/\r?\n/);
        for (const line of lines) {
          const trimmed = line.trim();
          if (!trimmed) continue;
  
          // 念のため 1 列目だけ利用
          const firstCol = trimmed.split(",")[0]?.trim();
          if (firstCol) {
            const doi = URLConverter.convertToDOI(firstCol);
            if(doi != null){
              urlSet.add(doi);
            }else{
              console.log(`Convert Error: ${firstCol} -> null`);
            }
          }
        }
      } catch {
        // url.csv が存在しない／読めない場合はスキップ
        continue;
      }
    }
  
    // 集合 Z をソートして配列にして返す
    return Array.from(urlSet).sort();
  }

  const unique_url_list = build_unique_url_list(inputFolderPath, new Set());
  const output_text = unique_url_list.join("\n");


  try {
      fs.writeFileSync(`${outputFolderPath}/url.csv`, output_text);
      console.log(`Outputted url.csv in ${outputFolderPath}`);
  } catch (e) {
      console.log(e);
  }
