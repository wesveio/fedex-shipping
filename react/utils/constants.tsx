const fedexHandling: string[] = [
  'BATTERY',
  'HAZARDOUS_MATERIALS',
  'LIMITED_QUANTITIES_COMMODITIES',
  'ORM_D',
  'REPORTABLE_QUANTITIES',
  'SMALL_QUANTITY_EXCEPTION',
  'NONE',
]

const packingOptimization: string[] = [
  'None',
  "Pack all items into largest item's dimension",
  '[BETA] Smart Packing',
]

export { fedexHandling, packingOptimization }
