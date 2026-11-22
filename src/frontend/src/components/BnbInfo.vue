<script setup lang="ts">
import { ref } from 'vue'

const open = ref(false)
</script>

<template>
  <details class="bnb-info" :open="open || undefined" @toggle="open = ($event.target as HTMLDetailsElement).open">
    <summary>
      <h2>Czym jest BNB?</h2>
      <span class="hint">Kliknij, aby rozwinąć opis</span>
    </summary>
    <div class="content">
      <p>
        BNB to kryptowaluta powiązana z giełdą Binance, jedną z największych giełd kryptowalut na
        świecie, oraz natywny token sieci BNB Chain. Powstała w 2017 roku, początkowo pod nazwą
        Binance Coin. Jej cena zmienia się przez całą dobę, siedem dni w tygodniu.
      </p>
      <p>
        BNB służy między innymi do opłacania prowizji na giełdzie Binance oraz opłat za transakcje w
        sieci BNB Chain. Część tokenów jest okresowo niszczona w procesie nazywanym spalaniem,
        dlatego ich liczba stopniowo maleje.
      </p>
      <p>
        Cena BNB zależy od sytuacji całego rynku kryptowalut, ale także od wydarzeń dotyczących
        samej giełdy Binance, na przykład postępowań sądowych czy zmian we władzach. Aplikacja
        pokazuje, w jakim stopniu BNB podąża za rynkiem, a w jakim zachowuje się niezależnie.
      </p>
      <p>
        Zapis BNB/USDT oznacza cenę 1 BNB wyrażoną w USDT, czyli w tokenie powiązanym z dolarem
        amerykańskim, wartym około 1 dolara. Punktem odniesienia dla rynku jest bitcoin, największa
        kryptowaluta, czyli para BTC/USDT.
      </p>
    </div>
  </details>
</template>

<style scoped>
.bnb-info {
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 0 16px;
  margin-bottom: 24px;
}
summary {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  padding: 12px 0;
  list-style: none;
}
summary::-webkit-details-marker {
  display: none;
}
summary h2 {
  margin: 0;
  font-size: 1.1rem;
}
.hint {
  color: var(--text-muted);
  font-size: 0.85rem;
}
.content {
  padding-bottom: 16px;
}
.content p {
  margin: 0 0 12px;
  line-height: 1.6;
}
</style>
