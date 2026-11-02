# Notebooki analizy

Notebooki Jupyter do analizy BNB/USDT i do kontrolnego przeliczania wyników aplikacji. Wzory i obowiązujące warianty obliczeń są w [`../metody-statystyczne.md`](../metody-statystyczne.md), a dane testowe w jego rozdziale 11.

## Wymagania

- Python 3.12 lub nowszy,
- biblioteki z [`requirements.txt`](requirements.txt), w dokładnie tych wersjach, żeby wszyscy dostawali te same wyniki liczbowe.

## Przygotowanie środowiska

Jednorazowo, z katalogu głównego repozytorium. Środowisko `.venv/` jest ignorowane przez git.

Windows (PowerShell):

```powershell
python -m venv .venv
.venv\Scripts\Activate.ps1
python -m pip install -r docs/analysis/notebooks/requirements.txt
```

macOS i Linux:

```bash
python3 -m venv .venv
source .venv/bin/activate
python -m pip install -r docs/analysis/notebooks/requirements.txt
```

## Uruchomienie

Po aktywowaniu środowiska:

```bash
jupyter lab docs/analysis/notebooks
```

W VS Code wystarczy otworzyć plik `.ipynb` i wybrać jako kernel interpreter z `.venv`.

## Zasady

- Nazwy plików: numer i krótki opis po angielsku, małymi literami z myślnikami, np. `01-data-download.ipynb`, `02-descriptive-stats.ipynb`.
- Dane pobierane są z publicznego API Binance i nie są commitowane. W notebooku zapisywana jest data ostatniego dnia danych.
- Przed commitem uruchom notebook od początku do końca (Restart Kernel and Run All), żeby wyniki odpowiadały kodowi.
- Wyniki trafiające do dokumentacji sprawdza człowiek na danych źródłowych.
- Zmiana wersji bibliotek w `requirements.txt` to osobny PR, bo może zmienić wyniki liczbowe.
