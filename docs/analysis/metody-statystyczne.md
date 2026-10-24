# Metody analizy statystycznej BNB/USDT

Autor: @mmichalskii1

## 1. Cel dokumentu

Dokument opisuje, co jest liczone i jakimi wzorami. Niektóre statystyki mają kilka wariantów, np. odchylenie standardowe z dzielnikiem n albo n−1, a biblioteki liczą je domyślnie różnie, dlatego dla każdej metody wskazano jeden obowiązujący wariant.

## 2. Dane i oznaczenia

Analiza opiera się na cenach zamknięcia dziennych świec z Binance: `BNBUSDT` oraz porównawczo `BTCUSDT`. Dzień liczony jest w UTC, którego definicja jest w dokumencie wymagań. Uwzględniane są tylko zamknięte świece.

- $P_t$ to cena zamknięcia w dniu $t$,
- $r_t$ to zwrot prosty, $R_t$ to zwrot logarytmiczny,
- $n$ to liczba zwrotów, o jeden mniejsza niż liczba cen,
- $M_t$ to zwrot logarytmiczny BTC, czyli rynku.

Gdy brakuje dnia, zwrot liczony jest między sąsiednimi dostępnymi dniami, a luka jest odnotowana, bez interpolacji. Skala roczna używa 365 dni, bo rynek krypto działa codziennie.

### 2.1. Okres analizy

Analiza obejmuje okres od 1 stycznia 2023 do wczoraj, czyli do ostatniej zamkniętej świecy. W październiku 2026 to około 1390 dni, a codziennie przybywa jeden. Początek w 2023 roku wybrano, bo dane są świeże i jest ich dość na stabilne statystyki, rok 2022 z upadkiem Terry i FTX zawyżyłby zmienność, a w okresie mieszczą się najważniejsze wydarzenia wokół Binance.

Statystyki opisowe, rozkład zwrotów, korelacja, beta i obsunięcie liczone są na całym wybranym okresie, a wskaźniki kroczące mają własne okna. Potrzebują one danych sprzed okresu, np. 200 dni dla SMA200, które nie wchodzą do pozostałych statystyk. Przy krótszym okresie statystyki wymagające więcej obserwacji pokazują ostrzeżenie. Wyniki w dokumentacji są opisywane z datą ostatniego dnia danych i odświeżane przed oddaniem projektu.

## 3. Zwroty

$$
r_t = \frac{P_t}{P_{t-1}} - 1 \qquad\qquad R_t = \ln\frac{P_t}{P_{t-1}}
$$

Zwrot prosty jest intuicyjny, a logarytmiczne sumują się w czasie, $\sum R_t = \ln(P_N/P_0)$, więc większość statystyk liczona jest na nich. Podział:
- zwroty proste: średnia, mediana, najgorszy i najlepszy dzień,
- zwroty logarytmiczne: zmienność, skośność, kurtoza, histogram, korelacja, beta, analiza wydarzeń,
- zwrot za cały okres liczony z cen jako $P_N/P_0 - 1$.

## 4. Statystyki opisowe

Na zwrotach prostych:
- liczba dni $n$, pokazywana zawsze, bo od niej zależy wiarygodność reszty,
- średni dzienny zwrot i mediana, mniej wrażliwa na skrajne dni,
- najgorszy i najlepszy dzień z datą,
- odsetek dni wzrostowych, bez dni o zerowej zmianie,
- zwrot za cały okres, $P_N/P_0 - 1$,
- średni roczny zwrot CAGR, $(P_N/P_0)^{365/d} - 1$, gdzie $d$ to liczba dni kalendarzowych między datą pierwszej i ostatniej ceny. Liczony tylko wtedy, gdy $d \ge 365$.

Przy mniej niż 2 zwrotach statystyki nie są liczone.

## 5. Zmienność

### 5.1. Zmienność dla całego okresu

Odchylenie standardowe dziennych zwrotów logarytmicznych z dzielnikiem n−1, jak `STDEV.S` w Excelu i `.std()` w pandas:

$$
s = \sqrt{\frac{1}{n-1}\sum_{t=1}^{n}\left(R_t - \bar{R}\right)^2}
\qquad\qquad
\sigma_{\text{rok}} = s \cdot \sqrt{365}
$$

Dzienne odchylenie $s$ jest podawane w tabeli statystyk obok zmienności rocznej. Dla porównania indeksy akcji mają zmienność roczną 15 do 25%, kryptowaluty często kilkadziesiąt procent.

### 5.2. Zmienność krocząca

To samo w oknie 30 dni. Pierwsze 29 dni nie ma wartości. Skoki zmienności warto zestawić z wydarzeniami z rozdziału 9.

## 6. Średnie kroczące

Na cenach zamknięcia, dla okien 20, 50 i 200 dni:

$$
\text{SMA}_t = \frac{1}{w}\sum_{i=0}^{w-1} P_{t-i}
\qquad\qquad
\text{EMA}_t = \alpha P_t + (1-\alpha)\,\text{EMA}_{t-1},\quad \alpha = \frac{2}{w+1}
$$

Pierwsza wartość EMA to SMA z pierwszych $w$ dni. Średnie pokazują trend i nie służą do sygnałów kupna ani sprzedaży.

## 7. Obsunięcie

$$
DD_t = \frac{P_t}{\max_{s \le t} P_s} - 1
\qquad\qquad
\text{MDD} = \min_t DD_t
$$

Dla maksymalnego obsunięcia podawane są data szczytu, data dołka, liczba dni spadku oraz informacja, czy i kiedy cena wróciła do szczytu. Szczyt sprzed okresu nie jest uwzględniany, więc wynik zależy od daty początkowej, a liczenie na cenach zamknięcia pomija spadki w ciągu dnia.

## 8. Rozkład zwrotów

### 8.1. Skośność i kurtoza

Wersje skorygowane, zgodne z `SKEW` i `KURT` w Excelu oraz `.skew()` i `.kurt()` w pandas:

$$
G_1 = \frac{n}{(n-1)(n-2)} \sum \left(\frac{R_t - \bar{R}}{s}\right)^3
$$

$$
G_2 = \frac{n(n+1)}{(n-1)(n-2)(n-3)} \sum \left(\frac{R_t - \bar{R}}{s}\right)^4 - \frac{3(n-1)^2}{(n-2)(n-3)}
$$

Kurtoza jest nadwyżkowa, czyli dla rozkładu normalnego, w którym wartości skrajne są bardzo rzadkie, wynosi około 0. Wartość dodatnia oznacza grube ogony, czyli dni skrajne częstsze niż w rozkładzie normalnym, co na rynku krypto jest typowe. Ujemna skośność oznacza przewagę dużych spadków nad dużymi wzrostami. Uwaga na biblioteki: `scipy.stats.kurtosis` domyślnie liczy wersję nieskorygowaną.

### 8.2. Histogram

Histogram zwrotów logarytmicznych, w którym wysokość słupka to liczba dni z danego przedziału. Szerokość przedziałów wyznacza reguła Freedmana-Diaconisa, $h = 2 \cdot \text{IQR} \cdot n^{-1/3}$, przy liczbie przedziałów od 20 do 100. IQR to różnica między trzecim a pierwszym kwartylem, liczonymi z interpolacją liniową, jak `QUARTILE.INC` w Excelu i `.quantile()` w pandas.

Prawa strona od zera to dni wzrostowe, lewa spadkowe. Szeroki wykres z wyraźnymi słupkami na brzegach oznacza duże wahania i dodatnią kurtozę.

## 9. BNB na tle rynku

### 9.1. Korelacja i beta

Na zwrotach logarytmicznych BNB i BTC, tylko dla wspólnych dni:

$$
\rho = \frac{\sum (R_t - \bar{R})(M_t - \bar{M})}{\sqrt{\sum (R_t - \bar{R})^2 \sum (M_t - \bar{M})^2}}
\qquad\qquad
\beta = \frac{\sum (R_t - \bar{R})(M_t - \bar{M})}{\sum (M_t - \bar{M})^2}
$$

Beta to nachylenie regresji $R_t = \alpha + \beta M_t$, w Excelu `SLOPE`. Liczony jest też $R^2 = \rho^2$. Interpretacja:
- korelacja poniżej 0,3 to związek słaby, 0,3 do 0,7 umiarkowany, powyżej 0,7 silny, a wartość bliska 0 oznacza brak związku liniowego,
- beta około 1 oznacza podobne ruchy BNB i BTC, powyżej 1 silniejszą reakcję BNB, poniżej 1 słabszą,
- $R^2$ to część ruchów BNB wyjaśniona przez rynek, a $1 - R^2$ to ruch specyficzny dla BNB.

### 9.2. Korelacja krocząca

Korelacja w oknie 90 dni, pokazująca, czy związek BNB z rynkiem jest stały, czy słabnie w niektórych okresach.

### 9.3. Wydarzenia wokół Binance

Analiza zdarzeń sprawdza, czy przy ważnych wydarzeniach cena BNB zachowywała się inaczej, niż wynikałoby z ruchu rynku. Większość wydarzeń dotyczy Changpenga Zhao, założyciela Binance i jej szefa do 2023 roku. SEC to amerykański urząd nadzorujący rynek papierów wartościowych. Dniem wydarzenia jest dzień UTC, w którym informacja stała się publiczna. Wydarzenia:
- 5.06.2023: SEC pozywa Binance i Changpenga Zhao,
- 21.11.2023: ugoda Binance z władzami USA, Changpeng Zhao odchodzi ze stanowiska szefa Binance,
- 30.04.2024: Changpeng Zhao zostaje skazany na 4 miesiące więzienia,
- 27.09.2024: Changpeng Zhao wychodzi na wolność,
- 23.10.2025: prezydent USA ułaskawia Changpenga Zhao.

Procedura:
1. Na dniach od −120 do −11 przed wydarzeniem szacowana jest regresja $R_t = \alpha + \beta M_t$. Przerwa przed wydarzeniem ogranicza wpływ przecieków informacji.
2. W oknie wydarzenia, czyli dzień przed, w dniu wydarzenia i dzień po, liczony jest zwrot nadzwyczajny, czyli część ruchu niewyjaśniona przez BTC: $AR_t = R_t - (\hat{\alpha} + \hat{\beta} M_t)$. Opcjonalnie liczone jest też okno od −5 do +5 dni, które wymaga 5 dni danych po wydarzeniu.
3. Zwroty nadzwyczajne są sumowane: $CAR = \sum AR_t$.
4. Istotność sprawdza $t = CAR / (s_\varepsilon \sqrt{T})$, gdzie $s_\varepsilon$ to odchylenie reszt z kroku 1 z dzielnikiem $L-2$, a $T$ to długość okna wydarzenia. Wartość $|t| > 1{,}96$ oznacza reakcję istotną na poziomie 5%.

Wyraźnie ujemny CAR oznacza, że BNB spadł mocniej, niż wynikałoby z ruchu BTC, czyli reakcję specyficzną dla Binance. Ograniczenia: to pojedyncze wydarzenia, więc wnioski są opisowe, a wydarzenie wpływające na cały rynek, jak ułaskawienie Changpenga Zhao, jest częściowo pochłaniane przez BTC.

## 10. Wolumen

Używany jest wolumen w USDT, czyli pole o indeksie 7 świecy z API, liczonym od 0. Badana jest korelacja między wielkością ruchu ceny $|R_t|$ a logarytmem wolumenu, który ogranicza wpływ dni z ogromnym obrotem. Oczekiwany jest związek dodatni.

## 11. Dane testowe

Przykłady na wymyślonych cenach, do sprawdzenia, czy wzory z tego dokumentu są zaimplementowane poprawnie. Wyniki są policzone w Pythonie, więc po wpisaniu tych samych liczb implementacja powinna dać te same wartości.

### 11.1. Pięć cen

Ceny: 100, 110, 99, 108,9, 102,366. Zwroty proste: 0,10; −0,10; 0,10; −0,06. Oczekiwane wyniki, każdy na zwrotach tego rodzaju, na jakich liczy go dokument:
- zwroty logarytmiczne: 0,0953101798; −0,1053605157; 0,0953101798; −0,0618754037, o sumie 0,0233844402,
- średnia zwrotów prostych: 0,01, a mediana zwrotów prostych: 0,02,
- odchylenie standardowe zwrotów logarytmicznych z dzielnikiem n−1: 0,1048185044, a razy √365: 2,0025547144,
- zwrot za okres: 0,02366,
- maksymalne obsunięcie: −0,10, szczyt 110, dołek 99, bez powrotu do szczytu,
- SMA z 3 dni na końcu: 103,422,
- skośność $G_1$ zwrotów logarytmicznych: −0,1468980258, kurtoza $G_2$: −5,1579652449. Przy 4 obserwacjach nie mają znaczenia statystycznego, służą tylko do sprawdzenia wzoru.

### 11.2. Korelacja i beta

Ten test sprawdza sam wzór na dwóch dowolnych szeregach, wpisanych wprost, a nie wyliczonych z cen. Szereg BNB: 0,10; −0,10; 0,10; −0,06. Szereg BTC: 0,02; −0,01; 0,03; −0,02. Oczekiwane wyniki: korelacja 0,9317610191, beta 4,1176470588.

## 12. Weryfikacja wyników

Analiza jest wykonywana w Pythonie, w notebookach Jupyter z pandas, numpy, scipy i matplotlib, zapisanych w `docs/analysis/notebooks/`. Zgodnie z README wyniki trafiające do dokumentacji są przed publikacją sprawdzane przez człowieka na danych źródłowych.
