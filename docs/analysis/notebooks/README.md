# Notebooki analityczne

## 1. Przeznaczenie

Katalog zawiera notebooki Jupyter do analizy BNB/USDT i weryfikacji wyników aplikacji. Wzory określa [dokument metod](../metody-statystyczne.md). Przykłady testowe znajdują się w jego rozdziale 11 oraz w [rozszerzonych danych testowych](../dane-testowe-rozszerzone.md).

- [01](01-data-download.ipynb) pobiera i sprawdza zamknięte świece UTC.
- [02](02-descriptive-stats.ipynb) oblicza statystyki opisowe.
- [03](03-rolling-and-distribution.ipynb) oblicza wskaźniki kroczące i rysuje histogram.
- [04](04-bnb-vs-btc.ipynb) porównuje BNB z BTC.
- [05](05-event-study-and-volume.ipynb) analizuje zdarzenia i wolumen.

Wspólny kod pobierania i wyznaczania dziennych relacji cen znajduje się w [module market_data](market_data.py). Świece pozostają w pamięci, a wyniki notebooków zawierają podsumowania i wykresy.

## 2. Wymagania

- Python w wersji 3.12 lub nowszej.
- Biblioteki z wersjami określonymi w [requirements.txt](requirements.txt).

## 3. Przygotowanie środowiska

Środowisko wirtualne tworzy się jednorazowo w katalogu głównym repozytorium. W repozytorium katalog `.venv/` jest wyłączony przez `.gitignore`.

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
