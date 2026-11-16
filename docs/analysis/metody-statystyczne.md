# Metody analizy statystycznej BNB/USDT

Autor: @mmichalskii1

## 1. Cel dokumentu

Dokument opisuje, co jest liczone i jakimi wzorami. Niektóre statystyki mają kilka wariantów, np. odchylenie standardowe z dzielnikiem n albo n−1, a biblioteki liczą je domyślnie różnie, dlatego dla każdej metody wskazano jeden obowiązujący wariant.

## 2. Dane i oznaczenia

Analiza opiera się na cenach zamknięcia dziennych świec z Binance: `BNBUSDT` oraz porównawczo `BTCUSDT`. Dzień liczony jest w UTC, którego definicja jest w dokumencie wymagań. Uwzględniane są tylko zamknięte świece.

- $P_t$ to cena zamknięcia w dniu $t$,
- $r_t$ to zwrot prosty, $R_t$ to zwrot logarytmiczny,
- $n$ to liczba poprawnych dziennych zwrotów, o jeden mniejsza niż liczba cen przy pełnym szeregu,
- $M_t$ to zwrot logarytmiczny BTC, używany jako punkt odniesienia dla rynku.

Gdy brakuje dnia, luka jest odnotowana bez interpolacji. Zwrot między sąsiednimi dostępnymi dniami obejmuje wtedy więcej niż jedną dobę i jest wyłączony ze statystyk dziennych. Zwrot za cały okres i CAGR wykorzystują pierwszą i ostatnią dostępną cenę oraz, w przypadku CAGR, odstęp w dniach kalendarzowych. Skala roczna używa 365 dni, bo rynek krypto działa codziennie.

### 2.1. Okres analizy

Analiza obejmuje okres od 1 stycznia 2023 do ostatniego zamkniętego dnia UTC. Obejmuje rynek po upadku Terry i FTX w 2022 roku oraz późniejsze wydarzenia wokół Binance. Początek 2023 roku przypada w pobliżu dołka cyklu, gdy BNB kosztował około 245 USDT, a BTC około 16,5 tys. USDT. Przy interpretacji zwrotu za okres i CAGR należy uwzględnić ich zależność od punktu startowego.

Statystyki opisowe, rozkład zwrotów, korelacja i beta dla całego okresu oraz obsunięcie wykorzystują dane z wybranego okresu. Pierwsza cena okresu nie ma dziennego zwrotu. Wcześniejsze świece służą wyłącznie do obliczania wskaźników z oknami kroczącymi, wartości wolumenu względnego oraz estymacji modelu dla zdarzeń. Analiza zdarzeń używa własnych okien niezależnie od wybranego okresu. Wyniki w dokumentacji podają datę ostatniego dnia danych.

## 3. Zwroty

$$
r_t = \frac{P_t}{P_{t-1}} - 1 \qquad\qquad R_t = \ln\frac{P_t}{P_{t-1}}
$$

Zwrot prosty podaje względną zmianę ceny. Dla ciągłego szeregu sumę zwrotów logarytmicznych można zapisać jako $\sum R_t = \ln(P_N/P_0)$. Przy lukach suma ważnych zwrotów dziennych obejmuje tylko odcinki z dostępnymi danymi. Projekt stosuje następujący podział:
- zwroty proste: średnia, mediana, najgorszy i najlepszy dzień,
- zwroty logarytmiczne: zmienność, skośność, kurtoza, histogram, korelacja, beta, analiza wydarzeń,
- zwrot za cały okres liczony z cen jako $P_N/P_0 - 1$.

## 4. Statystyki opisowe

Na zwrotach prostych:
- liczba dni $n$, pokazywana zawsze, bo od niej zależy wiarygodność reszty,
- średni dzienny zwrot i mediana, mniej wrażliwa na skrajne dni,
- najgorszy i najlepszy dzień z datą,
- odsetek dni wzrostowych wśród dni o niezerowym zwrocie. Gdy wszystkie zwroty są zerowe, odsetek jest nieokreślony,
- zwrot za cały okres, $P_N/P_0 - 1$,
- średni roczny zwrot CAGR, $(P_N/P_0)^{365/d} - 1$, gdzie $d$ to liczba dni kalendarzowych między datą pierwszej i ostatniej ceny. Liczony tylko wtedy, gdy $d \ge 365$.

Przy mniej niż 2 zwrotach statystyki nie są liczone.

Średni dzienny zwrot nie mówi, czy cena w całym okresie wzrosła. Przy dużej zmienności średnia arytmetyczna zwrotów prostych jest wyższa niż średnie tempo wzrostu, np. wzrost o 50% i spadek o 50% dają średnią 0, a cenę niższą o 25%. O wyniku okresu mówi zwrot za cały okres i CAGR.

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

W każdym dniu z pełnym oknem 30 kolejnych zwrotów, czyli 31 kolejnych cen zamknięcia, liczymy zmienność w skali rocznej $s \cdot \sqrt{365}$. Pierwsza wartość w ciągłym szeregu pojawia się przy 31. cenie. Gdy okno zawiera lukę, pomijamy wartość i przerywamy linię wykresu.

## 6. Średnie kroczące

Na cenach zamknięcia, dla okien 20, 50 i 200 dni:

$$
\text{SMA}_t = \frac{1}{w}\sum_{i=0}^{w-1} P_{t-i}
\qquad\qquad
\text{EMA}_t = \alpha P_t + (1-\alpha)\,\text{EMA}_{t-1},\quad \alpha = \frac{2}{w+1}
$$

SMA wymaga $w$ cen z kolejnych dni kalendarzowych. Pierwsza wartość EMA w każdym ciągłym odcinku to SMA z pierwszych $w$ dni tego odcinka. Po luce EMA jest pusta do chwili zebrania nowego pełnego okna. Średnie służą do opisu trendu cenowego.

## 7. Obsunięcie

$$
DD_t = \frac{P_t}{\max_{s \le t} P_s} - 1
\qquad\qquad
\text{MDD} = \min_t DD_t
$$

Dla maksymalnego obsunięcia podajemy datę szczytu, datę dołka, odstęp w dniach kalendarzowych i pierwszą późniejszą datę zamknięcia na poziomie szczytu lub powyżej. Przy remisie wybieramy pierwszy dołek i ostatni równy szczyt przed nim. Przy obsunięciu równym zero daty i odstęp pozostają puste. Szczyt wyznaczamy w wybranym okresie na podstawie cen zamknięcia.

## 8. Rozkład zwrotów

### 8.1. Skośność i kurtoza

Wersje skorygowane, zgodne z `SKEW` i `KURT` w Excelu oraz `.skew()` i `.kurt()` w pandas:

$$
G_1 = \frac{n}{(n-1)(n-2)} \sum \left(\frac{R_t - \bar{R}}{s}\right)^3
$$

$$
G_2 = \frac{n(n+1)}{(n-1)(n-2)(n-3)} \sum \left(\frac{R_t - \bar{R}}{s}\right)^4 - \frac{3(n-1)^2}{(n-2)(n-3)}
$$

Kurtoza jest nadwyżkowa. W rozkładzie normalnym jej wartość teoretyczna wynosi 0, a w próbie może od niej odbiegać. Dodatnia kurtoza wskazuje na większy udział skrajnych wartości w czwartym momencie rozkładu. Ujemna skośność sugeruje asymetrię w stronę ujemnych zwrotów. Funkcja `scipy.stats.kurtosis` domyślnie liczy wersję nieskorygowaną.

Obie miary są wrażliwe na pojedyncze skrajne dni. Przy niezależnych obserwacjach z rozkładu normalnego przybliżone błędy standardowe to $\sqrt{6/n}$ dla skośności i $\sqrt{24/n}$ dla kurtozy, czyli około 0,45 i 0,89 przy 30 zwrotach. W projekcie próba poniżej 365 zwrotów otrzymuje ostrzeżenie. Przy opisie warto sprawdzić, czy znak skośności zmienia się po pominięciu najbardziej skrajnej obserwacji.

### 8.2. Histogram

Histogram pokazuje liczbę dni w przedziałach zwrotów logarytmicznych. Reguła Freedmana-Diaconisa wyznacza wstępną szerokość $h = 2 \cdot \text{IQR} \cdot n^{-1/3}$. Liczbę przedziałów obliczamy jako $\lceil(\max R_t-\min R_t)/h\rceil$ i ograniczamy do zakresu od 20 do 100. Gdy IQR wynosi 0, przyjmujemy 20 przedziałów. IQR to różnica między trzecim a pierwszym kwartylem liczonymi z interpolacją liniową, jak `QUARTILE.INC` w Excelu i `.quantile()` w pandas.

Prawa strona od zera to zwroty dodatnie, a lewa ujemne. Spadek o 10% odpowiada zwrotowi logarytmicznemu około $-0{,}105$. Gęstość rozkładu normalnego o średniej $\bar{R}$ i odchyleniu $s$, pomnożona przez liczbę zwrotów i faktyczną szerokość przedziału, daje skalę porównywalną z wysokością słupków. Krzywą pokazujemy, gdy $s>0$.

## 9. BNB na tle rynku

### 9.1. Korelacja i beta

Na zwrotach logarytmicznych BNB i BTC, tylko dla wspólnych dni:

$$
\rho = \frac{\sum (R_t - \bar{R})(M_t - \bar{M})}{\sqrt{\sum (R_t - \bar{R})^2 \sum (M_t - \bar{M})^2}}
\qquad\qquad
\beta = \frac{\sum (R_t - \bar{R})(M_t - \bar{M})}{\sum (M_t - \bar{M})^2}
$$

Beta to nachylenie regresji $R_t = \alpha + \beta M_t$, w Excelu `SLOPE`. Liczony jest też $R^2 = \rho^2$. Interpretacja:
- znak korelacji określa kierunek zależności. Umownie $|\rho|<0{,}3$ opisujemy jako słaby związek liniowy, od 0,3 do 0,7 jako umiarkowany, a powyżej 0,7 jako silny,
- beta bliska 1 oznacza, że model przewiduje zmianę zwrotu BNB zbliżoną do zmiany zwrotu BTC. Przy becie ujemnej kierunek jest przeciwny. Beta ma postać $\rho \cdot s_{BNB}/s_{BTC}$, więc silne nachylenie może współwystępować ze słabą korelacją,
- $R^2$ to udział wariancji zwrotów BNB wyjaśniany przez model z BTC w badanej próbie. $1-R^2$ to udział wariancji pozostający w resztach modelu.

Kontrolnie liczymy korelację rang Spearmana. Rozbieżność może wynikać z wpływu obserwacji skrajnych lub z nieliniowej zależności monotonicznej. Korelacje są nieokreślone, gdy przynajmniej jeden szereg zwrotów jest stały. Beta jest nieokreślona przy zerowej wariancji zwrotów BTC.

### 9.2. Korelacja krocząca

Korelacja Pearsona w oknie 90 kolejnych dni kalendarzowych z poprawnym dziennym zwrotem dla obu par. Okno może sięgać przed początek wybranego okresu. Brak poprawnego zwrotu w oknie albo zerowa wariancja jednego z szeregów daje pustą wartość i przerwę na wykresie.

### 9.3. Wydarzenia wokół Binance

Analiza zdarzeń sprawdza, czy przy ważnych wydarzeniach cena BNB zachowywała się inaczej, niż wynikałoby z ruchu rynku. Większość wydarzeń dotyczy Changpenga Zhao, założyciela Binance i jej szefa do 2023 roku. SEC to amerykański urząd nadzorujący rynek papierów wartościowych, a CFTC rynek instrumentów pochodnych. Dniem wydarzenia jest dzień UTC, w którym informacja stała się publiczna. Wydarzenia:
- 27.03.2023: CFTC pozywa Binance i Changpenga Zhao,
- 5.06.2023: SEC pozywa Binance i Changpenga Zhao,
- 21.11.2023: ugoda Binance z władzami USA, Changpeng Zhao odchodzi ze stanowiska szefa Binance,
- 30.04.2024: Changpeng Zhao zostaje skazany na 4 miesiące więzienia,
- 27.09.2024: Changpeng Zhao wychodzi na wolność,
- 29.05.2025: SEC umarza sprawę przeciw Binance,
- 23.10.2025: prezydent USA ułaskawia Changpenga Zhao,
- 29.10.2026: atak na Binance. [TRM Labs](https://www.trmlabs.com/resources/blog/north-korea-steals-usd-6-36-billion-from-binance-in-the-largest-hack-in-history) oszacował wartość wypływu środków z portfeli Binance na około 6,36 mld USD. Zamknięcie BNB/USDT spadło z 759,85 USDT 28.10 do 698,46 USDT 29.10, czyli o 8,08%. BTC/USDT spadł o 4,12%, a następnego dnia BNB wzrósł o 0,85%. Zwroty proste i CAR pokazują odpowiednio [notebook 02](notebooks/02-descriptive-stats.ipynb) i [notebook 05](notebooks/05-event-study-and-volume.ipynb).

Część wydarzeń była wcześniej znana albo zapowiadana. Oczekiwania rynku mogą więc znaleźć odzwierciedlenie w cenach przed rozpoczęciem okna zdarzenia. Jest to istotne m.in. dla wyjścia Changpenga Zhao z więzienia, zakończenia sprawy SEC i ułaskawienia.

Procedura:
1. Na dniach od −120 do −11 przed zdarzeniem szacujemy regresję $R_t = \alpha + \beta M_t + \varepsilon_t$. Z maksymalnie 110 dni odrzucamy dni bez wspólnych zwrotów, dni z okien od −5 do +5 innych zdarzeń oraz daty z dzienną zmianą ceny ponad 50% dla BNB lub BTC. $L$ oznacza faktyczną liczbę zwrotów w estymacji. Model wymaga $L \ge 3$ i dodatniej wariancji BTC. Przy $L<30$ oznaczamy próbę jako krótką.
2. W oknach od −1 do +1 i od −5 do +5 liczymy $AR_t = R_t - (\hat{\alpha} + \hat{\beta} M_t)$. Każde okno wymaga poprawnych zwrotów BNB i BTC dla wszystkich dni.
3. Sumujemy zwroty nadzwyczajne $CAR = \sum AR_t$. Wynik podajemy w jednostkach zwrotu logarytmicznego.
4. W błędzie prognozy uwzględniamy niepewność oszacowania parametrów regresji. $T$ oznacza liczbę dni okna zdarzenia, $\bar M_{\mathrm{est}}$ to średni zwrot BTC w próbie estymacyjnej, a $S_{xx}=\sum_{t\in\mathrm{est}}(M_t-\bar M_{\mathrm{est}})^2$. Odchylenie reszt $s_\varepsilon$ obliczamy z dzielnikiem $L-2$.

   $$
   SE(CAR) = s_\varepsilon \sqrt{T + \frac{T^2}{L} +
       \frac{(\sum_{\mathrm{zdarzenie}} M_t - T\bar M_{\mathrm{est}})^2}{S_{xx}}},
   \qquad t = \frac{CAR}{SE(CAR)}
   $$

   Dwustronną wartość $p$ wyznaczamy z rozkładu t z $L-2$ stopniami swobody. Zakładamy niezależne błędy o stałej wariancji i normalnym rozkładzie. Przy zerowym błędzie standardowym $t$ i $p$ pozostają puste. Dla 16 zaplanowanych porównań stosujemy korektę Holma przy poziomie 0,05. Porównania bez wyniku pozostają puste i wliczają się do liczby zaplanowanych testów. Metoda opiera się na [analizie błędu prognozy w modelu rynkowym opisanej przez MacKinlaya](https://www.bu.edu/econ/files/2011/01/MacKinlay-1996-Event-Studies-in-Economics-and-Finance.pdf).
5. Kontrolnie podstawiamy $\alpha=0$, pozostawiając $\hat{\beta}$ z tej samej regresji. $CAR^{(0)}=\sum(R_t-\hat{\beta}M_t)$ sprawdza wrażliwość wyniku na użycie wyrazu wolnego.

Ujemny CAR oznacza, że łączny zwrot BNB był niższy od prognozy modelu z BTC. Gdy zdarzenie oddziałuje również na BTC, benchmark ujmuje część reakcji wspólnej dla obu par. Wyniki pozostają zależne od wyboru benchmarku, okna i założeń o resztach. Wnioski mają charakter opisowy.

## 10. Wolumen

Używany jest wolumen w USDT, czyli pole o indeksie 7 świecy z API, liczonym od 0. Wolumen w USDT rośnie razem z ceną i ma własne trendy, więc korelacja z surowym wolumenem mierzyłaby częściowo wspólny trend. Dlatego używany jest wolumen względny:

$$
V^{\text{wzgl}}_t = \ln\frac{V_t}{\frac{1}{30}\sum_{i=1}^{30} V_{t-i}}
$$

Średnia obejmuje 30 poprzednich dni kalendarzowych, z uwzględnieniem zerowych wartości obrotu. Przy luce, zerowym obrocie w dniu $t$ lub zerowej średniej wolumen względny pozostaje pusty. Korelacje Pearsona i Spearmana wykorzystują dni z poprawnym $V^{\text{wzgl}}_t$ i dziennym zwrotem BNB obliczonym wewnątrz wybranego okresu. Miary mają charakter opisowy i wymagają co najmniej dwóch wspólnych obserwacji oraz zmienności w obu szeregach. Próba poniżej 90 obserwacji otrzymuje ostrzeżenie.

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
