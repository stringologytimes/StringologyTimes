import { createHash } from "crypto";
function hashString(str: string): number {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = (hash * 31 + str.charCodeAt(i)) | 0;
    }
    return hash >>> 0;
}
function sha256Hex(str: string): string {
    return createHash("sha256").update(str, "utf8").digest("hex");
  }

function hslToRgb(h: number, s: number, l: number): { r: number; g: number; b: number } {
    // h: 0–360, s,l: 0–100
    s /= 100;
    l /= 100;
    const c = (1 - Math.abs(2 * l - 1)) * s;
    const hp = h / 60;
    const x = c * (1 - Math.abs((hp % 2) - 1));
    let r1 = 0, g1 = 0, b1 = 0;
    if (0 <= hp && hp < 1) [r1, g1, b1] = [c, x, 0];
    else if (1 <= hp && hp < 2) [r1, g1, b1] = [x, c, 0];
    else if (2 <= hp && hp < 3) [r1, g1, b1] = [0, c, x];
    else if (3 <= hp && hp < 4) [r1, g1, b1] = [0, x, c];
    else if (4 <= hp && hp < 5) [r1, g1, b1] = [x, 0, c];
    else if (5 <= hp && hp < 6) [r1, g1, b1] = [c, 0, x];

    const m = l - c / 2;
    const r = Math.round((r1 + m) * 255);
    const g = Math.round((g1 + m) * 255);
    const b = Math.round((b1 + m) * 255);
    return { r, g, b };
}

function rgbToHex(r: number, g: number, b: number): string {
    return [r, g, b]
        .map(v => v.toString(16).padStart(2, "0"))
        .join(""); // 先頭の # は付けない (shields.io 用)
}

// 文字列 → shields.io で使える HEX 色コード
export function stringToShieldsColor(str: string): string {
    /*
    const hash = hashString(str);
    const hue = hash % 360;
    const saturation = 60;
    const lightness = 55;

    const { r, g, b } = hslToRgb(hue, saturation, lightness);
    return rgbToHex(r, g, b); // 例: "3fa7d6"
    */
    const hex = sha256Hex(str); // 64桁の16進
    return hex.slice(0, 6);     // 先頭6桁をカラーコードとして使う

}
  