import { updateRetry, loginViaCookies } from '../support/common/support.js'
import { appSetting } from '../support/outputvalidation.js'
import { FEDEX_SHIPPING_APP } from '../support/graphql_apps.js'
import {
  graphql,
  saveAppSetting,
  validateSaveAppSettingResponse,
} from '../support/graphql_testcase.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShipping } from '../support/api_testcase.js'

describe('FedEx UnHide sla scenarios', () => {
  loginViaCookies()

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
})
