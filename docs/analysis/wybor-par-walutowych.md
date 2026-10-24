# Wybór par walutowych do analizy

Autor: @mmichalskii1

## 1. Definicja pary

Cena aktywa na giełdzie jest zawsze wyrażona w innym aktywie. Para BNB/USDT, w API `BNBUSDT`, określa, ile USDT kosztuje 1 BNB. Obie pary w analizie są kwotowane w USDT, więc ceny i zwroty są w tej samej jednostce i można je porównywać. Pary z USDT mają też na Binance największy obrót.

## 2. Wybrane pary

- BNB/USDT: główny przedmiot analizy,
- BTC/USDT: punkt odniesienia dla rynku.

BNB to token Binance. Jego cena zależy od całego rynku i od sytuacji giełdy, co pozwala określić, jaka część ruchu ceny wynika z rynku, a jaka z wydarzeń wokół Binance. BTC to największa kryptowaluta, względem niej liczone są korelacja i beta BNB. Obie pary są notowane od 2017 roku.

## 3. Uwagi

- Okres analizy to od 1 stycznia 2023 do wczoraj, a obie pary mają dane z całego okresu.
- Dane o BNB pochodzą z giełdy, która wyemitowała token, co warto odnotować w wynikach.
- Binance zmienia listę par, więc przed analizą trzeba sprawdzić symbole przez `GET /api/v3/exchangeInfo`.
