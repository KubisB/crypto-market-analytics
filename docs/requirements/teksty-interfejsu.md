# Teksty interfejsu i lista wydarzeń

Autor: @mmichalskii1

## 1. Cel dokumentu

Dokument zawiera gotowe polskie teksty do interfejsu aplikacji oraz listę wydarzeń do analizy zdarzeń. Teksty są zgodne z dokumentem [`wymagania-i-zakres.md`](wymagania-i-zakres.md) i z dokumentem [`metody-statystyczne.md`](../analysis/metody-statystyczne.md), więc można je kopiować do kodu bez zmian.

Przyjęte konwencje:
- teksty do skopiowania są w cudzysłowach „ ”,
- w nawiasach klamrowych są miejsca na wartości wstawiane przez aplikację, np. `{data}`,
- daty są wyświetlane w formacie dd.mm.rrrr, a godziny w UTC,
- format liczb opisuje rozdział 8 dokumentu wymagań: procenty z dwoma miejscami po przecinku, ceny w USDT z dwoma, wolumen w milionach USDT, a skośność, kurtoza, korelacja i beta z czterema.

## 2. Nagłówek strony

- tytuł strony i karty przeglądarki: „Analiza rynku BNB/USDT”
- podtytuł: „Statystyczna analiza dziennych notowań z giełdy Binance na tle bitcoina”

## 3. Opis BNB

Sekcja na górze strony, domyślnie zwinięta, zgodnie z historyjką 6.10.

- nagłówek: „Czym jest BNB?”
- podpowiedź obok nagłówka: „Kliknij, aby rozwinąć opis”

Treść po rozwinięciu, w czterech akapitach:

„BNB to kryptowaluta powiązana z giełdą Binance, jedną z największych giełd kryptowalut na świecie, oraz natywny token sieci BNB Chain. Powstała w 2017 roku, początkowo pod nazwą Binance Coin. Jej cena zmienia się przez całą dobę, siedem dni w tygodniu.”

„BNB służy między innymi do opłacania prowizji na giełdzie Binance oraz opłat za transakcje w sieci BNB Chain. Część tokenów jest okresowo niszczona w procesie nazywanym spalaniem, dlatego ich liczba stopniowo maleje.”

„Cena BNB zależy od sytuacji całego rynku kryptowalut, ale także od wydarzeń dotyczących samej giełdy Binance, na przykład postępowań sądowych czy zmian we władzach. Aplikacja pokazuje, w jakim stopniu BNB podąża za rynkiem, a w jakim zachowuje się niezależnie.”

„Zapis BNB/USDT oznacza cenę 1 BNB wyrażoną w USDT, czyli w tokenie powiązanym z dolarem amerykańskim, wartym około 1 dolara. Punktem odniesienia dla rynku jest bitcoin, największa kryptowaluta, czyli para BTC/USDT.”

## 4. Wybór okresu

- etykiety pól: „Od”, „Do”
- skróty: „30 dni”, „90 dni”, „365 dni”, „Całość od 2023”
- tekst pod polami: „Daty w UTC. Skróty są liczone wstecz od ostatniego zamkniętego dnia, {data}.”

Komunikaty walidacji:
- puste pole: „Wybierz datę początkową i końcową.”
- data początkowa późniejsza niż końcowa: „Data początkowa nie może być późniejsza niż końcowa.”
- data wcześniejsza niż 1.01.2023: „Najwcześniejsza dostępna data to 01.01.2023.”
- data dzisiejsza lub przyszła: „Nie można wybrać dzisiejszej ani przyszłej daty, ponieważ świeca bieżącego dnia nie jest jeszcze zamknięta. Ostatnia dostępna data to {data}.”

## 5. Wykres ceny

- tytuł: „Cena BNB/USDT”
- przełącznik rodzaju wykresu: „Liniowy”, „Świece japońskie”
- opis świec japońskich przy przełączniku: „Każda świeca to jeden dzień. Prostokąt sięga od ceny otwarcia do ceny zamknięcia, a linie nad nim i pod nim do ceny najwyższej i najniższej. Świeca zielona oznacza wzrost ceny w ciągu dnia, czerwona spadek.”
- legenda średnich kroczących: „SMA 20”, „SMA 50”, „SMA 200”
- opis średnich kroczących: „Średnia cena z ostatnich 20, 50 lub 200 dni. Krótsze średnie pokazują bieżący kierunek ceny, a SMA 200 długoterminowy trend.”
- oś wolumenu: „Wolumen [mln USDT]”

Dymek po najechaniu na dzień:
- „{data}”
- „Otwarcie: {cena} USDT”
- „Najwyższa: {cena} USDT”
- „Najniższa: {cena} USDT”
- „Zamknięcie: {cena} USDT”
- „Zmiana dzienna: {procent}”
- „Wolumen: {wolumen} mln USDT”

## 6. Tabela statystyk

- tytuł: „Statystyki dla wybranego okresu”
- podtytuł: „{data od} do {data do}, {liczba} dni kalendarzowych”

Nazwy wierszy i opisy wyświetlane po najechaniu na znak zapytania:
- „Liczba cen”: „Liczba dni w wybranym okresie, dla których jest poprawna cena zamknięcia.”
- „Liczba zwrotów”: „Liczba dziennych zmian ceny, z których liczone są statystyki. Zmiana powstaje z cen dwóch kolejnych dni, więc zwrotów jest o jeden mniej niż cen, a przy brakującym dniu jeszcze mniej. Od tej liczby zależy wiarygodność pozostałych wartości.”
- „Średni dzienny zwrot”: „O ile procent średnio zmieniała się cena z dnia na dzień. Przy dużych wahaniach może być dodatni, mimo że cena w całym okresie spadła, dlatego wynik okresu pokazuje zwrot za cały okres.”
- „Mediana zwrotu”: „Zwrot typowego dnia. Połowa dni miała wynik niższy, a połowa wyższy.”
- „Odchylenie standardowe dzienne”: „Rozrzut dziennych zmian ceny, liczony na zwrotach logarytmicznych. Im wyższe, tym większe dzienne wahania.”
- „Zmienność roczna”: „Siła wahań ceny w skali roku, czyli odchylenie dzienne pomnożone przez pierwiastek z 365. Wyższa wartość oznacza bardziej nerwowy rynek.”
- „Najgorszy dzień”: „Największy dzienny spadek ceny w wybranym okresie.”
- „Najlepszy dzień”: „Największy dzienny wzrost ceny w wybranym okresie.”
- „Odsetek dni wzrostowych”: „Część dni ze zmianą ceny, w których cena wzrosła. Dni bez zmiany ceny są pomijane.”
- „Skośność”: „Wartość ujemna oznacza, że skrajne spadki były większe niż skrajne wzrosty, czyli dłuższy lewy ogon rozkładu.”
- „Kurtoza”: „Wartość powyżej 0 oznacza, że dni skrajne zdarzały się częściej niż w rozkładzie normalnym, w którym wartości skrajne są bardzo rzadkie.”
- „Maksymalne obsunięcie”: „Największy spadek ceny od szczytu w wybranym okresie.”
- „Zwrot za cały okres”: „O ile procent zmieniła się cena między pierwszym a ostatnim dniem okresu.”
- „Średni roczny zwrot (CAGR)”: „Średnie roczne tempo zmiany ceny przy założeniu stałego tempa zmian. Liczony tylko dla okresów co najmniej rocznych.”

Komunikaty przy wartościach:
- brak wartości: „Za mało danych: wymagane {wymagane}, w wybranym okresie {dostępne}.”, gdzie obie wartości są podawane z jednostką, np. „365 zwrotów” albo „2 ceny”
- CAGR dla okresu krótszego niż rok: „Liczony tylko dla okresów co najmniej rocznych.”
- ostrzeżenie: „Wynik mało wiarygodny: zalecane co najmniej {liczba} zwrotów, w wybranym okresie jest {liczba}.”
- wartość nieokreślona: „Wartość nieokreślona, ponieważ cena w wybranym okresie się nie zmieniała.”
- odsetek dni wzrostowych bez dni ze zmianą ceny: „Brak dni ze zmianą ceny.”
- luka w danych: „Brak danych z {liczba} dni. Zmiany ceny obejmujące te dni są pomijane.”

## 7. Pozostałe wykresy

Zmienność:
- tytuł: „Zmienność 30-dniowa w skali rocznej”
- opis: „Zmienność z ostatnich 30 dni, liczona osobno dla każdego dnia. Pokazuje okresy spokoju i paniki na rynku.”

Obsunięcia:
- tytuł: „Spadek ceny od szczytu”
- opis: „O ile procent cena jest niższa od najwyższej ceny osiągniętej do danego dnia w wybranym okresie. Zero oznacza, że cena jest na szczycie.”
- podsumowanie pod wykresem: „Maksymalne obsunięcie {procent}: szczyt {data}, dołek {data}, {liczba} dni spadku.”
- dalsza część podsumowania, gdy cena wróciła do szczytu: „Cena wróciła do poziomu szczytu {data}.”
- dalsza część podsumowania, gdy nie wróciła: „Cena nie wróciła do poziomu szczytu do końca okresu.”

Rozkład zwrotów:
- tytuł: „Rozkład dziennych zwrotów”
- oś pozioma: „Dzienny zwrot logarytmiczny [%]”
- oś pionowa: „Liczba dni”
- legenda: „Liczba dni”, „Rozkład normalny”
- objaśnienie pod wykresem: „Wysokość słupka to liczba dni, w których zwrot mieścił się w danym przedziale. Słupki na prawo od zera to dni wzrostowe, na lewo spadkowe. Krzywa pokazuje rozkład normalny o tej samej średniej i odchyleniu. Słupki wyższe od krzywej w środku i na brzegach oznaczają, że dni skrajne zdarzały się częściej, niż zakłada rozkład normalny. Przy dużych ruchach zwrot logarytmiczny różni się od zmiany procentowej, np. spadek o 10% to zwrot logarytmiczny −10,5%.”

BNB na tle bitcoina:
- tytuł sekcji: „BNB na tle bitcoina”
- wykres: „Zwrot skumulowany od początku okresu”
- opis wykresu: „Oba szeregi startują od 0%, dzięki czemu można porównać zmiany cen o zupełnie różnej skali.”
- „Korelacja”: „Na ile ruchy BNB i bitcoina idą w tę samą stronę. 1 oznacza pełną zgodność ruchów, 0 brak związku liniowego.”
- „Beta”: „O ile procent średnio zmieniał się BNB, gdy bitcoin zmieniał się o 1%. Przy słabej korelacji to tylko przeciętna, a nie reguła dla pojedynczych dni.”
- „R²”: „Udział wariancji dziennych zwrotów BNB wyjaśniany przez model z bitcoinem. Reszta to ruch specyficzny dla BNB, ale też czynniki, których model nie obejmuje.”
- wykres korelacji kroczącej: „Korelacja z bitcoinem w oknie 90 dni”
- opis: „Pokazuje, czy związek BNB z rynkiem jest stały, czy słabnie w niektórych okresach.”

Wolumen:
- opis: „Obrót w USDT pod wykresem ceny. Zależność jest liczona dla obrotu względnego, czyli obrotu danego dnia na tle średniej z 30 poprzednich dni.”

Wydarzenia wokół Binance:
- tytuł sekcji: „Wydarzenia wokół Binance”
- przełącznik okna: „Okno −1 do +1”, „Okno −5 do +5”
- kolumny tabeli: „Data”, „Wydarzenie”, „Zwrot BNB”, „Zwrot BTC”, „Zwrot nadzwyczajny”, „Istotność”
- wartości kolumny istotności: „istotna”, „nieistotna”, „brak wyniku”
- opis „Zwrot BNB” i „Zwrot BTC”: „Łączna zmiana procentowa ceny w wybranym oknie wydarzenia, liczona od zamknięcia dnia poprzedzającego okno do zamknięcia ostatniego dnia okna.”
- opis „Istotność”: „Test uwzględnia, że sprawdzanych jest kilka wydarzeń naraz. Reakcja jest istotna, gdy przy założeniach modelu trudno ją przypisać zwykłym wahaniom rynku.”
- opis „brak wyniku”: „Za mało danych w oknie wydarzenia lub w okresie przed nim.”
- opis „Zwrot nadzwyczajny”: „Część zmiany ceny BNB w wybranym oknie, której nie wyjaśnia model z BTC, zsumowana w jednostkach zwrotu logarytmicznego.”
- objaśnienie pod tabelą: „Wyniki nie zależą od wybranego okresu, ponieważ każde wydarzenie jest analizowane we własnym oknie czasowym. Dotyczą pojedynczych wydarzeń i mają charakter opisowy. Część wydarzeń była wcześniej zapowiadana, więc brak reakcji ceny nie oznacza, że wydarzenie było nieważne.”

## 8. Stany i komunikaty

- ładowanie przy pierwszym wejściu: „Pobieranie danych z Binance…”
- ładowanie po zmianie okresu: „Przeliczanie…”
- API Binance niedostępne, są zapisane dane: „Nie udało się odświeżyć danych z Binance. Wyświetlane są dane zapisane {data} o {godzina} UTC.”
- API Binance niedostępne, brak zapisanych danych: „Nie udało się pobrać danych z Binance. Spróbuj ponownie za kilka minut.”
- brak połączenia z serwerem aplikacji: „Nie można połączyć się z serwerem aplikacji.”
- błąd nieoczekiwany: „Wystąpił nieoczekiwany błąd. Odśwież stronę i spróbuj ponownie.”
- przycisk przy błędzie: „Spróbuj ponownie”

## 9. Stopka

„Źródło danych: Binance, pary BNB/USDT i BTC/USDT, świece dzienne w UTC. Dane do dnia {data}, ostatnie pobranie {data} o {godzina} UTC.”

„Projekt edukacyjny. Wyniki analiz nie stanowią porady inwestycyjnej.”

## 10. Lista wydarzeń

Wydarzenia pochodzą z rozdziału 9.3 dokumentu metod. Dniem wydarzenia jest dzień UTC, w którym informacja stała się publiczna. Etykieta jest krótkim podpisem na wykresie ceny, a opis trafia do tabeli wydarzeń.

- 27.03.2023, etykieta „Pozew CFTC”: „CFTC pozywa Binance i Changpenga Zhao.”
- 05.06.2023, etykieta „Pozew SEC”: „SEC pozywa Binance i Changpenga Zhao.”
- 21.11.2023, etykieta „Ugoda z USA”: „Ugoda Binance z władzami USA, Changpeng Zhao odchodzi ze stanowiska szefa Binance.”
- 30.04.2024, etykieta „Wyrok”: „Changpeng Zhao zostaje skazany na 4 miesiące więzienia.”
- 27.09.2024, etykieta „Wyjście na wolność”: „Changpeng Zhao wychodzi na wolność.”
- 29.05.2025, etykieta „Umorzenie sprawy SEC”: „SEC umarza sprawę przeciw Binance.”
- 23.10.2025, etykieta „Ułaskawienie”: „Prezydent USA ułaskawia Changpenga Zhao.”

Objaśnienia nazw do dymka przy tabeli:
- „Changpeng Zhao: założyciel Binance i jej szef do 2023 roku.”
- „SEC: amerykański urząd nadzorujący rynek papierów wartościowych.”
- „CFTC: amerykański urząd nadzorujący rynek instrumentów pochodnych.”

Ta sama lista w formacie JSON, do pliku konfiguracyjnego aplikacji:

```json
[
  {
    "id": "cftc-lawsuit",
    "date": "2023-03-27",
    "label": "Pozew CFTC",
    "description": "CFTC pozywa Binance i Changpenga Zhao."
  },
  {
    "id": "sec-lawsuit",
    "date": "2023-06-05",
    "label": "Pozew SEC",
    "description": "SEC pozywa Binance i Changpenga Zhao."
  },
  {
    "id": "us-settlement",
    "date": "2023-11-21",
    "label": "Ugoda z USA",
    "description": "Ugoda Binance z władzami USA, Changpeng Zhao odchodzi ze stanowiska szefa Binance."
  },
  {
    "id": "sentence",
    "date": "2024-04-30",
    "label": "Wyrok",
    "description": "Changpeng Zhao zostaje skazany na 4 miesiące więzienia."
  },
  {
    "id": "release",
    "date": "2024-09-27",
    "label": "Wyjście na wolność",
    "description": "Changpeng Zhao wychodzi na wolność."
  },
  {
    "id": "sec-dismissal",
    "date": "2025-05-29",
    "label": "Umorzenie sprawy SEC",
    "description": "SEC umarza sprawę przeciw Binance."
  },
  {
    "id": "pardon",
    "date": "2025-10-23",
    "label": "Ułaskawienie",
    "description": "Prezydent USA ułaskawia Changpenga Zhao."
  }
]
```
