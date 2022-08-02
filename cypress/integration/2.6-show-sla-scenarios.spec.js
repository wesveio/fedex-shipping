import {
  preserveCookie,
  updateRetry,
  testSetup,
} from '../support/common/support'
import { appSetting } from '../support/fedex.outputvalidation'
import { FEDEX_SHIPPING_APP } from '../support/graphql_apps.js'
import {
  graphql,
  saveAppSetting,
  validateSaveAppSettingResponse,
} from '../support/fedex.graphql'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShipping } from '../support/apis'

describe('FedEx UnHide sla scenarios', () => {
  testSetup()

  it(`Unhide sla`, updateRetry(3), () => {
    cy.hideSla(false).then((sla) => {
      graphql(
        FEDEX_SHIPPING_APP,
        saveAppSetting(appSetting, sla),
        validateSaveAppSettingResponse
      )
    })
  })

  // Adding Product to Cart & Verifying shipping method shows
  calculateShipping(data)

  preserveCookie()
})
