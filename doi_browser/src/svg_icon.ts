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
    const safeText = escapeSvgText(text);
    const safeBackgroundColor = escapeSvgAttribute(backgroundColor);
    const safeTextColor = escapeSvgAttribute(textColor);

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

/*
export function createClickableIcon(text: string, url: string): HTMLAnchorElement {
    const link = document.createElement("a");
    setFavicon(text);

    link.href = url;
    link.innerHTML = createIconSvg(text);
    link.ariaLabel = `Go to ${url}`;

    return link;
}
*/
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
