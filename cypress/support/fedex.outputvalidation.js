import { PRODUCTS } from './products'

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
  singleProduct: {
    prefix: 'Single Product',
    postalCode: '33180',
    productName: PRODUCTS.apache,
  },
  multiProduct: {
    prefix: 'Multi Product',
    postalCode: '33180',
    productName1: PRODUCTS.apache,
    productName2: PRODUCTS.amacsa,
  },
}
