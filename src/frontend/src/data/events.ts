export interface BinanceEvent {
  id: string
  date: string
  label: string
  description: string
}

export const events: BinanceEvent[] = [
  {
    id: 'cftc-lawsuit',
    date: '2023-03-27',
    label: 'Pozew CFTC',
    description: 'CFTC pozywa Binance i Changpenga Zhao.',
  },
  {
    id: 'sec-lawsuit',
    date: '2023-06-05',
    label: 'Pozew SEC',
    description: 'SEC pozywa Binance i Changpenga Zhao.',
  },
  {
    id: 'us-settlement',
    date: '2023-11-21',
    label: 'Ugoda z USA',
    description: 'Ugoda Binance z władzami USA, Changpeng Zhao odchodzi ze stanowiska szefa Binance.',
  },
  {
    id: 'sentence',
    date: '2024-04-30',
    label: 'Wyrok',
    description: 'Changpeng Zhao zostaje skazany na 4 miesiące więzienia.',
  },
  {
    id: 'release',
    date: '2024-09-27',
    label: 'Wyjście na wolność',
    description: 'Changpeng Zhao wychodzi na wolność.',
  },
  {
    id: 'sec-dismissal',
    date: '2025-05-29',
    label: 'Umorzenie sprawy SEC',
    description: 'SEC umarza sprawę przeciw Binance.',
  },
  {
    id: 'pardon',
    date: '2025-10-23',
    label: 'Ułaskawienie',
    description: 'Prezydent USA ułaskawia Changpenga Zhao.',
  },
]
