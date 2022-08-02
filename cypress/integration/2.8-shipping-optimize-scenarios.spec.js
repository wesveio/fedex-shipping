import { testSetup, updateRetry } from '../support/common/support.js'
import {
  getAppSettings,
  graphql,
  saveAppSetting,
  validateGetAppSettingsResponse,
  validateSaveAppSettingResponse,
} from '../support/fedex.graphql.js'
import {
  appSetting,
  smartPackingAccessKey,
} from '../support/fedex.outputvalidation.js'
import { data } from '../fixtures/shippingOptimizePayload.json'
import { loadCalculateShippingAPI } from '../support/apis.js'
import { FEDEX_SHIPPING_APP } from '../support/graphql_apps.js'
import fedexSelectors from '../support/fedex.selectors.js'

const prefix = 'Shipping Optimize'

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`Get App Settings`, updateRetry(2), () => {
    graphql(FEDEX_SHIPPING_APP, getAppSettings(), (response) => {
      validateGetAppSettingsResponse(response)
      cy.getSettings(response.body)
    })
  })

  it.skip(`${prefix} - Generate access key`, updateRetry(2), () => {
    cy.visit('/admin/app/packing-optimization')
    cy.get(fedexSelectors.SmartPackingAccessKey)
      .should('be.visible')
      .clear()
      .type(smartPackingAccessKey)
    cy.contains('Save').click()
    cy.get(fedexSelectors.PickingOptimizeAlert).should(
      'have.text',
      'Successfully Saved'
    )
  })

  it(
    `${prefix} - Select shipping optiomize type None and validate`,
    updateRetry(3),
    () => {
      appSetting.optimizeShippingType = 0
      cy.readAppSettingsFromJSON().then((sla) => {
        graphql(
          FEDEX_SHIPPING_APP,
          saveAppSetting(appSetting, sla),
          validateSaveAppSettingResponse
        )
      })
      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === 'First Overnight'
        )

        expect(filtershippingMethod)
          .to.be.an('array')
          .and.to.have.lengthOf.above(0)
      })
    }
  )

  it(
    `${prefix} - Select shipping optiomize type Pack All In One and validate`,
    updateRetry(3),
    () => {
      appSetting.optimizeShippingType = 1
      cy.readAppSettingsFromJSON().then((sla) => {
        graphql(
          FEDEX_SHIPPING_APP,
          saveAppSetting(appSetting, sla),
          validateSaveAppSettingResponse
        )
      })
      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === 'First Overnight'
        )

        expect(filtershippingMethod)
          .to.be.an('array')
          .and.to.have.lengthOf.above(0)
      })
    }
  )

  it.skip(
    `${prefix} - Select shipping optiomize type Smart Packing and validate`,
    updateRetry(3),
    () => {
      appSetting.optimizeShippingType = 2
      appSetting.packingAccessKey = smartPackingAccessKey
      cy.readAppSettingsFromJSON().then((sla) => {
        graphql(
          FEDEX_SHIPPING_APP,
          saveAppSetting(appSetting, sla),
          validateSaveAppSettingResponse
        )
      })
      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === 'First Overnight'
        )

        expect(filtershippingMethod)
          .to.be.an('array')
          .and.to.have.lengthOf.above(0)
      })
    }
  )
})
