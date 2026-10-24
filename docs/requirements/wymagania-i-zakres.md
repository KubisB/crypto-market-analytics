# Wymagania i zakres projektu

Autor: @mmichalskii1

## 1. Cel dokumentu

Dokument określa, co aplikacja ma robić, a co pozostaje poza zakresem. Jest punktem odniesienia przy implementacji, testach i opisie wyników. Szczegóły techniczne znajdują się w `docs/architecture/`.

Wymagania są oznaczone jako konieczne albo dodatkowe, czyli realizowane w miarę czasu.

## 2. Opis produktu

Aplikacja webowa pobiera z Binance historyczne notowania pary BNB/USDT, liczy na nich statystyki i pokazuje wyniki na wykresach.

BNB to token giełdy Binance. Jego cena zależy od całego rynku kryptowalut i od sytuacji samej giełdy, co pozwala sprawdzić, na ile BNB podąża za rynkiem, a na ile zachowuje się niezależnie. Punktem odniesienia rynku jest bitcoin, czyli para BTC/USDT. Jest konieczna do wyliczenia korelacji i bety.

## 3. Zakres

W zakresie:
- pobieranie dziennych świec OHLCV z publicznego API Binance i zapis lokalny,
- walidacja danych i wyliczanie dziennych zwrotów,
- analiza statystyczna z dokumentu `docs/analysis/metody-statystyczne.md`,
- wykresy i tabele w interfejsie webowym w Vue.js z wyborem okresu,
- dokumentacja i testy.

Poza zakresem:
- handel i funkcje wymagające konta lub kluczy API,
- rekomendacje inwestycyjne i prognozowanie cen,
- dane w czasie rzeczywistym,
- logowanie i konta użytkowników,
- giełdy inne niż Binance,
- dane o podaży i spalaniu BNB, niedostępne w rynkowym API.

## 4. Pojęcia

- Para, np. BNB/USDT, to cena 1 BNB wyrażona w USDT. W API zapisywana jako `BNBUSDT`.
- USDT to stablecoin powiązany z dolarem, 1 USDT to około 1 dolar.
- UTC, czyli uniwersalny czas koordynowany, to światowy standard czasu bez zmiany na czas letni. Czas polski to UTC+1 zimą i UTC+2 latem.
- Świeca dzienna to podsumowanie dnia handlu: cena otwarcia, najwyższa, najniższa, zamknięcia i wolumen. Dzień liczony jest w UTC.
- Zwrot to procentowa zmiana ceny z dnia na dzień.
- Zmienność to rozrzut zwrotów, liczony jako odchylenie standardowe.
- Obsunięcie, czyli drawdown, to spadek ceny od ostatniego szczytu.
- Beta określa siłę reakcji BNB na ruchy BTC.

## 5. Użytkownicy

Aplikacja jest dla osób znających podstawy statystyki, ale niekoniecznie rynek kryptowalut, więc przy wskaźnikach są krótkie wyjaśnienia.

## 6. Historyjki użytkownika

### 6.1. Wykres ceny BNB, konieczne

Jako użytkownik chcę zobaczyć, jak zmieniała się cena BNB w wybranym okresie.
- domyślnie okres od 1.01.2023 do wczoraj, wykres ceny zamknięcia,
- po najechaniu na punkt widać datę, ceny OHLC i wolumen,
- opcjonalnie przełączenie na świece japońskie.

Świeca japońska pokazuje jeden dzień jako prostokąt między ceną otwarcia a zamknięcia, z liniami sięgającymi do ceny najwyższej i najniższej. Zielona oznacza dzień wzrostowy, czerwona spadkowy. W odróżnieniu od wykresu liniowego pokazuje też wahania w ciągu dnia.

### 6.2. Wybór okresu, konieczne

Jako użytkownik chcę wybrać zakres dat analizy.
- pola daty od i do oraz skróty: 30 dni, 90 dni, 365 dni i całość od 2023, liczone wstecz od wczoraj,
- nie można wybrać daty wcześniejszej niż 1.01.2023 ani dzisiejszej, bo dzisiejsza świeca nie jest zamknięta,
- przy błędnym zakresie wyświetla się czytelny komunikat,
- zmiana okresu przelicza wszystkie wykresy i statystyki.

### 6.3. Tabela statystyk, konieczne

Jako użytkownik chcę zobaczyć najważniejsze statystyki w jednym miejscu.
- tabela zawiera: liczbę dni, średni zwrot, medianę, odchylenie standardowe dziennych zwrotów logarytmicznych, zmienność roczną, najgorszy i najlepszy dzień z datami, odsetek dni wzrostowych, skośność, kurtozę, maksymalne obsunięcie, zwrot za cały okres i CAGR,
- przy każdej wartości jest krótki opis,
- wyniki są zgodne z obliczeniami kontrolnymi.

### 6.4. Zmienność w czasie, konieczne

Wykres zmienności 30-dniowej. Dni bez pełnego okna nie mają wartości i nie są rysowane jako zero.

### 6.5. Obsunięcia, konieczne

Wykres spadku ceny od szczytu z zaznaczonym maksymalnym obsunięciem: wielkością, datą szczytu, datą dołka i informacją, czy cena wróciła do poziomu szczytu.

### 6.6. Rozkład zwrotów, dodatkowe

Histogram dziennych zwrotów. Wysokość słupka to liczba dni, w których zmiana ceny mieściła się w danym przedziale. Prawa strona od zera to dni wzrostowe, lewa spadkowe. Pod wykresem jest krótkie objaśnienie, jak go czytać.

### 6.7. BNB na tle BTC, konieczne

- wykres zwrotu skumulowanego obu par, oba szeregi startują od 0%,
- korelacja i beta,
- dodatkowo korelacja krocząca 90-dniowa, pokazująca, czy związek z rynkiem jest stały.

### 6.8. Wydarzenia wokół Binance, dodatkowe

Ważne wydarzenia zaznaczone na wykresie ceny oraz tabela z reakcją BNB na tle rynku. Lista wydarzeń i metoda są w dokumencie o metodach.

### 6.9. Wolumen, dodatkowe

Wykres wolumenu pod wykresem ceny oraz zależność między wielkością ruchów ceny a obrotem.

### 6.10. Informacja o BNB, konieczne

Jako użytkownik chcę przeczytać, czym jest BNB, nawet jeśli nie znam rynku kryptowalut.
- opis jest na górze strony, domyślnie zwinięty, rozwija się po kliknięciu,
- wyjaśnia, że BNB to token giełdy Binance, do czego służy, że jego liczba maleje przez spalanie i że cena zależy od rynku oraz od sytuacji giełdy,
- wyjaśnia zapis BNB/USDT i rolę bitcoina jako punktu odniesienia.

### 6.11. Źródło danych, konieczne

Na stronie widać źródło danych, np. „Binance, BNB/USDT, świece dzienne UTC”, datę ostatniego pobrania oraz informację, że projekt jest edukacyjny i nie stanowi porady inwestycyjnej.

## 7. Wymagania dotyczące danych

Szczegóły źródła są w rozdziale 10, a wzory w dokumencie o metodach.
- dane to dzienne świece `BNBUSDT` i `BTCUSDT` z publicznego API Binance, tylko zamknięte, do wczoraj włącznie,
- dane są aktualizowane raz dziennie po północy UTC, a gdy API jest niedostępne, wyświetlane są ostatnie zapisane dane z datą aktualizacji,
- walidacja: brak duplikatów dat, rosnąca kolejność, cena najniższa nie większa od otwarcia i zamknięcia, najwyższa nie mniejsza, ceny dodatnie, a błędne rekordy są odrzucane,
- brakujące dni nie są uzupełniane, a dzienne zmiany ceny powyżej 50% są oznaczane do ręcznej weryfikacji, ale nie usuwane,
- BNB z BTC są porównywane tylko po wspólnych datach,
- gdy danych jest za mało na daną statystykę, wyświetlana jest pusta wartość z przyczyną, a nie błąd.

## 8. Wymagania dla interfejsu

- wykres ceny i wybór okresu na górze, niżej tabela statystyk i pozostałe wykresy,
- widoczne stany ładowania i błędów zamiast pustych wykresów,
- interfejs po polsku, procenty z dwoma miejscami po przecinku, ceny w USDT z dwoma, wolumen w milionach USDT, a wskaźniki bez jednostki, takie jak skośność, kurtoza, korelacja i beta, z czterema,
- działanie w Chrome, Firefoxie i Edge oraz na telefonach,
- stopka ze źródłem danych i zastrzeżeniem o edukacyjnym charakterze projektu.

## 9. Wymagania niefunkcjonalne

- dane zapisane lokalnie ładują się w mniej niż pół sekundy, a pierwsze pobranie pełnej historii może trwać kilka sekund,
- wyniki zgadzają się z obliczeniami kontrolnymi w Pythonie do szóstej cyfry znaczącej, a dane testowe są w dokumencie o metodach,
- wszystkie daty i godziny są w UTC, co widać w interfejsie.

## 10. Źródło danych

### 10.1. Oficjalne API

Dane pochodzą z oficjalnego, publicznego REST API Binance, które nie wymaga konta ani klucza. Dokumentacja: https://developers.binance.com/docs/binance-spot-api-docs/rest-api/market-data-endpoints

Adres bazowy to `https://data-api.binance.vision`, serwer przeznaczony wyłącznie do publicznych danych rynkowych. Zapasowo te same zapytania obsługuje `https://api.binance.com`.

### 10.2. Zapytanie

Endpoint `GET /api/v3/klines` z parametrami:
- `symbol`: `BNBUSDT` lub `BTCUSDT`,
- `interval`: `1d`, wielkość liter ma znaczenie,
- `startTime`, `endTime`: początek i koniec okresu w milisekundach od 1.01.1970 UTC, opcjonalne,
- `limit`: `1000`, domyślnie 500, maksymalnie 1000.

Przykład:

```
GET https://data-api.binance.vision/api/v3/klines?symbol=BNBUSDT&interval=1d&startTime=1791763200000&limit=2
```

Każda świeca w odpowiedzi to tablica 12 wartości:

```
[1791763200000, "754.24000000", "758.03000000", "732.26000000", "732.91000000",
 "108635.60700000", 1791849599999, "81335469.84276000", 509024,
 "45460.00700000", "34047834.79001000", "0"]
```

Używane pola, numerowane od 0:
- 0: czas otwarcia w ms, z którego pochodzi data dnia,
- 1 do 4: ceny otwarcia, najwyższa, najniższa i zamknięcia, przy czym zamknięcie jest podstawą analizy,
- 5: wolumen w walucie bazowej, czyli BNB dla `BNBUSDT` i BTC dla `BTCUSDT`,
- 6: czas zamknięcia w ms, potrzebny do odrzucenia niezamkniętej świecy,
- 7: wolumen w walucie kwotowania, czyli w USDT, używany w analizie wolumenu,
- 8: liczba transakcji, opcjonalnie.

Pozostałe pola są nieużywane. Ceny i wolumeny są tekstem, czasy liczbami.

### 10.3. Sposób pobierania

Okres od 1.01.2023 to około 1390 świec, do tego 200 dni wcześniejszych na rozgrzanie średnich kroczących, czyli 2 zapytania na parę. Kolejne zapytania ustawiają `startTime` na czas otwarcia ostatniej otrzymanej świecy plus jeden dzień, czyli 86 400 000 ms, aż API zwróci mniej niż 1000 świec.

API zwraca też bieżący, niezamknięty dzień. Świeca z czasem zamknięcia późniejszym niż moment pobrania jest odrzucana, więc najświeższe dane są z wczoraj. Świeca zamyka się o północy UTC, czyli o 2:00 czasu polskiego latem i 1:00 zimą, więc wystarczy aktualizacja raz dziennie po tej godzinie.

Aktywność pary sprawdza `GET /api/v3/exchangeInfo?symbol=BNBUSDT`, który powinien zwrócić `"status": "TRADING"`.

### 10.4. Limity

Zapytanie o świece kosztuje 2 punkty wagi, a limit to 6000 punktów na minutę dla adresu IP, więc nie stanowi ograniczenia. Zużycie podaje nagłówek `X-MBX-USED-WEIGHT-1M`. Po przekroczeniu limitu API zwraca 429 z nagłówkiem `Retry-After`. Dalsze zapytania kończą się kodem 418, czyli blokadą IP od 2 minut do 3 dni.

### 10.5. Archiwum plików

Gotowe pliki CSV z kolumnami jak w API są też na https://data.binance.vision. Od 2025 roku czasy w nich są w mikrosekundach.

## 11. Informacje dla zespołu deweloperskiego

Podstawy rynku kryptowalut i konwencje obliczeń. Wzory są w dokumencie o metodach.

### 11.1. Charakterystyka rynku

- Rynek działa całą dobę, 7 dni w tygodniu, więc każdy dzień ma świecę, a brak dnia oznacza lukę w danych.
- Dzień liczony jest w UTC. Świeca z datą 12.10 obejmuje 12.10 od 00:00 do 23:59:59 UTC, czyli w Polsce latem od 2:00 12.10 do 1:59 13.10. Ceną dnia jest cena zamknięcia.
- Ceny par mają różną skalę, BNB kosztuje setki USDT, a BTC dziesiątki tysięcy, dlatego porównywane są zwroty, a nie ceny. Wykres BNB na tle BTC pokazuje zmianę procentową od początku okresu, oba szeregi startują od 0%.
- W analizie używany jest wolumen w USDT, czyli wartość obrotu, bo jest porównywalny między dniami i parami. Wolumen w walucie bazowej, czyli BNB albo BTC, to tylko liczba monet.

### 11.2. Konwencje liczb

- Zwroty i wskaźniki procentowe są ułamkami, np. 0,05, a na procenty zamieniane dopiero przy wyświetlaniu.
- Obsunięcie jest zawsze ujemne albo równe 0, korelacja mieści się między −1 a 1, a beta może być dowolna.
- Z N cen powstaje N−1 zwrotów, pierwszy dzień okresu nie ma zwrotu.
- Zmienność na skalę roczną przelicza się mnożąc przez pierwiastek z 365, a nie przez 365 ani 252.

### 11.3. Wymagana liczba danych

- wykres ceny: 1 dzień,
- statystyki opisowe: 2 zwroty,
- CAGR: okres co najmniej 365 dni,
- zmienność okresu: 2 zwroty, wiarygodnie od 30,
- zmienność krocząca: 31 cen, czyli 30 zwrotów,
- SMA i EMA: tyle cen, ile wynosi okno, czyli 20, 50 lub 200,
- obsunięcie: 2 ceny,
- skośność: 3 zwroty, kurtoza: 4 zwroty, obie wiarygodnie od 30,
- histogram: 2 zwroty, wiarygodnie od 30,
- korelacja i beta z BTC: 2 wspólne zwroty, wiarygodnie od 30,
- korelacja krocząca: 90 wspólnych zwrotów,
- analiza wydarzenia: 120 dni przed wydarzeniem i 1 dzień po nim, a dla opcjonalnego okna od −5 do +5 dni 5 dni po nim,
- wolumen: wiarygodnie od 30.

Poniżej minimum zwracana jest pusta wartość z przyczyną, np. „za mało danych, wymagane 365 dni”. Między minimum a poziomem wiarygodnym wynik jest liczony z ostrzeżeniem.

### 11.4. Wskaźniki kroczące

Wskaźnik kroczący liczony jest osobno dla każdego dnia z ostatnich N dni, łącznie z tym dniem. Dni bez pełnego okna mają pustą wartość, a nie zero, bo zero oznaczałoby dzień bez ruchu ceny.

Żeby wartości były od pierwszego dnia okresu, pobieranych jest dodatkowo 200 dni wcześniejszych, co wystarcza dla SMA200. Służą one tylko do rozgrzania wskaźników, nie są pokazywane na wykresach i nie wchodzą do pozostałych statystyk.

### 11.5. Opisy wskaźników w interfejsie

Propozycje krótkich wyjaśnień:
- średni zwrot: o ile procent średnio zmieniała się cena z dnia na dzień,
- mediana: zwrot typowego dnia, połowa dni miała wynik niższy, a połowa wyższy,
- zmienność roczna: siła wahań ceny w skali roku, wyższa oznacza bardziej nerwowy rynek,
- zmienność 30-dniowa: pokazuje okresy spokoju i paniki,
- maksymalne obsunięcie: największy spadek ceny od szczytu w okresie,
- CAGR: średni roczny zwrot przy stałym tempie zmian,
- skośność: wartość ujemna oznacza przewagę dużych spadków nad dużymi wzrostami,
- kurtoza: wartość powyżej 0 oznacza, że dni skrajne zdarzały się częściej niż w rozkładzie normalnym, w którym wartości skrajne są bardzo rzadkie,
- korelacja z BTC: 1 oznacza pełną zgodność ruchów, 0 brak związku liniowego,
- beta: średnia procentowa zmiana BNB przy zmianie BTC o 1%,
- SMA 200: średnia cena z ostatnich 200 dni, pokazuje długoterminowy trend,
- zwrot nadzwyczajny: część zmiany ceny BNB, której nie wyjaśnia ruch całego rynku.

## 12. Założenia

- API Binance jest dostępne z Polski i nie wymaga klucza dla danych rynkowych,
- cena zamknięcia o północy UTC reprezentuje dany dzień, a USDT jest traktowany jak dolar,
- przeliczenia na skalę roczną używają 365 dni, bo rynek działa codziennie,
- analiza i weryfikacja wyników są wykonywane w Pythonie, w notebookach Jupyter z pandas, numpy, scipy i matplotlib, przechowywanych w `docs/analysis/notebooks/`.
