export function updateSurchargeRateAndPercentage(rate = 0, percentage = 0) {
  return cy.readFile('.fedexPayload.json').then((items) => {
    const { slaSettings } = items.data.getAppSettings

    for (const ship in slaSettings) {
      slaSettings[ship].surchargeFlatRate = rate
      slaSettings[ship].surchargePercent = percentage
    }

    return slaSettings
  })
}
