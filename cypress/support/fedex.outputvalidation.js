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
  data: {
    items: [
      {
        id: '880090',
        quantity: 1,
        groupId: null,
        unitPrice: 500.0,
        modal: '',
        unitDimension: {
          weight: 10.0,
          height: 10,
          width: 10,
          length: 10,
        },
      },
    ],
    origin: {
      zipCode: '33020',
      country: 'USA',
      state: 'FL',
      city: 'Hollywood',
      coordinates: null,
      residential: false,
    },
    destination: {
      zipCode: '00010002',
      country: 'USA',
      state: null,
      city: null,
      coordinates: null,
      residential: false,
    },
    shippingDateUTC: '2022-05-31T01:02:45.128577+00:00',
    currency: null,
  },
  singleProduct: {
    postalCode: '33180',
    productName: PRODUCTS.apache,
  },
  multiProduct: {
    postalCode: '33180',
    productName1: PRODUCTS.apache,
    productName2: PRODUCTS.amacsa,
  },
}
