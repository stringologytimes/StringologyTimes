import csv
import re
import time
from urllib.parse import urljoin

import requests
from bs4 import BeautifulSoup


START_URL = "https://dblp.org/db/conf/index.html"

OUTPUT_TSV = "dblp_conference_acronyms.tsv"
OMITTED_OUTPUT_TSV = "omitted_dblp_conference_acronyms.tsv"

MAX_FETCHES = 5  # None にすると制限なし


def normalize_space(s: str) -> str:
    return re.sub(r"\s+", " ", s).strip()


def parse_entry(text: str):
    """
    DBLP のリンクテキストから acronym と full_name を抽出する。

    対応する主な形式:
      1. ACRONYM - Full Name
         例: DCC - Data Compression Conference

      2. Full Name (ACRONYM)
         例: Data Compression Conference (DCC)

    戻り値:
      (acronym, full_name) または None
    """
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


def is_conference_link(href: str) -> bool:
    """
    /db/conf/ 以下の会議ページへのリンクだけを対象にする。
    index.html, アルファベットジャンプ, previous/next などは除外する。
    """
    if not href:
        return False

    if href.startswith("#"):
        return False

    return "/db/conf/" in href and not href.endswith("/db/conf/")


def get_next_page_url(soup: BeautifulSoup, current_url: str):
    """
    DBLP の一覧は 100 件ごとにページ送りされる。
    [next 100 entries] へのリンクがあれば次ページとして返す。
    """
    for a in soup.find_all("a"):
        if normalize_space(a.get_text()) == "[next 100 entries]":
            return urljoin(current_url, a.get("href"))

    return None


def is_omitted(row: dict) -> bool:
    """
    full_name または source_text に ... を含むものを省略データとみなす。
    """
    return "..." in row["full_name"] or "..." in row["source_text"]


def write_tsv(path: str, rows: list[dict]):
    with open(path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(
            f,
            fieldnames=["acronym", "full_name", "dblp_url", "source_text"],
            delimiter="\t",
        )
        writer.writeheader()
        writer.writerows(rows)


def main():
    session = requests.Session()
    session.headers.update({
        "User-Agent": "conference-acronym-extractor/1.0"
    })

    rows = []
    seen = set()

    url = START_URL
    fetch_count = 0

    while url:
        if MAX_FETCHES is not None and fetch_count >= MAX_FETCHES:
            print(f"stop: reached MAX_FETCHES = {MAX_FETCHES}")
            break

        print(f"fetch: {url}")
        fetch_count += 1

        res = session.get(url, timeout=30)
        res.raise_for_status()

        soup = BeautifulSoup(res.text, "html.parser")

        for a in soup.find_all("a"):
            href = a.get("href")
            text = normalize_space(a.get_text())

            if not is_conference_link(urljoin(url, href or "")):
                continue

            parsed = parse_entry(text)
            if parsed is None:
                continue

            acronym, full_name = parsed
            dblp_url = urljoin(url, href)

            key = (acronym, full_name, dblp_url)
            if key in seen:
                continue

            seen.add(key)
            rows.append({
                "acronym": acronym,
                "full_name": full_name,
                "dblp_url": dblp_url,
                "source_text": text,
            })

        url = get_next_page_url(soup, url)

        time.sleep(1)

    rows.sort(key=lambda r: (r["acronym"].lower(), r["full_name"].lower()))

    normal_rows = []
    omitted_rows = []

    for row in rows:
        if is_omitted(row):
            omitted_rows.append(row)
        else:
            normal_rows.append(row)

    write_tsv(OUTPUT_TSV, normal_rows)
    write_tsv(OMITTED_OUTPUT_TSV, omitted_rows)

    print(f"written: {OUTPUT_TSV}")
    print(f"entries: {len(normal_rows)}")

    print(f"written: {OMITTED_OUTPUT_TSV}")
    print(f"omitted entries: {len(omitted_rows)}")

    print(f"fetches: {fetch_count}")


if __name__ == "__main__":
    main()