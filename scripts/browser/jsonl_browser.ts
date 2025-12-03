
export function hello() {
    window.addEventListener('DOMContentLoaded', () => {
        fetch('./jsonl/stringology_dblp.jsonl')
            .then(response => {
                if (!response.ok) {
                    throw new Error('HTTPエラー: ' + response.status);
                }
                return response.text();  // ★ json ではなく text として取得
            })
            .then(text => {
                const output = document.getElementById('output');
                const errorDiv = document.getElementById('error');

                const lines = text.split(/\r?\n/).filter(line => line.trim() !== '');

                if (lines.length === 0) {
                    if(output != null){
                        output.textContent = 'ファイルに有効な行がありません。';
                    }
                    return;
                }

                lines.forEach((line, index) => {
                    const div = document.createElement('div');
                    div.className = 'item';

                    try {
                        const obj = JSON.parse(line);

                        // 1行が文字列そのものの場合（"..."）
                        if (typeof obj === 'string') {
                            div.textContent = obj;
                        }
                        // 1行が { "text": "..." } のようなオブジェクトの場合
                        else if (obj && typeof obj === 'object' && 'text' in obj) {
                            div.textContent = String(obj.text);
                        }
                        // それ以外の形式は整形したJSONを表示
                        else {
                            div.textContent = JSON.stringify(obj, null, 2);
                        }
                    } catch (e) {
                        // パースできなかった行はそのまま表示＋エラー情報
                        div.textContent = `行 ${index + 1} をJSONとして解釈できません:\n${line}\n\n${e}`;
                    }

                    if(output != null){
                        output.appendChild(div);
                    }
                });
            })
            .catch(err => {
                const errorDiv = document.getElementById('error');
                if(errorDiv != null){
                    errorDiv.textContent =
                        '読み込みエラー: ' + err.message +
                        '\n（file:/// で開いている場合はローカルHTTPサーバ経由で開いてください）';
                }
            });
    });
}