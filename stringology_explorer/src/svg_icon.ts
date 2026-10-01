function escapeSvgText(text: string): string {
    return text
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
}

function measureTextWidth(text: string, font: string): number {
    const canvas = document.createElement("canvas");
    const context = canvas.getContext("2d");

    if (context === null) {
        throw new Error("Failed to get canvas context.");
    }

    context.font = font;
    return context.measureText(text).width;
}
function escapeSvgAttribute(value: string): string {
    return value
        .replace(/&/g, "&amp;")
        .replace(/"/g, "&quot;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
}
function createIconSvg(
    text: string,
    fontSize: number,
    backgroundColor: string,
    textColor: string
): string {
    const { resolvedBackgroundColor, resolvedTextColor } =
        resolveIconColors(text, backgroundColor, textColor);

    const safeText = escapeSvgText(text);
    const safeBackgroundColor = escapeSvgAttribute(resolvedBackgroundColor);
    const safeTextColor = escapeSvgAttribute(resolvedTextColor);

    const fontFamily = "sans-serif";
    const fontWeight = "bold";
    const font = `${fontWeight} ${fontSize}px ${fontFamily}`;

    const paddingX = fontSize * 0.75;
    const paddingY = fontSize * 0.35;

    const textWidth = measureTextWidth(text, font);

    const width = Math.ceil(textWidth + paddingX * 2);
    const height = Math.ceil(fontSize + paddingY * 2);

    return `
      <svg xmlns="http://www.w3.org/2000/svg"
           width="${width}"
           height="${height}"
           viewBox="0 0 ${width} ${height}">
        <rect width="${width}"
              height="${height}"
              rx="${height / 2}"
              fill="${safeBackgroundColor}"/>
        <text x="50%" y="50%"
              font-size="${fontSize}"
              font-family="${fontFamily}"
              font-weight="${fontWeight}"
              fill="${safeTextColor}"
              text-anchor="middle"
              dominant-baseline="central">${safeText}</text>
      </svg>
    `;
}

function resolveIconColors(
    text: string,
    backgroundColor: string,
    textColor: string
): { resolvedBackgroundColor: string; resolvedTextColor: string } {
    const bgIsRandom = backgroundColor.toLowerCase() === "random";
    const textIsRandom = textColor.toLowerCase() === "random";

    const seed = hashString(text);

    // 背景色・文字色ともに random:
    // text から決まる背景色 + 白/黒のうち見やすい方
    if (bgIsRandom && textIsRandom) {
        const bgRgb = seededReadableBackgroundRgb(seed);
        const fgRgb = pickReadableTextColor(bgRgb);

        return {
            resolvedBackgroundColor: rgbToHex(bgRgb),
            resolvedTextColor: rgbToHex(fgRgb),
        };
    }

    // 背景色だけ random:
    // text から決まる背景色をベースにし、
    // 指定文字色とのコントラストが足りなければ seed をずらして再探索
    if (bgIsRandom && !textIsRandom) {
        const fgRgb = hexToRgb(textColor);

        for (let i = 0; i < 32; i++) {
            const bgRgb = seededReadableBackgroundRgb(seed + i * 2654435761);

            if (contrastRatio(bgRgb, fgRgb) >= 4.5) {
                return {
                    resolvedBackgroundColor: rgbToHex(bgRgb),
                    resolvedTextColor: textColor,
                };
            }
        }

        // フォールバック
        const bgRgb = seededReadableBackgroundRgb(seed);
        return {
            resolvedBackgroundColor: rgbToHex(bgRgb),
            resolvedTextColor: textColor,
        };
    }

    // 文字色だけ random:
    // 背景に対して白/黒のうち見やすい方
    if (!bgIsRandom && textIsRandom) {
        const bgRgb = hexToRgb(backgroundColor);
        const fgRgb = pickReadableTextColor(bgRgb);

        return {
            resolvedBackgroundColor: backgroundColor,
            resolvedTextColor: rgbToHex(fgRgb),
        };
    }

    // どちらも固定
    return {
        resolvedBackgroundColor: backgroundColor,
        resolvedTextColor: textColor,
    };
}

function pickReadableTextColor(
    bgRgb: [number, number, number]
): [number, number, number] {
    const white: [number, number, number] = [255, 255, 255];
    const black: [number, number, number] = [0, 0, 0];

    const whiteContrast = contrastRatio(bgRgb, white);
    const blackContrast = contrastRatio(bgRgb, black);

    return whiteContrast >= blackContrast ? white : black;
}

function hashString(s: string): number {
    // FNV-1a
    let hash = 2166136261;

    for (let i = 0; i < s.length; i++) {
        hash ^= s.charCodeAt(i);
        hash = Math.imul(hash, 16777619);
    }

    return hash >>> 0;
}

function seededReadableBackgroundRgb(seed: number): [number, number, number] {
    // text から決まる HSL を使って背景色を作る
    // 極端に見づらい色を避けるため、彩度・明度をある程度制限する
    const hue = seed % 360;
    const saturation = 60 + ((seed >>> 8) % 21); // 60..80
    const lightness = 35 + ((seed >>> 16) % 21); // 35..55

    return hslToRgb(hue, saturation, lightness);
}

function hslToRgb(h: number, s: number, l: number): [number, number, number] {
    s /= 100;
    l /= 100;

    const c = (1 - Math.abs(2 * l - 1)) * s;
    const x = c * (1 - Math.abs((h / 60) % 2 - 1));
    const m = l - c / 2;

    let r = 0, g = 0, b = 0;

    if (h < 60) [r, g, b] = [c, x, 0];
    else if (h < 120) [r, g, b] = [x, c, 0];
    else if (h < 180) [r, g, b] = [0, c, x];
    else if (h < 240) [r, g, b] = [0, x, c];
    else if (h < 300) [r, g, b] = [x, 0, c];
    else [r, g, b] = [c, 0, x];

    return [
        Math.round((r + m) * 255),
        Math.round((g + m) * 255),
        Math.round((b + m) * 255),
    ];
}

function rgbToHex([r, g, b]: [number, number, number]): string {
    return (
        "#" +
        [r, g, b]
            .map(v => v.toString(16).padStart(2, "0"))
            .join("")
    );
}

function hexToRgb(hex: string): [number, number, number] {
    const normalized = hex.trim().replace(/^#/, "");

    if (!/^[0-9a-fA-F]{6}$/.test(normalized)) {
        throw new Error(`Invalid hex color: ${hex}`);
    }

    const r = parseInt(normalized.slice(0, 2), 16);
    const g = parseInt(normalized.slice(2, 4), 16);
    const b = parseInt(normalized.slice(4, 6), 16);

    return [r, g, b];
}

function relativeLuminance([r, g, b]: [number, number, number]): number {
    const convert = (v: number) => {
        const x = v / 255;
        return x <= 0.03928
            ? x / 12.92
            : Math.pow((x + 0.055) / 1.055, 2.4);
    };

    const R = convert(r);
    const G = convert(g);
    const B = convert(b);

    return 0.2126 * R + 0.7152 * G + 0.0722 * B;
}

function contrastRatio(
    rgb1: [number, number, number],
    rgb2: [number, number, number]
): number {
    const l1 = relativeLuminance(rgb1);
    const l2 = relativeLuminance(rgb2);

    const lighter = Math.max(l1, l2);
    const darker = Math.min(l1, l2);

    return (lighter + 0.05) / (darker + 0.05);
}

function setFavicon(text: string, fontSize: number, backgroundColor: string, textColor: string): void {
    const svg = createIconSvg(text, fontSize, backgroundColor, textColor);

    const encoded = encodeURIComponent(svg)
        .replace(/'/g, "%27")
        .replace(/"/g, "%22");

    let favicon = document.querySelector<HTMLLinkElement>('link[rel~="icon"]');

    if (favicon === null) {
        favicon = document.createElement("link");
        favicon.rel = "icon";
        document.head.appendChild(favicon);
    }

    favicon.href = `data:image/svg+xml,${encoded}`;
}

export function setIconToLink(link: HTMLAnchorElement, text: string, url: string, fontSize: number, backgroundColor: string, textColor: string): void {
    link.innerHTML = createIconSvg(text, fontSize, backgroundColor, textColor);
    link.setAttribute("aria-label", text);
    link.href = url;
    link.ariaLabel = `Go to ${url}`;
}

export function setIconToSpan(
    span: HTMLSpanElement,
    text: string,
    fontSize: number,
    backgroundColor: string,
    textColor: string
): void {
    span.innerHTML = createIconSvg(
        text,
        fontSize,
        backgroundColor,
        textColor
    );

    span.setAttribute("aria-label", text);
    span.setAttribute("role", "img");
}

export function addIconToSpan(
    span: HTMLSpanElement,
    text: string,
    fontSize: number,
    backgroundColor: string,
    textColor: string
): void {
    var subSpan = document.createElement('span');
    span.appendChild(subSpan);
    setIconToSpan(subSpan, text, fontSize, backgroundColor, textColor);
}
