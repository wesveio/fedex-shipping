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

describe('FedEx Hide sla scenarios', () => {
  testSetup()

  it(`Hide sla`, updateRetry(3), () => {
    cy.hideSla(true).then((sla) => {
      graphql(
        FEDEX_SHIPPING_APP,
        saveAppSetting(appSetting, sla),
        validateSaveAppSettingResponse
      )
    })
  })

  // Adding Product to Cart & Verifying shipping method not shows
  calculateShipping(data)

  preserveCookie()
})
