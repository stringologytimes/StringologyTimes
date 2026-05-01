export function load_gzip_text(url: string): Promise<string> {
    return fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Failed to fetch file: ${response.statusText}`);
            }
            return response.arrayBuffer();
        })
        .then(buffer => {
            // @ts-ignore
            const pako = (globalThis as any).pako || (window as any).pako; // Assume pako is available in global scope
            if (!pako) {
                throw new Error('pako library not loaded. Please include pako in your HTML.');
            }
            const compressed = new Uint8Array(buffer);
            const decompressed = pako.ungzip(compressed);
            // Decode to UTF-8 text
            const decoder = new TextDecoder('utf-8');
            return decoder.decode(decompressed);
        });
}
export async function load_gzip_text_lines(url: string): Promise<string[]> {
    return load_gzip_text(url).then(text => text.split('\n'));

    /*
    if (remove_empty) {
        return load_gzip_text(url).then(text => text.split('\n').filter(line => line.trim()));
    } else {
    }
    */
}
export async function load_gzip_integer_lines(url: string): Promise<number[]> {
    return load_gzip_text(url).then(text => text.split('\n').map(line => parseInt(line)));
}

export async function load_gzip_integer_list_lines(url: string): Promise<number[][]> {
    const lines = await load_gzip_text(url).then(text => text.split('\n'));
    const r: number[][] = [];
    lines.forEach(line => {
        const parts = line.split(',');
        const row: number[] = [];
        parts.forEach(part => {
            const number = parseInt(part);
            if(!Number.isNaN(number)){
                row.push(number);
            }
        });
        r.push(row);
    });
    return r;
}