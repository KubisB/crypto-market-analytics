# Słownik pojęć i teksty interfejsu

Autor: @mmichalskii1

Dokument pomocniczy dla zespołu deweloperskiego i QA. Zawiera uzgodnione angielskie nazewnictwo do kodu i API oraz gotowe teksty po polsku do interfejsu użytkownika.

## 1. Słownik pojęć finansowych, PL na EN

Ujednolica nazwy klas, pól JSON, endpointów i zmiennych w backendzie C# oraz frontendzie TypeScript.

| Pojęcie po polsku | Nazwa angielska w kodzie i API | Uwagi |
|---|---|---|
| Para walutowa | `TradingPair` lub `Symbol` | np. `BNBUSDT`, `BTCUSDT` |
| Waluta bazowa i kwotowania | `BaseAsset`, `QuoteAsset` | dla `BNBUSDT` to `BNB` i `USDT` |
| Świeca dzienna OHLCV | `DailyCandle` | `Open`, `High`, `Low`, `Close`, dzień UTC |
| Wolumen w walucie kwotowania | `QuoteVolume` | pole o indeksie `7` z API Binance, w USDT |
| Wolumen względny | `RelativeVolume` | logarytm stosunku obrotu dnia do średniej z 30 poprzednich dni |
| Zwrot prosty | `SimpleReturn` | $r_t = P_t / P_{t-1} - 1$ |
| Zwrot logarytmiczny | `LogReturn` | $R_t = \ln(P_t / P_{t-1})$ |
| Zwrot za cały okres | `TotalReturn` | $P_N / P_0 - 1$ |
| Skumulowany zwrot | `CumulativeReturn` | wykres BNB na tle BTC od 0% |
| Średni roczny zwrot, CAGR | `Cagr` | liczony przy okresie co najmniej 365 dni |
| Odchylenie standardowe dzienne | `DailyVolatility` lub `LogReturnStdDev` | dzielnik $n - 1$ |
| Zmienność roczna | `AnnualizedVolatility` | $s \cdot \sqrt{365}$ |
| Zmienność krocząca | `RollingVolatility` | okno 30 zwrotów, czyli 31 cen, w skali rocznej |
| Średnia krocząca prosta i wykładnicza | `Sma`, `Ema` | okna `20`, `50`, `200` |
| Obsunięcie i maksymalne obsunięcie | `Drawdown`, `MaxDrawdown` | zawsze $\le 0$ |
| Szczyt, dołek i powrót do szczytu | `PeakDate`, `TroughDate`, `RecoveryDate` | `RecoveryDate` ma wartość `null`, jeśli cena nie wróciła do szczytu |
| Odsetek dni wzrostowych | `PositiveDaysRatio` | bez dni o zerowej zmianie |
| Skośność | `Skewness` | wersja skorygowana $G_1$ |
| Kurtoza nadwyżkowa | `ExcessKurtosis` | wersja skorygowana $G_2$, dla rozkładu normalnego około 0 |
| Histogram i przedziały | `ReturnDistribution`, `HistogramBins` | szerokość według reguły Freedmana-Diaconisa |
| Korelacja, beta i $R^2$ | `Correlation`, `Beta`, `RSquared` | liczone na wspólnych datach zwrotów logarytmicznych |
| Korelacja krocząca | `RollingCorrelation` | 90 kolejnych dni kalendarzowych ze wspólnymi zwrotami |
| Analiza zdarzeń | `EventStudy` | |
| Okno estymacji i okno zdarzenia | `EstimationWindow`, `EventWindow` | od −120 do −11 oraz od −1 do +1 lub od −5 do +5 |
| Zwrot nadzwyczajny i skumulowany | `AbnormalReturn`, `CumulativeAbnormalReturn` | skróty `Ar` i `Car`, wariant kontrolny `CarZeroAlpha` |
| Statystyka $t$ | `TStatistic` | $\lvert t\rvert$ określa dwustronne $p$ z rozkładu t; próg dla $p$ po korekcie Holma wynosi 0,05 |

## 2. Gotowe teksty do interfejsu

### 2.1. Sekcja „Czym jest BNB?”, domyślnie zwinięta

> **Czym jest BNB i co pokazuje ta aplikacja?**
>
> **BNB** to token powiązany z giełdą kryptowalut Binance i natywny token sieci BNB Chain. Służy między innymi do opłacania prowizji na giełdzie ze zniżką oraz opłat transakcyjnych w sieci, a jego podaż jest okresowo zmniejszana przez spalanie.
>
> Zapis **BNB/USDT** oznacza cenę 1 BNB wyrażoną w **USDT**, czyli stablecoinie powiązanym z dolarem amerykańskim, gdzie 1 USDT to około 1 dolar.
>
> Na cenę BNB wpływa zarówno kondycja całego rynku kryptowalut, jak i wydarzenia dotyczące samej giełdy Binance. Dlatego punktem odniesienia w analizie jest **Bitcoin, para BTC/USDT**. Zestawienie obu par pozwala oszacować, na ile ruchy BNB zbiegają się z rynkiem, a na ile od niego odbiegają, choć sam model jest przybliżeniem i nie dowodzi przyczyny zmian.

### 2.2. Uzupełnienie opisów wskaźników

- **Najlepszy i najgorszy dzień:** największy jednodniowy wzrost i spadek ceny zamknięcia w wybranym okresie, liczony w UTC.
- **Odsetek dni wzrostowych:** udział dni zakończonych na plusie wśród wszystkich dni ze zmianą ceny; dni bez zmiany nie są wliczane.
- **Obsunięcie w czasie** to procentowy spadek ceny względem dotychczasowego szczytu w wybranym okresie. Wartość 0% oznacza cenę na poziomie bieżącego maksimum.
- **$R^2$** to udział wariancji zwrotów BNB wyjaśniany przez model z BTC. Wartość mieści się między 0 a 1.
- **Korelacja krocząca 90-dniowa:** pokazuje, czy zależność między BNB a Bitcoinem była stała, czy słabła w wybranych okresach.
- **Wolumen względny:** obrót danego dnia na tle średniego obrotu z poprzednich 30 dni; wartości dodatnie oznaczają dzień o podwyższonej aktywności.

### 2.3. Objaśnienie pod histogramem

> Histogram pokazuje, jak często w wybranym okresie zdarzały się dni o danej sile wzrostu lub spadku, mierzonej dziennym zwrotem logarytmicznym. Słupki po prawej stronie zera to dni wzrostowe, a po lewej spadkowe. Im szersze ogony na brzegach wykresu względem nałożonej krzywej normalnej, tym częściej na rynku pojawiały się gwałtowne ruchy ceny.

### 2.4. Stopka i źródło danych

> Źródło danych: publiczne API Binance, pary `BNBUSDT` i `BTCUSDT`, świece dzienne UTC. Ostatnia aktualizacja: `[DATA UTC]`.
> Projekt ma charakter wyłącznie edukacyjny i nie stanowi porady inwestycyjnej.

## 3. Komunikaty przy małej próbie i brakach

| Sytuacja | Zachowanie UI | Komunikat po polsku |
|---|---|---|
| Brak danych do statystyki wymagającej co najmniej 2 zwrotów | Pusta wartość | `Do tej miary potrzeba co najmniej 2 dziennych zwrotów.` |
| CAGR przy okresie krótszym niż 365 dni | Pusta wartość | `Wymagany okres co najmniej 365 dni kalendarzowych.` |
| Zmienność, korelacja z BTC lub beta przy 2 do 29 odpowiednich zwrotach | Wynik z ostrzeżeniem | `Wynik orientacyjny. Próba zawiera mniej niż 30 zwrotów.` |
| Korelacja wolumenu względnego z wielkością zwrotu przy 2 do 89 wspólnych obserwacjach | Wynik z ostrzeżeniem | `Wynik orientacyjny. Próba zawiera mniej niż 90 wspólnych obserwacji.` |
| Skośność poniżej 3 zwrotów lub kurtoza poniżej 4 zwrotów | Pusta wartość | `Za mało danych do obliczenia miary rozkładu.` |
| Skośność lub kurtoza poniżej 365 zwrotów | Wynik z ostrzeżeniem | `Próba zawiera mniej niż 365 zwrotów. Pojedynczy skrajny dzień może silnie zmienić wynik.` |
| Zmienność krocząca 30-dniowa lub korelacja krocząca 90-dniowa przy za krótkim oknie | Pusta wartość na wykresie | `Brak pełnego okna obliczeniowego.` |
| Maksymalne obsunięcie bez powrotu do szczytu | Informacja w tabeli | `Cena nie wróciła jeszcze do szczytu w wybranym okresie.` |
| Błędny zakres dat, czyli przed 1.01.2023, od większe niż do lub dzisiejsza data | Blokada zapytania i komunikat | `Wybierz zakres od 1.01.2023 do wczoraj, bo dzisiejsza świeca UTC nie jest jeszcze zamknięta.` |
