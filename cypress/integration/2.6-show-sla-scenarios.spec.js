import {
  preserveCookie,
  updateRetry,
  testSetup,
} from '../support/common/support'
import { appSetting } from '../support/fedex.outputvalidation'
import {
  graphql,
  getAppSettings,
  saveAppSetting,
  validateGetAppSettingsResponse,
  validateSaveAppSettingResponse,
} from '../support/fedex.graphql'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShipping } from '../support/apis'

describe('FedEx UnHide sla scenarios', () => {
  testSetup()

  it(`Get App Settings`, updateRetry(2), () => {
    graphql(getAppSettings(), (response) => {
      validateGetAppSettingsResponse(response)
      cy.getSettings(response.body)
    })
  })

  it(`Unhide sla`, updateRetry(3), () => {
    cy.hideSla(false).then((sla) => {
      graphql(saveAppSetting(appSetting, sla), validateSaveAppSettingResponse)
    })
  })

  // Adding Product to Cart & Verifying shipping method shows
  calculateShipping(data)

  preserveCookie()
})
