# Notebooki analityczne

## 1. Przeznaczenie

Katalog zawiera notebooki Jupyter służące do analizy notowań pary BNB/USDT oraz do kontrolnego przeliczania wyników uzyskiwanych w aplikacji. Wzory i obowiązujące warianty obliczeń określa dokument [`metody-statystyczne.md`](../metody-statystyczne.md), a dane testowe zawiera jego rozdział 11.

## 2. Wymagania

- Python w wersji 3.12 lub nowszej.
- Biblioteki wymienione w pliku [`requirements.txt`](requirements.txt). Wersje bibliotek są ustalone dokładnie, co zapewnia identyczne wyniki liczbowe u wszystkich członków zespołu.

## 3. Przygotowanie środowiska

Środowisko wirtualne tworzy się jednorazowo w katalogu głównym repozytorium. Katalog `.venv/` jest wyłączony z repozytorium w pliku `.gitignore`.

System Windows (PowerShell):

```powershell
python -m venv .venv
.venv\Scripts\Activate.ps1
python -m pip install -r docs/analysis/notebooks/requirements.txt
```

Systemy macOS i Linux:

```bash
python3 -m venv .venv
source .venv/bin/activate
python -m pip install -r docs/analysis/notebooks/requirements.txt
```

## 4. Uruchamianie

Po aktywacji środowiska wirtualnego JupyterLab uruchamia się poleceniem:

```bash
jupyter lab docs/analysis/notebooks
```

Notebooki można również otwierać w VS Code. Jako kernel należy wówczas wskazać interpreter ze środowiska `.venv`.

## 5. Zasady pracy

- Nazwa pliku składa się z numeru porządkowego i krótkiego opisu w języku angielskim, zapisanego małymi literami z myślnikami, np. `01-data-download.ipynb` lub `02-descriptive-stats.ipynb`.
- Dane rynkowe są pobierane z publicznego API Binance i nie są umieszczane w repozytorium. Każdy notebook podaje datę ostatniego dnia danych, na których został wykonany.
- Przed zatwierdzeniem zmian notebook należy wykonać w całości od początku (Restart Kernel and Run All), tak aby zapisane wyniki odpowiadały aktualnemu kodowi.
- Wyniki przeznaczone do dokumentacji są weryfikowane przez członka zespołu na danych źródłowych.
- Zmiana wersji bibliotek w pliku `requirements.txt` wymaga osobnego pull requestu, ponieważ może wpłynąć na wyniki liczbowe.
