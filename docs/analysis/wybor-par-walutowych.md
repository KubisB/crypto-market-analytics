# Wybór par walutowych do analizy

Autor: @mmichalskii1

## 1. Definicja pary

Cena aktywa na giełdzie jest wyrażona w innym aktywie. Para BNB/USDT, w API `BNBUSDT`, określa, ile USDT kosztuje 1 BNB. Obie ceny są kwotowane w USDT, a porównujemy ich względne zmiany, czyli zwroty.

## 2. Wybrane pary

- BNB/USDT: główny przedmiot analizy,
- BTC/USDT: punkt odniesienia dla rynku.

BNB jest powiązany z giełdą Binance i jest natywnym tokenem BNB Chain. BTC służy jako punkt odniesienia. Korelacja i beta opisują zależność między zwrotami obu aktywów. Przy analizie zdarzeń model pozwala oszacować, jak bardzo zwroty BNB odbiegały od oczekiwań opartych na zwrotach BTC. Obie pary są notowane od 2017 roku.

## 3. Uwagi

- Okres analizy to od 1 stycznia 2023 do wczoraj, a obie pary mają dane z całego okresu.
- Dane o BNB pochodzą z giełdy, która wyemitowała token, co warto odnotować w wynikach.
- Binance zmienia listę par, więc przed analizą trzeba sprawdzić symbole przez `GET /api/v3/exchangeInfo`.
