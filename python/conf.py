import csv
import re
import time
from urllib.parse import urljoin

import requests
from bs4 import BeautifulSoup


START_URL = "https://dblp.org/db/conf/index.html"
OUTPUT_TSV = "dblp_conference_acronyms.tsv"


MAX_INDEX_FETCHES = 1   # 一覧ページの最大取得数。テストなら 1、本番なら None
MAX_DETAIL_FETCHES = 20  # 個別ページの最大取得数。テストなら 20、本番なら None
FETCH_DETAIL_PAGES = True  # True にすると ... を個別ページで補完する
FETCH_INTERVAL_SECONDS = 1.0  # fetch間隔。例: 0.5, 1.0, 2.0


def normalize_space(s: str) -> str:
    return re.sub(r"\s+", " ", s).strip()


def parse_entry(text: str):
    text = normalize_space(text)

    # 形式1: ACRONYM - Full Name
    m = re.match(r"^(.+?)\s+-\s+(.+)$", text)
    if m:
        acronym = normalize_space(m.group(1))
        full_name = normalize_space(m.group(2))
        return acronym, full_name

    # 形式2: Full Name (ACRONYM)
    m = re.match(r"^(.+?)\s+\(([^()]+)\)$", text)
    if m:
        full_name = normalize_space(m.group(1))
        acronym = normalize_space(m.group(2))
        return acronym, full_name

    return None


def parse_heading(text: str):
    """
    個別ページの見出しから acronym と full_name を抽出する。

    例:
      IEEE International Conference on 3D System Integration (3DIC)
        -> 3DIC, IEEE International Conference on 3D System Integration
    """
    text = normalize_space(text)

    m = re.match(r"^(.+?)\s+\(([^()]+)\)$", text)
    if not m:
        return None

    full_name = normalize_space(m.group(1))
    acronym = normalize_space(m.group(2))

    return acronym, full_name


def is_conference_link(abs_url: str) -> bool:
    if not abs_url:
        return False

    if "/db/conf/" not in abs_url:
        return False

    if abs_url.endswith("/db/conf/") or abs_url.endswith("/db/conf/index.html"):
        return False

    return True


def get_next_page_url(soup: BeautifulSoup, current_url: str):
    for a in soup.find_all("a"):
        if normalize_space(a.get_text()) == "[next 100 entries]":
            return urljoin(current_url, a.get("href"))

    return None


def fetch_detail_name(session: requests.Session, dblp_url: str):
    """
    DBLP の個別 conference ページを取得し、h1 から正式名を取る。
    失敗したら None を返す。
    """
    res = session.get(dblp_url, timeout=30)
    res.raise_for_status()

    soup = BeautifulSoup(res.text, "html.parser")

    h1 = soup.find("h1")
    if h1 is None:
        return None

    heading = normalize_space(h1.get_text(" "))
    return parse_heading(heading)


def main():
    session = requests.Session()
    session.headers.update({
        "User-Agent": "conference-acronym-extractor/1.0"
    })

    rows_by_url = {}
    index_fetch_count = 0

    url = START_URL

    while url:
        if MAX_INDEX_FETCHES is not None and index_fetch_count >= MAX_INDEX_FETCHES:
            print(f"stop: reached MAX_INDEX_FETCHES = {MAX_INDEX_FETCHES}")
            break

        print(f"fetch index: {url}")
        index_fetch_count += 1

        res = session.get(url, timeout=30)
        res.raise_for_status()

        soup = BeautifulSoup(res.text, "html.parser")

        for a in soup.find_all("a"):
            href = a.get("href")
            text = normalize_space(a.get_text())
            abs_url = urljoin(url, href or "")

            if not is_conference_link(abs_url):
                continue

            parsed = parse_entry(text)
            if parsed is None:
                continue

            acronym, full_name = parsed

            # 同じ dblp_url について、... でない名前を優先する
            old = rows_by_url.get(abs_url)
            new_row = {
                "acronym": acronym,
                "full_name": full_name,
                "dblp_url": abs_url,
                "source_text": text,
                "name_source": "index",
            }

            if old is None:
                rows_by_url[abs_url] = new_row
            else:
                old_has_dots = "..." in old["full_name"]
                new_has_dots = "..." in full_name

                if old_has_dots and not new_has_dots:
                    rows_by_url[abs_url] = new_row

        url = get_next_page_url(soup, url)
        time.sleep(FETCH_INTERVAL_SECONDS)

    if FETCH_DETAIL_PAGES:
        detail_fetch_count = 0

        for dblp_url, row in list(rows_by_url.items()):
            # ... を含む行だけ補完する
            if "..." not in row["full_name"] and "..." not in row["source_text"]:
                continue

            if MAX_DETAIL_FETCHES is not None and detail_fetch_count >= MAX_DETAIL_FETCHES:
                print(f"stop: reached MAX_DETAIL_FETCHES = {MAX_DETAIL_FETCHES}")
                break

            print(f"fetch detail: {dblp_url}")
            detail_fetch_count += 1

            try:
                parsed = fetch_detail_name(session, dblp_url)
            except requests.RequestException as e:
                print(f"warning: failed to fetch {dblp_url}: {e}")
                continue

            if parsed is None:
                continue

            detail_acronym, detail_full_name = parsed

            # 一覧ページの acronym と個別ページの acronym が一致する場合だけ安全に置換
            if detail_acronym == row["acronym"]:
                row["full_name"] = detail_full_name
                row["source_text"] = f"{detail_full_name} ({detail_acronym})"
                row["name_source"] = "detail_h1"

            time.sleep(FETCH_INTERVAL_SECONDS)

    rows = list(rows_by_url.values())
    rows.sort(key=lambda r: (r["acronym"].lower(), r["full_name"].lower()))

    with open(OUTPUT_TSV, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(
            f,
            fieldnames=[
                "acronym",
                "full_name",
                "dblp_url",
                "source_text",
                "name_source",
            ],
            delimiter="\t",
        )
        writer.writeheader()
        writer.writerows(rows)

    print(f"written: {OUTPUT_TSV}")
    print(f"entries: {len(rows)}")
    print(f"index fetches: {index_fetch_count}")


if __name__ == "__main__":
    main()