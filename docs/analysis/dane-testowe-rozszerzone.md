# Rozszerzone dane testowe dla implementacji

Autor: @mmichalskii1

Uzupełnienie rozdziału 11 dokumentu `metody-statystyczne.md` o przykłady liczbowe dla pozostałych metod: średniej wykładniczej EMA, zmienności kroczącej, szerokości przedziałów histogramu, analizy zdarzeń oraz wolumenu względnego. Wszystkie wartości zostały wyliczone w Pythonie 3.12.

## 1. Średnia EMA z 3 dni i zmienność krocząca z oknem 3

Dane wejściowe to pięć cen z rozdziału 11.1: `100`, `110`, `99`, `108.9`, `102.366`.

### Średnia EMA z 3 dni
Współczynnik wygładzania $\alpha = \frac{2}{3+1} = 0{,}5$. Pierwsza wartość to SMA z pierwszych 3 cen:
- dzień 3, cena `99`: `103.0000000000`
- dzień 4, cena `108.9`: `105.9500000000`
- dzień 5, cena `102.366`: `104.1580000000`

### Zmienność krocząca dla okna 3 zwrotów logarytmicznych, dzielnik $n-1$
Pierwsza wartość pojawia się przy czwartej cenie, czyli po 3 zwrotach:
- okno 1, zwroty `R_1, R_2, R_3`: dzienne odchylenie `0.1158572800`, w skali rocznej `2.2134502273`
- okno 2, zwroty `R_2, R_3, R_4`: dzienne odchylenie `0.1055675066`, w skali rocznej `2.0168643809`

## 2. Reguła Freedmana-Diaconisa dla szerokości przedziału histogramu

Na 4 zwrotach logarytmicznych z rozdziału 11.1, czyli `0.0953101798`, `-0.1053605157`, `0.0953101798`, `-0.0618754037`, przy kwartylach z interpolacją liniową jak `QUARTILE.INC` w Excelu i `.quantile()` w pandas:
- $Q_1$: `-0.0727466817`
- $Q_3$: `0.0953101798`
- $\text{IQR} = Q_3 - Q_1$: `0.1680568615`
- szerokość przedziału $h = 2 \cdot \text{IQR} \cdot n^{-1/3}$: `0.2117383774`

Dla $n = 4$ surowa liczba przedziałów wynosi `1`, a w aplikacji jest dodatkowo ograniczana do przedziału od `20` do `100`.

## 3. Analiza zdarzeń

Syntetyczne zwroty logarytmiczne:
- okno estymacji o długości $L = 5$:
  - $M_{\text{est}} = [0.01, -0.02, 0.03, -0.01, 0.02]$
  - $R_{\text{est}} = [0.02, -0.03, 0.05, -0.01, 0.03]$
- okno zdarzenia o długości $T = 3$:
  - $M_{\text{ev}} = [0.01, -0.02, 0.00]$
  - $R_{\text{ev}} = [-0.01, -0.08, 0.01]$

Oczekiwane wyniki regresji z dzielnikiem $L - 2 = 3$ dla $s_\varepsilon$:
- $\hat{\alpha}$: `0.0027906977`
- $\hat{\beta}$: `1.5348837209`
- $s_\varepsilon$: `0.0030499714`

Oczekiwane wyniki w oknie zdarzenia:
- $AR_t$: `[-0.0281395349, -0.0520930233, 0.0072093023]`
- $CAR$: `-0.0730232558`
- błąd standardowy $SE(CAR)$ z uwzględnieniem błędu prognozy: `0.0069922309`
- statystyka $t = CAR / SE(CAR)$: `-10.4434846730`
- dwustronne $p$ przy 3 stopniach swobody, przed korektą Holma: `0.0018740542`
- wariant kontrolny z $\alpha = 0$: $AR^{(0)}_t = [-0.0253488372, -0.0493023256, 0.0100000000]$, $CAR^{(0)} = -0.0646511628`

## 4. Wolumen względny

Dla okna 3 dni poprzedzających i wartości wolumenu w USDT równych `100.0`, `200.0`, `300.0`, `400.0`:
- średnia z 3 poprzednich dni dla dnia 4: `200.0`
- $V^{\text{wzgl}}_4 = \ln(400 / 200) = \ln(2)$: `0.6931471806`

Jeśli wolumen pierwszego dnia wynosi 0, średnia z trzech poprzednich dni wynosi `166.6666666667`, a wolumen względny to $\ln(2{,}4)$, czyli `0.8754687374`. Gdy brakuje któregokolwiek z trzech poprzednich dni, wynik pozostaje pusty.
