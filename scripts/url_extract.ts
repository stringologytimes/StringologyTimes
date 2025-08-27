
import * as fs from 'fs' 

const text : string = fs.readFileSync("data/test.txt", 'utf8');
const URL_RE = /\bhttps?:\/\/[^\s<>"'`]+/gi;
//const URL_RE = /http(s)?:\/\/([\S])+/;
//console.log(text);

/**
 * 末尾の不要な記号を削る
 */
function smartTrim(url: string): string {
  let out = url;
  out = out.replace(/[.,;:!?]+$/u, "");
  return out;
}

/**
 * 与えられたテキストから http/https URL を抽出。
 * - 出現順を保持
 * - 重複は除去
 * - 括弧を含むURLは除外
 * - 実際の改行(\n)を含むURLは除外
 * - 文字列リテラルの \n (バックスラッシュ + n) を含むURLも除外
 */
export function extractHttpUrlsClean(text: string): string[] {
  const seen = new Set<string>();
  const result: string[] = [];

  const matches = text.match(URL_RE) ?? [];
  for (const raw of matches) {
    const cleaned = smartTrim(raw);

    // 括弧を含むURLは除外
    if (/[()[\]{}<>]/.test(cleaned)) continue;
    // 実際の改行を含む場合は除外
    if (cleaned.includes("\n")) continue;
    // 文字列リテラルの \n (バックスラッシュ+n) を含む場合は除外
    if (cleaned.includes("\\n")) continue;

    if (!seen.has(cleaned)) {
      seen.add(cleaned);
      result.push(cleaned);
    }
  }
  return result;
}

// ---- CLI ----
function readStdinSync(): string {
  try {
    return fs.readFileSync(0, "utf8");
  } catch {
    return "";
  }
}

async function main() {
  const args = process.argv.slice(2);
  const asJson = args.includes("--json");
  const files = args.filter((a) => !a.startsWith("-"));


  const urls = extractHttpUrlsClean(text);

  const set = new Set<string>();
  urls.forEach((v) => {
    const b1 = v.indexOf(".gif") == -1;
    const b2 = v.indexOf(".jpg") == -1;
    const b3 = v.indexOf(".png") == -1;
    const b4 = v.indexOf(".pdf") == -1;

    if(b1 && b2 && b3 && b4){
    set.add(v);
    }
  })

  const url_array : string[] = [];
  for(const url of set){
    url_array.push(url);
  }
  url_array.sort();

  url_array.forEach((v) => console.log(v));


}

if (typeof require !== "undefined" && typeof module !== "undefined" && require.main === module) {
  void main();
}