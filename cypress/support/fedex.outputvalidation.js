export default {
  appSetting: {
    userCredentialKey: Cypress.env().base.fedex.userCredentialKey,
    userCredentialPassword: Cypress.env().base.fedex.userCredentialPassword,
    parentCredentialKey: '',
    parentCredentialPassword: '',
    clientDetailMeterNumber: Cypress.env().base.fedex.clientDetailMeterNumber,
    clientDetailAccountNumber:
      Cypress.env().base.fedex.clientDetailAccountNumber,
    isLive: false,
    residential: true,
    optimizeShippingType: 0,
    unitWeight: 'KG',
    unitDimension: 'CM',
    packingAccessKey: '',
  },
  slaName: 'FedEx Ground',
  dockId: '1',
}
