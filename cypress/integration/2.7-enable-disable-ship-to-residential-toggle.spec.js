import { testSetup, updateRetry } from '../support/common/support.js'
import {
  getAppSettings,
  graphql,
  saveAppSetting,
  validateGetAppSettingsResponse,
  validateSaveAppSettingResponse,
} from '../support/graphql_testcase.js'
import { appSetting } from '../support/outputvalidation.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { loadCalculateShippingAPI } from '../support/api_testcase.js'
import { FEDEX_SHIPPING_APP } from '../support/graphql_apps.js'

const prefix = 'Ship To Residential'

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`Get App Settings`, updateRetry(2), () => {
    graphql(FEDEX_SHIPPING_APP, getAppSettings(), (response) => {
      validateGetAppSettingsResponse(response)
      cy.getSettings(response.body)
    })
  })

  it(`${prefix} - Disable Ship to Residential`, updateRetry(3), () => {
    appSetting.residential = false
    cy.readAppSettingsFromJSON().then((sla) => {
      graphql(
        FEDEX_SHIPPING_APP,
        saveAppSetting(appSetting, sla),
        validateSaveAppSettingResponse
      )
    })
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === 'FedEx Ground'
      )

      expect(filtershippingMethod)
        .to.be.an('array')
        .and.to.have.lengthOf.above(0)
    })
  })

  it(`${prefix} - Enable Ship to Residential`, updateRetry(3), () => {
    appSetting.residential = true
    cy.readAppSettingsFromJSON().then((sla) => {
      graphql(
        FEDEX_SHIPPING_APP,
        saveAppSetting(appSetting, sla),
        validateSaveAppSettingResponse
      )
    })
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === 'FedEx Home Delivery'
      )

      expect(filtershippingMethod)
        .to.be.an('array')
        .and.to.have.lengthOf.above(0)
    })
  })
})
