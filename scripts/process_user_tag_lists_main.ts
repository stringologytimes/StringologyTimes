import * as fs from 'fs' 
import * as path from "path";
import { URLConverter } from "./basic_functions/url_converter";

const outputFolderPath = "data/auto_generated";
const inputFolderPath = "data/raw/user_files";


export function build_unique_tag_list(rootDir: string, denied_user_names: Set<string>): string[] {
    const url_tag_map = new Map<string, Set<string>>();
  
    // X 直下のエントリを取得
    const entries = fs.readdirSync(rootDir, { withFileTypes: true });
  
    for (const entry of entries) {
      if (!entry.isDirectory()) continue; // フォルダ Y だけ対象
  
      const csvPath = path.join(rootDir, entry.name, "tag.csv");
  
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
                if(!url_tag_map.has(doi)){
                    url_tag_map.set(doi, new Set<string>());
                }

                const cols = trimmed.split(",");
                for(let i = 1; i < cols.length; i++){
                    const tag = cols[i].trim();
                    url_tag_map.get(doi)!.add(tag);
                }

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

    const result_list : string[] = new Array();
    url_tag_map.forEach((value, key) => {
        Array.from(value).forEach((tag) => {
            result_list.push(`${key}, ${tag}`);
        })
    });
    result_list.sort();
    return result_list;
  }

  const unique_tag_list = build_unique_tag_list(inputFolderPath, new Set());
  const output_text = unique_tag_list.join("\n");


  try {
      fs.writeFileSync(`${outputFolderPath}/tag.csv`, output_text);
      console.log(`Outputted tag.csv in ${outputFolderPath}`);
  } catch (e) {
      console.log(e);
  }
